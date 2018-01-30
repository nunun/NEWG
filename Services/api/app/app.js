var url            = require('url');
var config         = require('libservices').config;
var logger         = require('libservices').logger;
var mindlinkClient = require('libservices').MindlinkClient.activate();
var matchingServer = require('libservices').WebSocketServer.activate();

// mindlink client
mindlinkClient.setConfig(config.mindlinkClient);
mindlinkClient.setLogger(logger.mindlinkClient);
mindlinkClient.on('connect', function() {
    matchingServer.start();
});
//mindlinkClient.on('disconnect', function() {
//});
//mindlinkClient.on('data', function(data) {
//});

// matching server
matchingServer.setConfig(config.matchingServer);
matchingServer.setLogger(logger.matchingServer);
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
