var config         = require('./config');
var logger         = require('./logger');
var matchingServer = require('./matching_server');
var MindlinkClient = require('libmindlink').MindlinkClient;
var mindlinkClient = new MindlinkClient(config.mindlinkClient);

// mindlink client
mindlinkClient.on('connect', function() {
    matchingServer.start();
});
//mindlinkClient.on('data', function(data) {
//    // nothing to do yet.
//});

// matching server
matchingServer.on('start', function() {
    logger.matchingServer.info('start.');
});

mindlinkClient.start();
