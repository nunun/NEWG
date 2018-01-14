var util         = require('util');
var EventEmitter = require('events').EventEmitter;
var trim         = require('string.prototype.trim');
var WebSocket    = require('ws');
var logger       = require('./logger');

// constructor
function MindlinkClient(config) {
    this.init(config);
}
util.inherits(MindlinkClient, EventEmitter);

// init
MindlinkClient.prototype.init = function(config) {
    this.ws         = null;   // websocket
    this.config     = config; // configuration
    this.retryCount = 0;      // retry count
    this.joined     = false;  // is joined
};

// connect
MindlinkClient.prototype.start = function() {
    logger.mindlinkClient.debug('start.');
    var self = this;
    var serverName    = self.config.url;
    var retryMaxCount = self.config.retry.maxCount || 0;
    var retryInterval = self.config.retry.interval || 3000;

    // websocket connection
    self.ws = new WebSocket(self.config.url);
    self.emit('connect');

    // on open
    self.ws.on('open', function(){
        logger.mindlinkClient.debug(serverName + ': open');
        self.ws.emit('send', 'join');
    });

    // on send
    self.ws.on('send', function(message) {
        logger.mindlinkClient.debug(serverName + ': send: message[' + message + ']');
        self.ws.send(message);
    });

    // on message
    self.ws.on('message', function(message) {
        logger.mindlinkClient.debug(serverName + ': message: message[' + message.toString() + ']');

        // join
        if (!self.joined) {
            if (message != 'join') {
                self.ws.close();
                return;
            }
            self.joined = true;
            self.emit('join');
            return;
        }

        // data
        var data = JSON.parse(message);
        self.emit('data', data);
    });

    // on close
    self.ws.on('close', function(code, reason) {
        logger.mindlinkClient.debug(serverName + ': close: code[' + code + '] reason[' + reason + ']');
        if (!self.joined) {
            if ((retryMaxCount < 0) || (retryMaxCount > 0 && self.retryCount < retryMaxCount)) {
                self.retryCount += 1;
                setTimeout(function() {
                    self.start();
                }, retryInterval);
                return;
            }
            if (retryMaxCount != 0) {
                code   = 9999;
                reason = 'retry exceeded.';
            }
        }
        if (self.joined) {
            self.emit('leave');
        }
        self.init(self.config);
        self.emit('disconnect', code, reason);
    });

    // on error
    self.ws.on('error', function(err) {
        logger.mindlinkClient.debug(serverName + ': error: err[' + err.message + ']');
    });
};

// stop
MindlinkClient.prototype.stop = function(data) {
    if (!this.ws) {
        return;
    }
    this.ws.close();
}

// send
MindlinkClient.prototype.send = function(data) {
    if (!this.ws) {
        return
    }
    var sendData = JSON.stringify(data);
    this.ws.emit('send', sendData);
}

// exports
module.exports = MindlinkClient;
