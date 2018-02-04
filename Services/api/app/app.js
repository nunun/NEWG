var url            = require('url');
var config         = require('libservices').config;
var logger         = require('libservices').logger;
var mindlinkClient = require('libservices').MindlinkClient.activate(config.mindlinkClient, logger.mindlinkClient);
var protocols      = require('./protocols');

// mindlink client
mindlinkClient.on('connect', function() {
    mindlinkClient.sendState({alias:'api'}, function(err) {
        if (err) {
            logger.mindlinkClient.error(err.toString());
            process.exit(1);
            return;
        }
        logger.mindlinkClient.info('mindlink client initialized.');
        logger.mindlinkClient.info('listeing api requests through mindlink ...');
    });
});
mindlinkClient.on('request', function(data) {
    switch (data.cmd) {
    case protocols.CMD.API.MATCHING_REQUEST:
        mindlinkClient.sendResponse(data, {cmd:protocols.CMD.API.MATCHING_RESPONSE, address:'example.com:80'});
        break;
    }
});

// start app ...
mindlinkClient.start();
