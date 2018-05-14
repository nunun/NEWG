var url               = require('url');
var util              = require('util');
var uuid              = require('uuid/v1');
var trim              = require('string.prototype.trim');
var WebSocket         = require('ws');
var RequestContext    = require('./internal_types/request_context');
var Request           = require('./internal_types/request');
var Response          = require('./internal_types/response');
var crypter           = require('./crypter');
var instanceContainer = require('./instance_container').activate();

// constructor
function WebSocketClient(config, logger) {
    this.init(config, logger);
}
util.inherits(WebSocketClient, function(){});

// init
WebSocketClient.prototype.init = function(config, logger) {
    this.config = config; // config
    this.logger = logger; // logger
    this.uuid   = uuid(); // uuid

    // event listeners
    this.startEventListener      = null; // start
    this.connectEventListener    = null; // connect
    this.disconnectEventListener = null; // disconnect
    this.dataEventListener       = {};   // data

    // crypter
    this.crypter = new crypter(config.cryptSetting);

    // clear
    this.clear();
};

// clear
WebSocketClient.prototype.clear = function() {
    if (this.requestContext) {
        this.requestContext.clear();
    }
    if (this.ws) {
        this.ws.terminate();
    }

    // webscoket client
    this.ws         = null;
    this.retryCount = 0;
    this.opened     = false;
    this.closed     = false;

    // request context
    this.requestContext = new RequestContext(this.logger);
}

// connect
WebSocketClient.prototype.start = function(url, queries) {
    var self    = this;
    self.logger.info('start');
    var startUrl     = url;
    var startQueries = queries;
    if (queries == undefined && typeof(url) !== 'string') {
        startUrl     = null;
        startQueries = url;
    }
    var connectUrl     = startUrl || self.config.url           || null;
    var connectQueries =             self.config.queries       || null;
    var retryCount     =             self.config.retryCount    || 10;
    var retryInterval  =             self.config.retryInterval || 3000;

    // setup
    self.clear();

    // add connect queries and start queries
    if (connectQueries || startQueries) {
        var parsedUrl = url.parse(connectUrl, true);
        if (connectQueries) {
            for (var i in connectQueries) {
                parsedUrl.query[i] = connectQueries[i];
            }
        }
        if (startQueries) {
            for (var i in startQueries) {
                parsedUrl.query[i] = startQueries[i];
            }
        }
        connectUrl = url.format(parsedUrl);
    }

    // websocket connection
    self.logger.debug('connecting to \'' + connectUrl + '\'.');
    self.ws = new WebSocket(connectUrl);

    // on open
    self.ws.on('open', function(){
        self.logger.info('on open');
        self.opened = true;
        if (self.connectEventListener) {
            self.connectEventListener();
        }
    });

    // on message
    self.ws.on('message', function(message) {
        self.logger.debug('on message: message[' + message + ']');
        var recvData = JSON.parse(self.crypter.decrypt(message));
        if (recvData.requestId && recvData.response) {
            self.requestContext.setResponse(recvData.requestId, recvData.error, recvData.data);
            return;
        }
        if (self.dataEventListener[recvData.type]) {
            var res = new Response(self, recvData.type, recvData.requestId, self.logger);
            self.dataEventListener[recvData.type](recvData, res);
        } else {
            self.logger.debug('no data event listener for data type[' + recvData.type + ']');
        }
    });

    // on close
    self.ws.on('close', function(code, reason) {
        self.logger.debug('on close: code[' + code + '] reason[' + reason + ']');
        if (!self.opened && !self.closed) {
            self.logger.debug('on close: retrying ...');
            if ((retryCount < 0) || (retryCount > 0 && self.retryCount < retryCount)) {
                self.retryCount += 1;
                setTimeout(function() {
                    self.start(url, queries);
                }, retryInterval);
                return;
            }
            if (retryCount != 0) {
                code   = 9999;
                reason = 'retry exceeded.';
            }
        }
        var disconnectEventListener = self.disconnectEventListener;
        self.clear();
        if (disconnectEventListener) {
            disconnectEventListener(code, reason);
        }
    });

    // on error
    self.ws.on('error', function(err) {
        self.logger.debug('on error: err[' + err + ']');
        self.ws.terminate();
    });

    // start
    if (self.startEventListener) {
        self.startEventListener();
    }
};

// stop
WebSocketClient.prototype.stop = function() {
    this.logger.debug('stop');
    if (!this.ws) {
        return;
    }
    this.closed = true;
    this.ws.close();
}

// send
WebSocketClient.prototype.send = function(type, data, callback, timeout) {
    if (!this.ws) {
        this.logger.debug('send: WehSocket is not opened yet.');
        if (callback) {
            callback(new Error('WebSocket is not opened yet.'), null);
        }
        return;
    }
    this.logger.debug('send: type[' + type + '] data[' + util.inspect(data, {depth:null,breakLength:Infinity}) + '] callback[' + ((callback)? true : false) + '] timeout[' + timeout + ']');
    var sendData = {};
    sendData.type      = type;
    sendData.data      = data;
    sendData.requestId = 0;
    sendData.response  = false;
    sendData.error     = null;
    if (callback) {
        var requestId = this.requestContext.nextRequestId();
        sendData.requestId = requestId;
        var request = Request.rentFromPool(requestId, callback, (timeout || this.config.requestTimeout || 10000));
        this.requestContext.setRequest(request);
    }
    this.sendData(sendData);
    return sendData.requestId;
}

// send data
WebSocketClient.prototype.sendData = function(sendData) {
    var message = this.crypter.encrypt(JSON.stringify(sendData));
    this.ws.send(message);
}

// cancel request
WebSocketClient.prototype.cancelRequest = function(requestId, error) {
    this.requestContext.cancelRequest(requestId, error);
}

// set start event listener
WebSocketClient.prototype.setStartEventListener = function(eventListener) {
    this.startEventListener = eventListener;
}

// set connect event listener
WebSocketClient.prototype.setConnectEventListener = function(eventListener) {
    this.connectEventListener = eventListener;
}

// set disconnect event listener
WebSocketClient.prototype.setDisconnectEventListener = function(eventListener) {
    this.disconnectEventListener = eventListener;
}

// set data event listener
WebSocketClient.prototype.setDataEventListener = function(type, eventListener) {
    if (eventListener == null) {
        delete this.dataEventListener[type];
        return;
    }
    this.dataEventListener[type] = function(recvData, res) {
        eventListener(recvData.data, res);
    };
}

// set data thru event listener
WebSocketClient.prototype.setDataThruEventListener = function(type, eventListener) {
    if (eventListener == null) {
        delete this.dataEventListener[type];
        return;
    }
    this.dataEventListener[type] = eventListener;
}

// get client
WebSocketClient.getClient = function(clientName) {
    return instanceContainer.find(clientName);
}

// activate
WebSocketClient.activate = function(config, logger) {
    var client = new WebSocketClient(config, logger);
    if (config) {
        instanceContainer.add(config.clientName, client);
    }
    return client;
}

// exports
module.exports = WebSocketClient;
