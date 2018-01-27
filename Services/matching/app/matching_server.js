var net          = require('net');
var EventEmitter = require('events').EventEmitter;
var assert       = require('assert');
var util         = require('util');
var config       = require('./config');
var logger       = require('./logger');

// constructor
function MatchingServer(port) {
    this.init();
}
util.inherits(MatchingServer, EventEmitter);

// init
MatchingServer.prototype.init = function() {
    this.tcpServer = null; // tcp server
    this.port      = null; // server port
};

// start
MatchingServer.prototype.start = function(port) {
    logger.matchingServer.debug('start.');
    var self = this;
    self.port = config.matchingServer.port;

    // tcp server
    logger.matchingServer.info('listening on tcp port ' + self.port + '.');
    self.tcpServer = net.createServer(function(client) {
        var clientName = client.remoteAddress + ':' + client.remotePort;
        logger.matchingServer.info('client connected. (' + clientName + ')');
    }).listen(self.port, function() {
        logger.matchingServer.info('listen started.');
        self.emit('start');
    });
}

module.exports = new MatchingServer();
