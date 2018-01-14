var log4js = require('log4js');
var config = require('./config');

log4js.configure(config.log4js);

module.exports = {
    test:           log4js.getLogger('test'),
    mindlinkClient: log4js.getLogger('mindlinkClient'),
    appServer:      log4js.getLogger('appServer'),
};
