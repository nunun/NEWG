var url              = require('url');
var config           = require('./services/library/config');
var logger           = require('./services/library/logger');
var models           = require('./services/protocols/models');
var mindlinkClient   = require('./services/library/mindlink_client').activate(config.mindlinkClient, logger.mindlinkClient);
var matchingServer   = require('./services/library/websocket_server').activate(config.matchingServer, logger.matchingServer);
var MatchData        = models.MatchConnectData;
var MatchConnectData = models.MatchConnectData;

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
    // マッチングID を検索
    var matchingId = matchingClient.acceptData.matchingId;
    MatchingData.getCache(matchingId, (err,data) => {
        // ユーザ特定
        var userId = data.users[0];

        // TODO
        // 本来であれば、マッチングキューに追加してマッチングするが、
        // 現在はダミーマッチングのためやらない。

        // ダミーマッチング
        // サービス一覧からゲームサーバをとって返却
        var cond = ".*{.alias == \"server\" && .serverState == \"ready\" && .serverAddress != "" && .serverPort > 0 && .load < 1.0}";
        mindlinkClient.sendQuery(cond, function(err,services) {
            if (err) {
                res.send({err:err.toString()});
                return;
            }

            // サービスがあるか確認
            if (services.length <= 0) {
                res.send({err:new Error('server full.')});
                return;
            }

            // サービス発見
            // ただし常にリストの最初のサービスを利用
            var service = services[0];
            logger.mindlinkClient.debug('service found: service[' + util.inspect(service, {depth:null,breakLength:Infinity}) + ']');

            // サーバ接続情報を返却
            var matchConnectData = new MatchConnectData();
            matchConnectData.serverAddress = service.serverAddress;
            matchConnectData.serverPort    = service.serverPort;
            matchConnectData.matchId       = null; // NOTE ダミーマッチングのため必要無し
            matchingClient.send(0, matchConnectData);

            // 返却後 3 秒で切断
            setTimeout(() => {
                matchingClient.stop();
            }, 3000);
        });
    });
});
matchingServer.setDisconnectEventListener(function(matchingClient) {
    mindlinkClient.cancelRequest(matchingClient.requestId);
});

// 開始
mindlinkClient.start();
