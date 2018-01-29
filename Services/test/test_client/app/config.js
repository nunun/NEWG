var config = require('config');

if (process.env.URL) {
    config.webSocketClient.url = process.env.URL;
}
if (process.env.RETRY_COUNT) {
    config.mindlinkClient.retryCount = process.env.RETRY_COUNT;
}
if (process.env.RETRY_INTERVAL) {
    config.mindlinkClient.retryInterval = process.env.RETRY_INTERVAL;
}
if (process.env.REQUEST_TIMEOUT) {
    config.mindlinkClient.requestTimeout = process.env.REQUEST_TIMEOUT;
}
if (process.env.LOG_LEVEL) {
    config.log4js.categories.testClient.level      = process.env.LOG_LEVEL;
    config.log4js.categories.webSocketClient.level = process.env.LOG_LEVEL;
    config.log4js.categories.redisClient.level     = process.env.LOG_LEVEL;
}

module.exports = config;
