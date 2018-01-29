var config = require('config');

if (process.env.URL) {
    config.webSocketClient.url = process.env.URL;
}
if (process.env.DB_URL) {
    config.couchClient.db = process.env.DB_URL;
}
if (process.env.LOG_LEVEL) {
    config.log4js.categories.testClient.level      = process.env.LOG_LEVEL;
    config.log4js.categories.webSocketClient.level = process.env.LOG_LEVEL;
    config.log4js.categories.couchClient.level     = process.env.LOG_LEVEL;
    config.log4js.categories.redisClient.level     = process.env.LOG_LEVEL;
}

module.exports = config;
