var NANO            = require('nano');
var util            = require('util');
var assert          = require('assert');
var clientContainer = require('./client_container').activate();

// constructor
function CouchClient(config, logger) {
    this.init(config, logger);
}
util.inherits(CouchClient, function(){});

// init
CouchClient.prototype.init = function(config, logger) {
    this.config = config; // config
    this.logger = logger; // logger

    // event listeners
    this.startEventListener      = null; // start
    this.connectEventListener    = null; // connect
    this.disconnectEventListener = null; // disconnect

    // clear
    this.clear();
};

// clear
CouchClient.prototype.clear = function() {
    // couch client
    this.nano = null; // internal nano connection
};

// start
CouchClient.prototype.start = function() {
    var self = this;
    self.logger.info('start');
    var connectUrl    = self.config.url;
    var retryCount    = self.config.retryCount    || 10;
    var retryInterval = self.config.retryInterval || 3000;

    // setup
    self.clear();

    // create couch connection
    self.nano = NANO({
        url: connectUrl,
        log: function(id, args) {
            self.logger.debug('CouchDB: id[' + JSON.stringify(id) + '] args[' + args +']');
        },
    });

    // check url configuration
    if (!self.nano.config || !self.nano.config.db || self.nano.config.db == "") {
        if (self.disconnectEventListener) {
            self.disconnectEventListener(new Error('no database name to connect. please specify name in configuration url.'));
        }
        return;
    }

    // connect and reconnect
    var currentRetryCount = 0;
    function reconnect(err, force) {
        if (!force && ++currentRetryCount > retryCount) {
            var disconnectEventListener = self.disconnectEventListener;
            self.clear();
            if (disconnectEventListener) {
                disconnectEventListener(err);
            }
            return;
        }
        self.logger.debug('retrying ...');
        setTimeout(connect, retryInterval);
    }
    function connect() {
        self.nano.server.db.get(self.nano.config.db, function(err, body) {
            if (err) {
                if (err.statusCode == 404) {
                    // no database exists. create it.
                    self.logger.debug('create database \'' + self.nano.config.db + '\' ...');
                    self.nano.server.db.create(self.nano.config.db, function(err, body) {
                        if (err) {
                            reconnect(err);
                            return;
                        }
                        reconnect(null, true);
                    });
                    return;
                }
                reconnect(err);
                return;
            }
            currentRetryCount = 0;
            self.logger.debug('use database \'' + self.nano.config.db + '\'.');
            if (self.connectEventListener) {
                self.connectEventListener();
            }
        });
    }
    connect();

    // start
    if (self.startEventListener) {
        self.startEventListener();
    }
}

// stop
//CouchClient.prototype.stop = function() {
//    // NOTE we have no api to stop couch ...?
//}

// get connection
CouchClient.prototype.getConnection = function() {
    return this.nano;
}

// set start event listener
CouchClient.prototype.setStartEventListener = function(eventListener) {
    this.startEventListener = eventListener;
}

// set connect event listener
CouchClient.prototype.setConnectEventListener = function(eventListener) {
    this.connectEventListener = eventListener;
}

// set disconnect event listener
CouchClient.prototype.setDisconnectEventListener = function(eventListener) {
    this.disconnectEventListener = eventListener;
}

// get client
CouchClient.getClient = function(clientName) {
    return clientContainer.find(clientName);
}

// activator
CouchClient.activate = function(config, logger) {
    client = new CouchClient(config, logger);
    if (config) {
        clientContainer.add(config.clientName, client);
    }
    return client;
}

module.exports = CouchClient;
