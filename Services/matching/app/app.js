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
    // サーバステータス設定
    statusData.matchingServerState = 'standby';
    statusData.matchingServerUrl   = 'ws://' + config.matchingServer.fqdn
                                   +     ':' + config.matchingServer.port;

    // サーバステータス送信
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
    // サーバステータス更新
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
    matchingAccept(matchingClient);
});
matchingServer.setDisconnectEventListener(function(matchingClient) {
    matchingCancel(matchingClient);
});

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// マッチングキュー
var matchingQueue = [];

// マッチングタイマー
var matchingTimerId = 0;

// ユーザを受け入れ
function matchingAccept(matchingClient, matchingId) {
    MatchingData.getCache(matchingClient.acceptData.matchingId, (err,data) => {
        if (err) {
            matchingAbort(matchingClient, new Error("cache error."));
            return;
        }
        if (!data) {
            matchingAbort(matchingClient, new Error("invalid matchingId."));
            return;
        }
        matchingClient.userId = users[0];
        matchingIdentify(matchingClient);
    });
}

// ユーザを識別
function matchingIdentify(matchingClient) {
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
        matchingEnqueue(matchingClient);
    });
}

// ユーザをキューに追加
function matchingEnqueue(matchingClient) {
    // キューに追加
    matchingQueue.push(matchingClient);

    // 更新ループの起動
    if (!matchingTimerId) {
        setInterval(matchingUpdate, 500); // NOTE 500ms で更新する
    }
}

// マッチング処理
function matchingUpdate() {
    // TODO
    // 更新処理
}

// マッチング中断
function matchingAbort(matchingClient, err) {
    // 即キャンセル
    matchingCancel(matchingClient);

    // メッセージ送信
    matchingClient.send(0, {err:err});

    // 返却後 2 秒で切断
    setTimeout(() => {
        matchingClient.stop();
    }, 2000);
}

// マッチングキャンセル
function matchingCancel(matchingClient) {
    // マッチングキューから外す
    for (i = matchingQueue.length - 1; i >= 0; i--) {
        if (matchingQueue[i] == matchingClient) {
            matchingQueue.splice(i, 1);
        }
    }

    // NOTE
    // 今は必要ないのでコメントアウト
    //mindlinkClient.cancelRequest(matchingClient.requestId);
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// start app ...
couchClient.start();

// NOTE
// ユーザ特定
// 本来であればユーザをチェックして難易度別にサーバを選択。
// 今はシンプルマッチングなのでやらない。
//var userId = data.users[0];
// シンプルマッチング
// サービス一覧からゲームサーバをとって返却
// var cond = ".*{.alias == \"server\" && .serverState == \"ready\" && .load < 1.0}";
// mindlinkClient.sendQuery(cond, function(err,services) {
//     if (err) {
//         res.send({err:err.toString()});
//         return;
//     }
//     if (!services || services.length <= 0) {
//         res.send({err:new Error('no server found.')});
//         return;
//     }
//     var service = services[0];
//     logger.mindlinkClient.debug('server found: service[' + util.inspect(service, {depth:null,breakLength:Infinity}) + ']');
//
//     // サーバ接続情報を返却
//     var matchConnectData = new MatchConnectData();
//     matchConnectData.serverAddress = service.serverAddress;
//     matchConnectData.serverPort    = service.serverPort;
//     matchConnectData.matchId       = null; // NOTE シンプルマッチングのため必要無し
//     matchingClient.send(0, matchConnectData);
//
//     // 返却後 2 秒で切断
//     setTimeout(() => {
//         matchingClient.stop();
//     }, 2000);
// });
