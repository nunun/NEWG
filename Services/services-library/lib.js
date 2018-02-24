var fs     = require('fs');
var util   = require('util');
var trim   = require('string.prototype.trim');
var config = require('config');
var log4js = require('log4js');

// getConfig
// support environment variable expand with [value,  "${ENV}"].
function getConfig(config) {
    if (   Array.isArray(config) && config.length == 2
        && util.isString(config[1]) && config[1].startsWith("${") && config[1].endsWith("}")) {
        config[0] = getConfig(config[0]);
        var envName = config[1].substring(2, config[1].length - 1);
        var envType = typeof(config[0]);
        var isFile  = false;
        if (envName.startsWith("<")) {
            envName = envName.substring(1);
            isFile  = true;
        }
        if (process.env[envName]) {
            var envValue = process.env[envName];
            if (isFile) {
                envValue = trim(fs.readFileSync(envValue, 'utf-8'));
            }
            if (envType == "number") {
                envValue = parseInt(envValue);
            }
            config = envValue;
        } else {
            config = config[0];
        }
    } else if (typeof(config) == "object") {
        for (var i in config) {
            config[i] = getConfig(config[i]);
        }
    }
    return config;
}

// getLogger
// get loggers from config.
function getLogger(config) {
    var logger = {};
    log4js.configure(config.log4js);
    if (config.log4js.categories) {
        for (var i in config.log4js.categories) {
            logger[i] = log4js.getLogger(i);
        }
    }
    return logger;
}

module.exports = {}
module.exports.config          = getConfig(config);
module.exports.logger          = getLogger(module.exports.config);
module.exports.WebSocketServer = require('./websocket_server');
module.exports.WebSocketClient = require('./websocket_client');
module.exports.MindlinkClient  = require('./mindlink_client');
module.exports.CouchClient     = require('./couch_client');
module.exports.RedisClient     = require('./redis_client');
