var util = require('util');

// constructor
function ResponseToRemote(mindlikClient, from, type, requestId, logger) {
    this.mindlikClient = mindlikClient;
    this.to            = from;
    this.type          = type;
    this.requestId     = requestId;
    this.logger        = logger;
}
util.inherits(ResponseToRemote, function(){});

// send
ResponseToRemote.prototype.send = function(data) {
    if (!this.mindlikClient.ws) {
        this.logger.debug('ResponseToRemote.send: WehSocket is not opened yet.');
        return false;
    }
    this.logger.debug('ResponseToRemote.send: data[' + util.inspect(data, {depth:null,breakLength:Infinity}) + ']');
    var sendData = {};
    sendData.type             = this.mindlikClient.DATA_TYPE.M;
    sendData.data             = data;
    sendData.requestId        = 0;
    sendData.response         = false;
    sendData.error            = null;
    sendData.remote           = {};
    sendData.remote.from      = null;
    sendData.remote.to        = this.to;
    sendData.remote.type      = this.type;
    sendData.remote.requestId = this.requestId;
    sendData.remote.response  = true;
    sendData.remote.error     = null;
    this.mindlikClient.sendData(sendData);
    return true;
}

// error
ResponseToRemote.prototype.error = function(error) {
    if (!this.mindlikClient.ws) {
        this.logger.debug('ResponseToRemote.error: WehSocket is not opened yet.');
        return false;
    }
    this.logger.debug('ResponseToRemote.error: error[' + error + ']');
    var sendData = {};
    sendData.type             = this.mindlikClient.DATA_TYPE.M;
    sendData.data             = null;
    sendData.requestId        = 0;
    sendData.response         = false;
    sendData.error            = null;
    sendData.remote           = {};
    sendData.remote.from      = null;
    sendData.remote.to        = this.to;
    sendData.remote.type      = this.type;
    sendData.remote.requestId = this.requestId;
    sendData.remote.response  = true;
    sendData.remote.error     = error;
    this.mindlikClient.sendData(sendData);
    return true;
}

// exports
module.exports = ResponseToRemote;
