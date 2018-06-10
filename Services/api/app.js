var url                 = require('url');
var util                = require('util');
var bodyParser          = require('body-parser');
var cors                = require('cors');
var config              = require('./services/library/config');
var logger              = require('./services/library/logger');
var models              = require('./services/protocols/models');
var couchClient         = require('./services/library/couch_client').activate(config.couchClient, logger.couchClient);
var redisClient         = require('./services/library/redis_client').activate(config.redisClient, logger.redisClient);
var mindlinkClient      = require('./services/library/mindlink_client').activate(config.mindlinkClient, logger.mindlinkClient);
var webapiServer        = require('./services/library/webapi_server').activate(config.webapiServer, logger.webapiServer);
var webapiRoutes        = require('./services/protocols/routes');
var WebAPIController    = require('./webapi_controller');
var APIServerStatusData = models.APIServerStatusData;
var statusData          = new APIServerStatusData();

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
    statusData.apiServerState = 'standby';

    // サーバステータス送信
    mindlinkClient.sendStatus(statusData, function(err) {
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

// webapi server
webapiServer.setStartEventListener(function() {
    // サーバステータス更新
    statusData.apiServerState = 'ready';
    mindlinkClient.sendStatus(statusData);
});
webapiServer.setSetupEventListener(function(express, app) {
    // express のセットアップ
    app.use(cors());
    app.use(webapiServer.bodyDecrypter());
    app.use(bodyParser.urlencoded({extended: true}));
    app.use(bodyParser.json());

    // WebAPI 経路のセットアップ
    var webapiRouter     = express.Router();
    var webapiController = new WebAPIController();
    webapiRoutes.setup(webapiRouter, webapiController, null, logger.webapiServer);
    app.use(webapiRouter);
});

// start app ...
couchClient.start();
