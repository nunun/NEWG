var log4js = require('log4js');
var config = require('./config');

log4js.configure(config.log4js);

module.exports = {
    testClient:      log4js.getLogger('testClient'),
    webSocketClient: log4js.getLogger('webSocketClient'),
    couchClient:     log4js.getLogger('couchClient'),
    redisClient:     log4js.getLogger('redisClient'),
};
