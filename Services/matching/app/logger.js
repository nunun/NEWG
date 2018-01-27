var log4js = require('log4js');
var config = require('./config');

log4js.configure(config.log4js);

module.exports = {
    matchingServer: log4js.getLogger('matchingServer'),
    mindlinkClient: log4js.getLogger('mindlinkClient'),
    test:           log4js.getLogger('test'),
};
