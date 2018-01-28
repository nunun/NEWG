var log4js = require('log4js');
var config = require('./config');

log4js.configure(config.log4js);

module.exports = {
    testServer:      log4js.getLogger('testServer'),
    webSocketServer: log4js.getLogger('webSocketServer'),
};
