var url            = require('url');
var config         = require('libservices').config;
var logger         = require('libservices').logger;
var mindlinkClient = require('libservices').MindlinkClient.activate(config.mindlinkClient, logger.mindlinkClient);
var protocols      = require('./protocols');

// mindlink client
mindlinkClient.setConnectEventListener(function() {
    mindlinkClient.requestUpdate({alias:'api'}, function(err) {
        if (err) {
            logger.mindlinkClient.error(err.toString());
            process.exit(1);
            return;
        }
        logger.mindlinkClient.info('mindlink client initialized.');
        logger.mindlinkClient.info('listening requests through mindlink ...');
    });
});
mindlinkClient.setRequestFromRemoteEventListener(protocols.CMD.API.MATCHING_REQUEST, function(data, sender) {
    // マッチング開始
    // サービス一覧からゲームサーバをとって返却
    // TODO 将来的には人数や空部屋などもチェック。
    mindlinkClient.requestServices('.*{.address != ""}', function(err,responseData) {
        if (err) {
            mindlinkClient.responseToRemote(sender, {err:err.toString()});
            return;
        }
        // サービスがあるか確認
        var services = responseData.services;
        if (!services || services.length <= 0) {
            mindlinkClient.responseToRemote(sender, {err:'no server found.'});
            return;
        }
        // 最初のサービスをとって返却
        var service = services[0];
        logger.mindlinkClient.debug('service found: service[' + service + ']');
        mindlinkClient.responseToRemote(sender, service);
    });
});

// start app ...
mindlinkClient.start();
