var http           = require('http');
var EventEmitter   = require('events').EventEmitter;
var assert         = require('assert');
var util           = require('util');
var config         = require('services-library').config;
var logger         = require('services-library').logger;
var mindlinkServer = require('./mindlink_server');

// constructor
function DebugHttpChannel() {
    this.init();
}
util.inherits(DebugHttpChannel, EventEmitter);

// init
DebugHttpChannel.prototype.init = function() {
    this.httpServer = null;                    // http server
    this.config     = config.debugHttpChannel; // configuration
    this.started    = false;                   // started flag
};

// start
DebugHttpChannel.prototype.start = function() {
    logger.debugHttpChannel.debug('start.');
    assert.ok(!this.started);
    this.started = true;
    var self = this;

    // enabled?
    if (!this.config.enable) {
        return;
    }

    // http server
    logger.debugHttpChannel.info('listening on tcp port ' + this.config.port + '.');
    self.httpServer = http.createServer((req, res) => {
        self.emit('request', req, res);
    }).listen(this.config.port, function() {
        self.emit('start');
    });
}

module.exports = new DebugHttpChannel();
