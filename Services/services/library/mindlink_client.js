var url               = require('url');
var util              = require('util');
var uuid              = require('uuid');
var trim              = require('string.prototype.trim');
var WebSocketClient   = require('./websocket_client');
var RequestToRemote   = require('./internal_types/request_to_remote');
var ResponseToRemote  = require('./internal_types/response_to_remote');
var Encrypter         = require('./encrypter');
var instanceContainer = require('./instance_container').activate();

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
    self.setDataThruEventListener(MindlinkClient.DATA_TYPE.M, function(recvData, res) {
        if (recvData.remote.requestId && recvData.remote.response) {
            self.requestContext.setResponse(recvData.remote.requestId, recvData.remote.error, recvData.data);
            return;
        }
        if (self.dataFromRemoteEventListener[recvData.remote.type]) {
            var res = new ResponseToRemote(self, recvData.remote.from, recvData.remote.type, recvData.remote.requestId, self.logger);
            self.dataFromRemoteEventListener[recvData.remote.type](recvData, res);
        } else {
            self.logger.debug('no data from remote event listener for data type[' + recvData.remote.type + ']');
        }
    });
}

// clear
MindlinkClient.prototype.clear = function() {
    MindlinkClient.super_.prototype.clear.call(this);
}

// send status
MindlinkClient.prototype.sendStatus = function(data, callback, timeout) {
    var sendData = data;
    return this.send(this.DATA_TYPE.S, sendData, callback, timeout);
}

// send query
MindlinkClient.prototype.sendQuery = function(jspath, callback, timeout) {
    var sendData = {}
    sendData.jspath = jspath;
    return this.send(this.DATA_TYPE.Q, sendData, (err,data) => {
        if (err) {
            callback(err, null);
            return;
        }
        var services = data.services;
        if (services == null) {
            callback(new Error("invalid response."), null);
            return;
        }
        callback(null, services);
    }, timeout);
}

// send to remote
MindlinkClient.prototype.sendToRemote = function(to, type, data, callback, timeout) {
    if (!this.ws) {
        this.logger.debug('sendToRemote: WehSocket is not opened yet.');
        if (callback) {
            callback(new Error('WebSocket is not opened yet.'), null);
        }
        return;
    }
    this.logger.debug('sendToRemote: to[' + to + '] type[' + type + '] data[' + util.inspect(data, {depth:null,breakLength:Infinity}) + '] callback[' + ((callback)? true : false) + '] timeout[' + timeout + ']');
    var sendData = {};
    sendData.type             = this.DATA_TYPE.M;
    sendData.data             = data;
    sendData.requestId        = 0;
    sendData.response         = false;
    sendData.error            = null;
    sendData.remote           = {};
    sendData.remote.from      = null;
    sendData.remote.to        = to;
    sendData.remote.type      = type;
    sendData.remote.requestId = 0;
    sendData.remote.response  = false;
    sendData.remote.error     = null;
    if (callback) {
        var requestId = this.requestContext.nextRequestId();
        sendData.remote.requestId = requestId;
        var request = RequestToRemote.rentFromPool(requestId, callback, (timeout || this.config.requestTimeout || 10000));
        this.requestContext.setRequest(request);
    }
    var message = this.encrypter.encrypt(JSON.stringify(sendData));
    this.ws.send(message);
    return sendData.remote.requestId;
}

// set data from remote event listener
MindlinkClient.prototype.setDataFromRemoteEventListener = function(type, eventListener) {
    if (eventListener == null) {
        delete this.dataFromRemoteEventListener[type];
        return;
    }
    this.dataFromRemoteEventListener[type] = function(recvData, res) {
        eventListener(recvData.data, res);
    }
}

// set data from remote thru event listener
MindlinkClient.prototype.setDataFromRemoteThruEventListener = function(type, eventListener) {
    if (eventListener == null) {
        delete this.dataFromRemoteEventListener[type];
        return;
    }
    this.dataFromRemoteEventListener[type] = eventListener;
}

// get client
MindlinkClient.getClient = function(clientName) {
    return instanceContainer.find(clientName);
}

// activate
MindlinkClient.activate = function(config, logger) {
    client = new MindlinkClient(config, logger);
    if (config) {
        instanceContainer.add(config.clientName, client);
    }
    return client;
}

// exports
module.exports = MindlinkClient;
