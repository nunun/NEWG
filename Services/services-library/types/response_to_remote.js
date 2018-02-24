var util = require('util');

// constructor
function ResponseToRemote(mindlikClient, from, type, requestId, requester, logger) {
    this.mindlikClient = mindlikClient;
    this.to            = from;
    this.type          = type;
    this.requestId     = requestId;
    this.requester     = requester;
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
    sendData.to           = this.to;
    sendData.payload      = data; // NOTE add payload
    sendData.payload.type = this.type;
    sendData.requestId    = this.requestId;
    sendData.requester    = this.requester;
    this.mindlikClient.send(this.mindlikClient.DATA_TYPE.M, sendData);
    return true;
}

// exports
module.exports = ResponseToRemote;
