var fs     = require('fs');
var util   = require('util');
var trim   = require('string.prototype.trim');
var log4js = require('log4js');
var config = require('./config');

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

module.exports = getLogger(config);
