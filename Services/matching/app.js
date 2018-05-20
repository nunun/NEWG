var url                       = require('url');
var util                      = require('util');
var config                    = require('./services/library/config');
var logger                    = require('./services/library/logger');
var models                    = require('./services/protocols/models');
var couchClient               = require('./services/library/couch_client').activate(config.couchClient, logger.couchClient);
var redisClient               = require('./services/library/redis_client').activate(config.redisClient, logger.redisClient);
var mindlinkClient            = require('./services/library/mindlink_client').activate(config.mindlinkClient, logger.mindlinkClient);
var matchingServer            = require('./services/library/websocket_server').activate(config.matchingServer, logger.matchingServer);
var UserData                  = models.UserData;
var MatchingData              = models.MatchingData;
var MatchConnectData          = models.MatchConnectData;
var MatchingServerStatusData  = models.MatchingServerStatusData;
var ServerSetupRequestMessage = models.ServerSetupRequestMessage;
var statusData                = new MatchingServerStatusData();

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
    // NOTE
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
mindlinkClient.setDataFromRemoteEventListener(0, (data,res) => {
    serverSetupResponse(res.to, data.matchId);
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
});

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// マッチングキュー
var matchingQueue = [];

// マッチング更新タイマーID
var matchingQueueUpdateTimerId = null;

// マッチング開始
function matchingStart(matchingClient) {
    // マッチングクライアント初期化
    matchingClient.matchingId = matchingClient.acceptData.matchingId;
    matchingClient.userId     = null;
    matchingClient.userData   = null;

    // 情報表示
    logger.matchingServer.debug('start matching with matchingId "' + matchingClient.matchingId + '".');

    // キューに追加
    matchingQueue.push(matchingClient);
    if (!matchingQueueUpdateTimerId) {
        setInterval(matchingUpdate, 500);//500ms更新
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
        matchingClient.userId = data.users[0];
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

    // NOTE
    // ここにマッチング処理を書きこむ。
    // 現在はシンプルマッチングなので、
    // キューの先頭からマッチが成立して
    // 空きサーバに順次接続するようになっている。

    // 最初のユーザを取得する
    var matchingClient = matchingQueue[0];
    if (!matchingClient.userData) {
        return; // ユーザデータ取得中の場合は無視
    }

    // マッチングからは除外 (後戻りできない)
    matchingCancel(matchingClient);

    // 参加開始
    var sceneName = "NetworkProvingGround";//NOTE 仮マップ
    joinStart([matchingClient], sceneName);
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

// 参加キュー
var joinQueue = [];

// 参加キュー更新タイマー
var joinQueueUpdateTimerId = null;

// マッチ番号自動生成用
var matchIdCounter = 0;

// 参加開始
function joinStart(matchingClients, sceneName) {
    // マッチID生成
    var matchId = ++matchIdCounter;

    // マッチングユーザ一覧生成
    var matchingUsers = [];
    for (var i in matchingClients) {
        var matchingClient = matchingClients[i];
        matchingUsers.push(matchingClient.userId);
    }

    // サーバ セットアップ リクエストメッセージ
    var serverSetupRequestMessage = new ServerSetupRequestMessage();
    serverSetupRequestMessage.matchId       = matchId;
    serverSetupRequestMessage.sceneName     = sceneName;
    serverSetupRequestMessage.matchingUsers = matchingUsers;

    // マッチデータ初期化
    var matchData = {};
    matchData.matchId                   = matchId;
    matchData.matchingClients           = matchingClients;
    matchData.serverSetupRequestMessage = serverSetupRequestMessage;
    matchData.service                   = null;
    matchData.waitCount                 = 0;
    matchData.requestCount              = 0;

    // 参加キューに追加
    joinQueue.push(matchData);
    if (!joinQueueUpdateTimerId) {
        setInterval(joinUpdate, 500);//500ms更新
    }

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
        logger.matchingServer.debug('server found: service[' + util.inspect(service, {depth:null,breakLength:Infinity}) + ']');
        matchData.service = service;
    });
}

// 参加更新
function joinUpdate() {
    // 参加キューが空なら何もしない
    if (joinQueue.length <= 0) {
        return;
    }

    // 参加キューを処理
    var abortList = [];
    for (var i in joinQueue) {
        var matchData = joinQueue[i];

        // サービスが確定していない場合は無視
        if (!matchData.service) {
            continue;
        }

        // サーバ セットアップ リクエスト送信
        matchData.waitCount--;
        if (matchData.waitCount <= 0) {
            // 待ちカウンタリセット
            matchData.waitCount = 6;

            // 送信しすぎの場合は中断予約
            matchData.sendCount++;
            if (matchData.sendCount > 3) {
                abortList.push(matchData);
                continue;
            }

            // 送信
            serverSetupRequest(matchData);
        }
    }

    // 中断してるものを処理
    for (var i in abortList) {
        var matchData = abortList[i];
        joinAbort(matchData, new Error('send limit exceeded.'));
    }
}

// サービス参加リクエスト
function serverSetupRequest(matchData) {
    mindlinkClient.sendToRemote(matchData.service.clientUuid, 0, matchData.serverSetupRequestMessage);
}

// サービス参加レスポンス
function serverSetupResponse(from, matchId) {
    // サーバがセットアップされたが根拠となるデータが既に無い
    // その場合はサーバ側に通知して、サーバを終了する。
    var foundMatchData = null;
    for (var i in joinQueue) {
        var matchData = joinQueue[i];
        if (matchData.matchId == matchId) {
            foundMatchData = matchData;
            break;
        }
    }
    if (!foundMatchData) {
        var errorMessage = 'matchId not found? (' + matchId + ')';
        logger.matchingServer.debug(errorMessage);
        mindlinkClient.sendToRemote(from, 0, {err:new Error(errorMessage)});
        return;
    }

    // 参加成功！
    joinReady(foundMatchData, foundMatchData.service.serverAddress, foundMatchData.service.serverPort);
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
    // 即キャンセル
    joinCancel(matchData);

    // メッセージ送信
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

// 参加キャンセル
function joinCancel(matchData) {
    for (i = joinQueue.length - 1; i >= 0; i--) {
        if (joinQueue[i] == matchData) {
            joinQueue.splice(i, 1);
        }
    }
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// start app ...
couchClient.start();