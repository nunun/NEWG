var url              = require('url');
var util             = require('util');
var bodyParser       = require('body-parser');
var config           = require('./services/library/config');
var logger           = require('./services/library/logger');
var mindlinkClient   = require('./services/library/mindlink_client').activate(config.mindlinkClient, logger.mindlinkClient);
var couchClient      = require('./services/library/couch_client').activate(config.couchClient, logger.couchClient);
var redisClient      = require('./services/library/redis_client').activate(config.redisClient, logger.redisClient);
var webapiServer     = require('./services/library/webapi_server').activate(config.webapiServer, logger.webapiServer);
var webapiRoutes     = require('./services/protocols/routes');
var WebAPIController = require('./webapi_controller');

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
    mindlinkClient.sendStatus({alias:'api'}, function(err) {
        if (err) {
            logger.mindlinkClient.error(err.toString());
            process.exit(1);
            return;
        }
        logger.mindlinkClient.info('mindlink client initialized.');
        logger.mindlinkClient.info('listening requests through mindlink ...');
        webapiServer.start();
    });
});
mindlinkClient.setDataFromRemoteEventListener(1 /*protocols.CMD.API.MATCHING_REQUEST*/, function(data, res) {
    // マッチング開始
    // サービス一覧からゲームサーバをとって返却
    // TODO 将来的には人数や空部屋などもチェック。
    mindlinkClient.sendQuery('.*{.address != ""}', function(err,services) {
        if (err) {
            res.send({err:err.toString()});
            return;
        }
        // サービスがあるか確認
        if (services.length <= 0) {
            res.send({err:'no service found.'});
            return;
        }
        // 最初のサービスをとって返却
        var service = services[0];
        logger.mindlinkClient.debug('service found: service[' + util.inspect(service, {depth:null,breakLength:Infinity}) + ']');
        res.send(service);
    });
});

// webapi server
webapiServer.setStartEventListener(function() {
    logger.webapiServer.info('webapi server started.');
});
webapiServer.setSetupEventListener(function(express, app) {
    logger.webapiServer.info('webapi server setup.');

    // express のセットアップ
    app.use(webapiServer.bodyDecrypter());
    app.use(bodyParser.json());
    app.use(bodyParser.urlencoded({extended: true}));

    // WebAPI 経路のセットアップ
    var webapiRouter     = express.Router();
    var webapiController = new WebAPIController();
    webapiRoutes.setup(webapiRouter, webapiController, null, logger.webapiServer);
    app.use(webapiRouter);
});

// start app ...
couchClient.start();
