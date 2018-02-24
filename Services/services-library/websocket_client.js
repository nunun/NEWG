var url            = require('url');
var util           = require('util');
var uuid           = require('uuid/v1');
var trim           = require('string.prototype.trim');
var WebSocket      = require('ws');
var RequestContext = require('./types/request_context');
var Request        = require('./types/request');
var Response       = require('./types/response');

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

// set config
WebSocketClient.prototype.setConfig = function(config) {
    this.config = config;
}

// set logger
WebSocketClient.prototype.setLogger = function(logger) {
    this.logger = logger;
}

// connect
WebSocketClient.prototype.start = function(options) {
    var self = this;
    self.logger.info('start');
    var connectUrl     = self.config.url           || null;
    var connectOptions = self.config.options       || null;
    var retryCount     = self.config.retryCount    || 10;
    var retryInterval  = self.config.retryInterval || 3000;

    // setup
    self.clear();

    // add options and connect options
    if (options || connectOptions) {
        var parsedUrl = url.parse(connectUrl, true);
        if (connectOptions) {
            for (var i in connectOptions) {
                parsedUrl.query[i] = connectOptions[i];
            }
        }
        if (options) {
            for (var i in options) {
                parsedUrl.query[i] = options[i];
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
        var data = JSON.parse(message);
        if (data.requestId && data.requester && data.requester == self.uuid) { // response?
            self.requestContext.setResponse(data.requestId, null, data);
            return;
        }
        if (self.dataEventListener[data.type]) {
            var res = new Response(self, data.type, data.requestId, data.requester, self.logger);
            self.dataEventListener[data.type](data, res);
        } else {
            self.logger.debug('no data event listener for data type[' + data.type + ']');
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
                    self.start(options);
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
    data.type = type;
    var requestId = null;
    if (callback) {
        requestId = this.requestContext.nextRequestId();
        data.requestId = requestId;
        data.requester = this.uuid;
        var request = Request.getRequest(requestId, callback, (timeout || this.config.requestTimeout || 10000));
        this.requestContext.setRequest(request);
    }
    var message = JSON.stringify(data);
    this.ws.send(message);
    return requestId;
}

// cancel request
WebSocketClient.prototype.cancelRequest = function(requestId) {
    this.requestContext.cancelRequest(requestId);
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
    this.dataEventListener[type] = eventListener;
}

// activate
WebSocketClient.activate = function(config, logger) {
    return new WebSocketClient(config, logger);
}

// exports
module.exports = WebSocketClient;
