var url            = require('url');
var config         = require('libservices').config;
var logger         = require('libservices').logger;
var mindlinkClient = require('libservices').MindlinkClient.activate(config.mindlinkClient, logger.mindlinkClient);
var protocols      = require('./protocols');

// mindlink client
mindlinkClient.on('connect', function() {
    mindlinkClient.sendUpdateRequest({alias:'api'}, function(err) {
        if (err) {
            logger.mindlinkClient.error(err.toString());
            process.exit(1);
            return;
        }
        logger.mindlinkClient.info('mindlink client initialized.');
        logger.mindlinkClient.info('listening requests through mindlink ...');
    });
});
mindlinkClient.on('request', function(data) {
    switch (data.cmd) {
    case protocols.CMD.API.MATCHING_REQUEST:
        mindlinkClient.sendServiceRequest('.*{.address != ""}', function(err,responseData) {
            if (err) {
                mindlinkClient.dispatchResponse(data, {err:err.toString()});
                return;
            }
            var services = responseData.services;
            if (!services || services.length <= 0) {
                mindlinkClient.dispatchResponse(data, {err:'no server found.'});
                return;
            }
            var service = services[0];
            logger.mindlinkClient.debug('service found: service[' + service + ']');
            mindlinkClient.dispatchResponse(data, service);
        });
        break;
    }
});

// start app ...
mindlinkClient.start();
