var url            = require('url');
var config         = require('libservices').config;
var logger         = require('libservices').logger;
var mindlinkClient = require('libservices').MindlinkClient.activate(config.mindlinkClient, logger.mindlinkClient);
var matchingServer = require('libservices').WebSocketServer.activate(config.matchingServer, logger.matchingServer);
var protocols      = require('./protocols');

// mindlink client
mindlinkClient.on('connect', function() {
    mindlinkClient.sendState({alias:'matching'}, function(err) {
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
matchingServer.on('connect', function(matchingClient) {
    var userId = matchingClient.accepted.userId;
    matchingClient.requestId = mindlinkClient.sendMessage('api', {cmd:protocols.CMD.API.MATCHING_REQUEST, userId:userId}, function(err, data) {
        matchingClient.send({err:((err)? err.toString() : null), data:data});
        matchingClient.stop();
    }, config.matchingServer.timeout);
});
matchingServer.on('disconnect', function(matchingClient) {
    mindlinkClient.cancelRequest(matchingClient.requestId);
});

// start app ...
mindlinkClient.start();
