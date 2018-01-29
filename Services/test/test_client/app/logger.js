var log4js = require('log4js');
var config = require('./config');

log4js.configure(config.log4js);

module.exports = {
    testClient:      log4js.getLogger('testClient'),
    webSocketClient: log4js.getLogger('webSocketClient'),
    redisClient:     log4js.getLogger('redisClient'),
};
