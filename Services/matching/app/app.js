var url            = require('url');
var config         = require('./config');
var logger         = require('./logger');
var matchingServer = require('libmindlink').WebSocketServer.activate(config.matchingServer, logger.matchingServer);
var mindlinkClient = require('libmindlink').MindlinkClient.activate(config.mindlinkClient, logger.mindlinkClient);

// mindlink client
mindlinkClient.on('connect', function() {
    matchingServer.start();
});
//mindlinkClient.on('disconnect', function() {
//});
//mindlinkClient.on('data', function(data) {
//});

// matching server
matchingServer.setAccepter(function(req) {
    var location = url.parse(req.url, true);
    if (!location.query.ck) {
        return null;
    }
    return location.query.ck;
});
//matchingServer.on('start', function(webSocketClient) {
//});
//matchingServer.on('connect', function(webSocketClient) {
//});
//matchingServer.on('disconnect', function(webSocketClient) {
//});
//matchingServer.on('data', function(webSocketClient, data) {
//});

// start app ...
mindlinkClient.start();
