var url            = require('url');
var util           = require('util');
var config         = require('libservices').config;
var logger         = require('libservices').logger;
var mindlinkClient = require('libservices').MindlinkClient.activate(config.mindlinkClient, logger.mindlinkClient);
var protocols      = require('./protocols');

// mindlink client
mindlinkClient.setConnectEventListener(function() {
    mindlinkClient.sendStatus({alias:'api'}, function(err) {
        if (err) {
            logger.mindlinkClient.error(err.toString());
            process.exit(1);
            return;
        }
        logger.mindlinkClient.info('mindlink client initialized.');
        logger.mindlinkClient.info('listening requests through mindlink ...');
    });
});
mindlinkClient.setDataFromRemoteEventListener(protocols.CMD.API.MATCHING_REQUEST, function(data, res) {
    // マッチング開始
    // サービス一覧からゲームサーバをとって返却
    // TODO 将来的には人数や空部屋などもチェック。
    mindlinkClient.sendQuery('.*{.address != ""}', function(err,responseData) {
        if (err) {
            res.send({err:err.toString()});
            return;
        }
        // サービスがあるか確認
        var services = responseData.services;
        if (!services || services.length <= 0) {
            res.send({err:'no server found.'});
            return;
        }
        // 最初のサービスをとって返却
        var service = services[0];
        logger.mindlinkClient.debug('service found: service[' + util.inspect(service) + ']');
        res.send(service);
    });
});

// start app ...
mindlinkClient.start();
