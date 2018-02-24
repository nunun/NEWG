var url              = require('url');
var util             = require('util');
var uuid             = require('uuid');
var trim             = require('string.prototype.trim');
var WebSocketClient  = require('./lib').WebSocketClient;
var RequestToRemote  = require('./types/request_to_remote');
var ResponseToRemote = require('./types/response_to_remote');

// constructor
function MindlinkClient(config, logger) {
    this.init(config, logger);
}
util.inherits(MindlinkClient, WebSocketClient);

// data types
MindlinkClient.DATA_TYPE = {};
MindlinkClient.DATA_TYPE.S = 1; // SERVICE
MindlinkClient.DATA_TYPE.Q = 2; // QUERY
MindlinkClient.DATA_TYPE.M = 3; // MESSAGE
MindlinkClient.prototype.DATA_TYPE = MindlinkClient.DATA_TYPE;

// init
MindlinkClient.prototype.init = function(config, logger) {
    MindlinkClient.super_.prototype.init.call(this, config, logger);
    var self = this;

    // event listeners
    self.dataFromRemoteEventListener = {}; // data from remote

    // connect to data from remote event listener.
    self.setDataEventListener(MindlinkClient.DATA_TYPE.M, function(data, res) {
        if (!data.payload) { // NOTE require payload
            return;
        }
        if (self.dataFromRemoteEventListener[data.payload.type]) {
            var res = new ResponseToRemote(self, data.from, data.payload.type, data.requestId, data.requester, self.logger);
            self.dataFromRemoteEventListener[data.payload.type](data.payload, res);
        } else {
            self.logger.debug('no data from remote event listener for data payload type[' + data.payload.type + ']');
        }
    });
}

// clear
MindlinkClient.prototype.clear = function() {
    MindlinkClient.super_.prototype.clear.call(this);
}

// send status
MindlinkClient.prototype.sendStatus = function(data, callback, timeout) {
    var sendData = {}
    sendData.service = data;
    return this.send(this.DATA_TYPE.S, sendData, callback, timeout);
}

// send query
MindlinkClient.prototype.sendQuery = function(jspath, callback, timeout) {
    var sendData = {}
    sendData.jspath = jspath;
    return this.send(this.DATA_TYPE.Q, sendData, callback, timeout);
}

// TODO refactor
// send to remote
MindlinkClient.prototype.sendToRemote = function(to, type, data, callback, timeout) {
    if (!this.ws) {
        this.logger.debug('sendToRemote: WehSocket is not opened yet.');
        if (callback) {
            callback(new Error('WebSocket is not opened yet.'), null);
        }
        return;
    }
    this.logger.debug('sendToRemote: to[' + to + '] type[' + type + '] data[' + util.inspect(data) + '] callback[' + ((callback)? true : false) + '] timeout[' + timeout + ']');
    var sendData = {};
    sendData.type         = this.DATA_TYPE.M;
    sendData.to           = to;
    sendData.payload      = data;
    sendData.payload.type = type;
    var requestId = null;
    if (callback) {
        requestId = this.requestContext.nextRequestId();
        sendData.requestId = requestId;
        sendData.requester = this.uuid;
        var request = RequestToRemote.getRequest(requestId, callback, (timeout || this.config.requestTimeout || 10000));
        this.requestContext.setRequest(request);
    }
    var message = JSON.stringify(sendData);
    this.ws.send(message);
    return requestId;
}

// set data from remote event listener
MindlinkClient.prototype.setDataFromRemoteEventListener = function(type, eventListener) {
    if (eventListener == null) {
        delete this.dataFromRemoteEventListener[type];
        return;
    }
    this.dataFromRemoteEventListener[type] = eventListener;
}

// activate
MindlinkClient.activate = function(config, logger) {
    return new MindlinkClient(config, logger);
}

// exports
module.exports = MindlinkClient;
