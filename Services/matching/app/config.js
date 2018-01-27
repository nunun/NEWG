var fs     = require('fs');
var trim   = require('string.prototype.trim');
var config = require('config');

if (process.env.PORT) {
    config.matchingServer.port = process.env.PORT;
}
if (process.env.MINDLINK_URL) {
    config.mindlinkClient.url = process.env.MINDLINK_URL;
}
if (process.env.MINDLINK_CONNECT_KEY) {
    config.mindlinkClient.connectKey = process.env.MINDLINK_CONNECT_KEY;
}
if (process.env.MINDLINK_CONNECT_KEY_FILE) {
    config.mindlinkClient.connectKeyFile = process.env.MINDLINK_CONNECT_KEY_FILE;
}
if (process.env.MINDLINK_RETRY_COUNT) {
    config.mindlinkClient.retry.maxCount = process.env.MINDLINK_RETRY_COUNT;
}
if (process.env.MINDLINK_RETRY_INTERVAL) {
    config.mindlinkClient.retry.interval = process.env.MINDLINK_RETRY_INTERVAL;
}
if (process.env.LOG_LEVEL) {
    config.log4js.categories.test.level           = process.env.LOG_LEVEL;
    config.log4js.categories.matchingServer.level = process.env.LOG_LEVEL;
    config.log4js.categories.mindlinkClient.level = process.env.LOG_LEVEL;
    config.log4js.categories.test.level           = process.env.LOG_LEVEL;
}

// NOTE load config.mindlinkClient.connectKey from connectKeyFile
if (config.mindlinkClient.connectKeyFile) {
    config.mindlinkClient.connectKey = trim(fs.readFileSync(config.mindlinkClient.connectKeyFile, 'utf-8'));
}

module.exports = config;
