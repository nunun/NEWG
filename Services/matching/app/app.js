var url            = require('url');
var config         = require('./services/library/config');
var logger         = require('./services/library/logger');
var mindlinkClient = require('./services/library/mindlink_client').activate(config.mindlinkClient, logger.mindlinkClient);
var matchingServer = require('./services/library/websocket_server').activate(config.matchingServer, logger.matchingServer);

// mindlink client
mindlinkClient.setConnectEventListener(function() {
    mindlinkClient.sendStatus({alias:'matching'}, function(err) {
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
matchingServer.setAccepter(function(req) {
    var location = url.parse(req.url, true);
    if (!location.query.user_id) {
        return null;
    }
    return {userId:location.query.user_id};
});
matchingServer.setConnectEventListener(function(matchingClient) {
    // 接続後、すぐにマッチング開始
    var userId = matchingClient.accepted.userId;
    matchingClient.requestId = mindlinkClient.sendToRemote('api', 1 /*protocols.CMD.API.MATCHING_REQUEST*/, {userId:userId}, function(err,responseData) {
        // マッチング結果をレスポンス
        // エラーまたはサーバへのアドレス。
        if (err) {
            matchingClient.send(0, {err:err.toString()});
        } else {
            matchingClient.send(0, {address:responseData.address});
        }
        // NOTE
        // レスポンス後 1 秒で切断
        setTimeout(function() {
            matchingClient.stop();
        }, 1000);
    }, config.matchingServer.timeout);
});
matchingServer.setDisconnectEventListener(function(matchingClient) {
    mindlinkClient.cancelRequest(matchingClient.requestId);
});

// start app ...
mindlinkClient.start();
