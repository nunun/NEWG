var url            = require('url');
var util           = require('util');
var bodyParser     = require('body-parser');
var config         = require('./services/library/config');
var logger         = require('./services/library/logger');
var mindlinkClient = require('./services/library/mindlink_client').activate(config.mindlinkClient, logger.mindlinkClient);
var webapiServer   = require('./services/library/webapi_server').activate(config.webapiServer, logger.webapiServer);
var routes         = require('./services/protocols/routes');

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

    // setup router
    var router = express.Router();
    var binder = {};
    binder.Test = function(req, res) {
        var resValue = {resValue:15};
        logger.webapiServer.debug("Test: incoming: " + util.inspect(req.body, {depth:null,breakLength:Infinity}));
        logger.webapiServer.debug("Test: outgoing: " + util.inspect(resValue, {depth:null,breakLength:Infinity}));
        res.send(resValue);
    }
    routes.setup(router, binder, null, logger.webapiServer);

    // setup app
    app.use(webapiServer.bodyDecrypter());
    app.use(bodyParser.json());
    app.use(bodyParser.urlencoded({extended: true}));
    app.use(router);
});

// start app ...
mindlinkClient.start();
