var url            = require('url');
var config         = require('libservices').config;
var logger         = require('libservices').logger;
var mindlinkClient = require('libservices').MindlinkClient.activate();

// mindlink client
mindlinkClient.setConfig(config.mindlinkClient);
mindlinkClient.setLogger(logger.mindlinkClient);
//mindlinkClient.on('connect', function() {
//});
//mindlinkClient.on('disconnect', function() {
//});
//mindlinkClient.on('data', function(data) {
//});

// start app ...
mindlinkClient.start();
