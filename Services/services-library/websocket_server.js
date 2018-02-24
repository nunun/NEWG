var url            = require('url');
var util           = require('util');
var uuid           = require('uuid/v1');
var trim           = require('string.prototype.trim');
var WebSocket      = require('ws');
var RequestContext = require('./types/request_context');
var Response       = require('./types/response');

// constructor
function WebSocketServer(config, logger, acceptor) {
    this.init(config, logger, acceptor);
}
util.inherits(WebSocketServer, function(){});

// init
WebSocketServer.prototype.init = function(config, logger, acceptor) {
    this.config   = config;   // config
    this.logger   = logger;   // logger
    this.acceptor = acceptor; // connection acceptor
    this.uuid     = uuid();   // server uuid

    // event listeners
    this.startEventListener      = null;
    this.stopEventListener       = null;
    this.connectEventListener    = null;
    this.disconnectEventListener = null;
    this.dataEventListener       = {};

    // clear
    this.clear();
};

// clear
WebSocketServer.prototype.clear = function() {
    if (this.requestContext) {
        this.requestContext.clear();
    }
    if (this.wss) {
        this.wss.close();
    }

    // webscoket server
    this.wss = null; // websocket

    // request context
    this.requestContext = new RequestContext(this.logger);
};

// set config
WebSocketServer.prototype.setConfig = function(config) {
    this.config = config;
}

// set logger
WebSocketServer.prototype.setLogger = function(logger) {
    this.logger = logger;
}

// set acceptor
WebSocketServer.prototype.setAccepter = function(acceptor) {
    this.acceptor = acceptor;
}

// start
WebSocketServer.prototype.start = function() {
    var self = this;
    self.logger.debug('start');

    // setup
    self.clear();

    // websocket server
    self.logger.info('start: listening on port ' + self.config.port + '.');
    self.wss = new WebSocket.Server({port:self.config.port});

    // on connect
    self.wss.on('connection', function(ws, req) {
        var clientName = req.connection.remoteAddress;
        self.logger.info('on connection: ' + clientName + ': websocket client connected.');

        // acceptor
        var accepted = null;
        if (self.acceptor) {
            accepted = self.acceptor(req);
            if (!accepted) {
                self.logger.info('on connection: ' + clientName + ': denied by acceptor!');
                ws.terminate();
                return;
            }
        }

        // websocket client
        var webSocketClient = {};
        webSocketClient.ws       = ws;
        webSocketClient.accepted = accepted;

        // send
        webSocketClient.send = function(type, data, callback, timeout) {
            if (!webSocketClient.ws) {
                self.logger.debug('send: WehSocket is not opened yet.');
                if (callback) {
                    callback(new Error('WebSocket is not opened yet.'), null);
                }
                return;
            }
            self.logger.debug('send: type[' + type + '] data[' + util.inspect(data) + '] callback[' + ((callback)? true : false) + '] timeout[' + timeout + ']');
            data.type = type;
            var requestId = null;
            if (callback) {
                requestId = this.requestContext.nextRequestId();
                data.requestId = requestId;
                data.requester = self.uuid;
                var request = Request.getRequest(requestId, callback, (timeout || this.config.requestTimeout || 10000));
                this.requestContext.setRequest(request);
            }
            var message = JSON.stringify(data);
            webSocketClient.ws.send(message);
            return requestId;
        }

        // stop
        webSocketClient.stop = function() {
            webSocketClient.ws.terminate();
        }

        // on message
        webSocketClient.ws.on('message', function(message){
            self.logger.debug('on message: ' + clientName + ': ' + self.uuid + ': ' + webSocketClient.uuid + ': message: message[' + util.inspect(message) + ']');

            // parse message to json data
            var data = null;
            try {
                // null?
                if (!message) {
                    throw new Error('message is null.');
                }

                // empty?
                message = trim(message);
                if (message == '') {
                    throw new Error('message is empty.');
                }

                // parse json
                data = JSON.parse(message);
                if (!data) {
                    throw new Error('data is null.');
                }
            } catch (e) {
                self.logger.debug(clientName + ': ' + self.uuid + ': ' + webSocketClient.uuid +': message: failed to parse json. (e = "' + util.inspect(err) + '")');
                webSocketClient.ng('failed to parse json. (' + util.inspect(err) + ')');
                return;
            }

            // handle data
            if (data.requestId && data.requester && data.requester == self.uuid) { // response?
                this.requestContext.setResponse(data.requestId, null, data);
                return;
            }
            if (self.dataEventListener[data.type]) {
                var res = new Response(webSocketClient, data.type, data.requestId, data.requester, self.logger);
                self.dataEventListener[data.type](webSocketClient, data, res);
            } else {
                self.logger.debug('no data event listener for data type[' + data.type + ']');
            }
        });

        // on close
        webSocketClient.ws.on('close', function(code, reason){
            self.logger.info('on close: ' + clientName + ': ' + self.uuid + ': ' + webSocketClient.uuid + ': websocket client disconnected. (code = ' + code + ', reason = "' + reason + '")');
            if (self.disconnectEventListener) {
                self.disconnectEventListener(webSocketClient, code, reason);
            }
        });

        // on error
        webSocketClient.ws.on('error', function(err){
            self.logger.info('on error: ' + clientName + ': ' + self.uuid + ': ' + webSocketClient.uuid + ': websocket client error. (' + err + ')');
        });

        // emit on connect
        if (self.connectEventListener) {
            self.connectEventListener(webSocketClient);
        }
    });

    // on listening
    self.wss.on('listening', function() {
        self.logger.debug('on listening');
        if (self.startEventListener) {
            self.startEventListener();
        }
    });
}

// stop
WebSocketServer.prototype.stop = function() {
    this.logger.debug('stop');
    if (!this.wss) {
        return;
    }
    this.wss.close();
    var stopEventListener = this.stopEventListener;
    this.clear();
    if (stopEventListener) {
        stopEventListener();
    }
}

// cancel request
WebSocketServer.prototype.cancelRequest = function(requestId) {
    this.requestContext.cancelRequest(requestId);
}

// start event listener
WebSocketServer.prototype.setStartEventListener = function(eventListener) {
    this.startEventListener = eventListener;
}

// stop event listener
WebSocketServer.prototype.setStopEventListener = function(eventListener) {
    this.stopEventListener = eventListener;
}

// connect event listener
WebSocketServer.prototype.setConnectEventListener = function(eventListener) {
    this.connectEventListener = eventListener;
}

// disconnect event listener
WebSocketServer.prototype.setDisconnectEventListener = function(eventListener) {
    this.disconnectEventListener = eventListener;
}

// set data event listener
WebSocketServer.prototype.setDataEventListener = function(type, eventListener) {
    if (eventListener == null) {
        delete this.dataEventListener[type];
        return;
    }
    this.dataEventListener[type] = eventListener;
}

// activator
WebSocketServer.activate = function(config, logger, acceptor) {
    return new WebSocketServer(config, logger, acceptor);
}

// exports
module.exports = WebSocketServer;
