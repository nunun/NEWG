var url                      = require('url');
var util                     = require('util');
var config                   = require('./services/library/config');
var logger                   = require('./services/library/logger');
var models                   = require('./services/protocols/models');
var couchClient              = require('./services/library/couch_client').activate(config.couchClient, logger.couchClient);
var redisClient              = require('./services/library/redis_client').activate(config.redisClient, logger.redisClient);
var mindlinkClient           = require('./services/library/mindlink_client').activate(config.mindlinkClient, logger.mindlinkClient);
var matchingServer           = require('./services/library/websocket_server').activate(config.matchingServer, logger.matchingServer);
var UserData                 = models.UserData;
var MatchingData             = models.MatchingData;
var MatchConnectData         = models.MatchConnectData;
var MatchingServerStatusData = models.MatchingServerStatusData;
var statusData               = new MatchingServerStatusData();

// couch client
couchClient.setConnectEventListener(function() {
    redisClient.start();
});

// redis client
redisClient.setConnectEventListener(function() {
    mindlinkClient.start();
});

// mindlink client
mindlinkClient.setConnectEventListener(function() {
    // マッチングサーバステータス設定
    statusData.matchingServerState = 'standby';
    statusData.matchingServerUrl   = 'ws://' + config.matchingServer.fqdn
                                   +     ':' + config.matchingServer.port;

    // マッチングサーバステータス送信
    mindlinkClient.sendStatus(statusData, function(err) {
        if (err) {
            logger.mindlinkClient.error(err.toString());
            process.exit(1);
            return;
        }
        logger.mindlinkClient.info('mindlink client initialized.');
        logger.mindlinkClient.info('starting matching server ...');
        matchingServer.start();
    });
});

// matching server
matchingServer.setStartEventListener(function() {
    // マッチングサーバステータス更新
    statusData.matchingServerState = 'ready';
    mindlinkClient.sendStatus(statusData);
});
matchingServer.setAcceptEventListener(function(req) {
    // クエリから マッチングID を確認, なければ弾く
    var location = url.parse(req.url, true);
    if (!location.query.matchingId) {
        return null;
    }
    // マッチングID を確認
    return {matchingId:location.query.matchingId};
});
matchingServer.setConnectEventListener(function(matchingClient) {
    matchingStart(matchingClient);
});
matchingServer.setDisconnectEventListener(function(matchingClient) {
    matchingCancel(matchingClient);

    // NOTE
    // 今は必要ないのでコメントアウト
    //mindlinkClient.cancelRequest(matchingClient.requestId);
});

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// マッチングキュー
var matchingQueue = [];

// マッチング更新タイマーID
var matchingUpdateTimerId = 0;

// マッチング開始
function matchingStart(matchingClient) {
    // キューに追加
    matchingQueue.push(matchingClient);

    // 更新ループの起動
    if (!matchingUpdateTimerId) {
        setInterval(matchingUpdate, 100); // NOTE 500ms で更新する
    }

    // ユーザ特定
    matchingIdentifyParameter(matchingClient);
}

// マッチングパラメータ確認
function matchingIdentifyParameter(matchingClient) {
    MatchingData.getCache(matchingClient.matchingId, (err,data) => {
        if (err) {
            matchingAbort(matchingClient, new Error("cache error."));
            return;
        }
        if (!data) {
            matchingAbort(matchingClient, new Error("invalid matchingId."));
            return;
        }
        matchingClient.userId = users[0];
        matchingIdentifyUser(matchingClient);
    });
}

// ユーザを識別
function matchingIdentifyUser(matchingClient) {
    UserData.get(matchingClient.userId, (err,userData) => {
        if (err) {
            matchingAbort(matchingClient, err);
            return;
        }
        if (!userData) {
            matchingAbort(matchingClient, new Error("invalid user."));
            return;
        }
        matchingClient.userData = userData;
    });
}

// マッチング更新処理
function matchingUpdate() {
    // マッチングキューが空なら何もしない
    if (matchingQueue.length <= 0) {
        return;
    }

    // 最初のユーザを取得する
    var matchingClient = matchingQueue[0];
    if (!matchingClient.userData) {
        return; // ユーザデータ取得中の場合は無視
    }

    // NOTE
    // マッチングからは除外 (後戻りできない)
    matchingCancel(matchingClient);

    // NOTE
    // ダミーマッチングなので
    // 最初の人から必ずマッチが確定
    joinStart(matchingClient);
}

// マッチング終了
function matchingClose(matchingClient, data) {
    // 即キャンセル
    matchingCancel(matchingClient);

    // メッセージ送信
    matchingClient.send(0, data);

    // 返却後 2 秒で切断
    setTimeout(() => {
        matchingClient.stop();
    }, 2000);
}

// マッチング中断
function matchingAbort(matchingClient, err) {
    matchingClose(matchingClient, {err:err});
}

// マッチングキャンセル
function matchingCancel(matchingClient) {
    for (i = matchingQueue.length - 1; i >= 0; i--) {
        if (matchingQueue[i] == matchingClient) {
            matchingQueue.splice(i, 1);
        }
    }
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// マッチ番号 (自動生成用)
var matchIdCounter = 0;

// 参加開始
function joinStart(matchingClient) {
    // マッチデータ (仮)
    var matchData = {};
    matchData.matchId         = ++matchIdCounter;
    matchData.matchingClients = [matchingClient];

    // サービス検索
    joinSearch(matchData);
}

// 参加できそうなサービスを検索
function joinSearch(matchData) {
    var cond = ".*{.alias == \"server\" && .serverState == \"standby\" && .load < 1.0}";
    mindlinkClient.sendQuery(cond, function(err,services) {
        if (err) {
            joinAbort(matchData, err);
            return;
        }
        if (!services || services.length <= 0) {
            joinAbort(matchData, new Error('no server found.'));
            return;
        }
        var service = services[0];
        logger.mindlinkClient.debug('server found: service[' + util.inspect(service, {depth:null,breakLength:Infinity}) + ']');
        joinConfirm(matchData, service);
    });
}

// 参加できそうなサービスに参加を確認
function joinConfirm(matchData, service) {
    // TODO
    // 参加可能かサーバに確認する
    // サーバの初期化が終わってから入れるので...
    //joinReady(matchData, service.serverAddress, service.serverPort);
}

// 参加可能
function joinReady(matchData, serverAddress, serverPort) {
    var matchConnectData = new MatchConnectData();
    matchConnectData.serverAddress = serverAddress;
    matchConnectData.serverPort    = serverPort;
    matchConnectData.matchId       = matchData.matchId;
    joinClose(matchData, matchConnectData);
}

// 参加終了
function joinClose(matchData, data) {
    var matchingClients = matchData.matchingClients;
    for (var i in matchingClients) {
        var matchingClient = matchingClients[i];
        matchingClose(matchingClient, data);
    }
    matchingClients.splice(0, matchingClients.length);
}

// 参加中断
function joinAbort(matchData, err) {
    joinClose(matchData, {err:err});
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// start app ...
couchClient.start();
