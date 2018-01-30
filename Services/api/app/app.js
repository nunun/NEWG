var url            = require('url');
var config         = require('libservices').config;
var logger         = require('libservices').logger;
var mindlinkClient = require('libservices').MindlinkClient.activate();

// mindlink client
mindlinkClient.setConfig(config.mindlinkClient);
mindlinkClient.setLogger(logger.mindlinkClient);
mindlinkClient.on('request', function(data) {
    switch (data.type) {
    case 100: // [server] match start.
        mindlinkClient.response(data, {param:'hoge'});
        break;
    //case 101: // [matching] join user.
    //  break;
    //case 102: // [matching] get user.
    //  break;
    }
});

// start app ...
mindlinkClient.start();
