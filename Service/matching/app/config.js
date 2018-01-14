var config = require('config');

if (process.env.LOG_LEVEL) {
    config.log4js.categories.test.level           = process.env.LOG_LEVEL;
    config.log4js.categories.mindlinkClient.level = process.env.LOG_LEVEL;
    config.log4js.categories.appServer.level      = process.env.LOG_LEVEL;
}

if (process.env.MINDLINK_URL) {
    config.mindlinkClient1.url = process.env.MINDLINK_URL;
}
if (process.env.MINDLINK_RETRY_COUNT) {
    config.mindlinkClient1.retry.maxCount = process.env.MINDLINK_RETRY_COUNT;
}
if (process.env.MINDLINK_RETRY_INTERVAL) {
    config.mindlinkClient1.retry.interval = process.env.MINDLINK_RETRY_INTERVAL;
}

module.exports = config;
