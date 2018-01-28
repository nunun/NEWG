var config = require('config');

if (process.env.PORT) {
    config.webSocketServer.port = process.env.PORT;
}
if (process.env.LOG_LEVEL) {
    config.log4js.categories.testServer.level      = process.env.LOG_LEVEL;
    config.log4js.categories.webSocketServer.level = process.env.LOG_LEVEL;
}

module.exports = config;
