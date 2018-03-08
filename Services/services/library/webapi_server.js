var url     = require('url');
var util    = require('util');
var uuid    = require('uuid/v1');
var trim    = require('string.prototype.trim');
var express = require('express');
var Crypter = require('./crypter');

// constructor
function WebAPIServer(config, logger) {
    this.init(config, logger);
}
util.inherits(WebAPIServer, function(){});

// init
WebAPIServer.prototype.init = function(config, logger) {
    this.config = config;   // config
    this.logger = logger;   // logger
    this.app    = null;     // webapp
    this.uuid   = uuid();   // server uuid

    // event listeners
    this.startEventListener = null;
    this.stopEventListener  = null;
    this.setupEventListener = null;

    // crypter
    this.crypter = new Crypter(config.cryptSetting);

    // clear
    this.clear();
};

// clear
WebAPIServer.prototype.clear = function() {
    if (this.app) {
        this.app.close();
    }

    // webapi server
    this.app = null; // webapp
};

// start
WebAPIServer.prototype.start = function() {
    var self = this;
    self.logger.debug('start');

    // setup
    self.clear();

    // websocket server
    self.logger.info('start: listening on port ' + self.config.port + '.');
    self.app = express();

    // setup
    if (self.setupEventListener) {
        self.setupEventListener(express, self.app, self.config, self.logger);
    }

    // listen
    self.app.listen(self.config.port, function() {
        self.logger.debug('on listening');
        if (self.startEventListener) {
            self.startEventListener();
        }
    });
}

// stop
WebAPIServer.prototype.stop = function() {
    this.logger.debug('stop');
    if (!this.app) {
        return;
    }
    var stopEventListener = this.stopEventListener;
    this.clear();
    if (stopEventListener) {
        stopEventListener();
    }
}

// create express middleware 'bodyDecrypter'
WebAPIServer.prototype.bodyDecrypter = function() {
    var self = this;
    return function(req, res, next) {
        req.body = self.crypter.decrypt(req.body);
        next();
    }
}

// start event listener
WebAPIServer.prototype.setStartEventListener = function(eventListener) {
    this.startEventListener = eventListener;
}

// stop event listener
WebAPIServer.prototype.setStopEventListener = function(eventListener) {
    this.stopEventListener = eventListener;
}

// setup event listener
WebAPIServer.prototype.setSetupEventListener = function(eventListener) {
    this.setupEventListener = eventListener;
}

// activator
WebAPIServer.activate = function(config, logger) {
    return new WebAPIServer(config, logger);
}

// exports
module.exports = WebAPIServer;
