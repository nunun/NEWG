var util = require('util');

// constructor
function Response(webSocketClient, type, requestId, requester, logger) {
    this.webSocketClient = webSocketClient;
    this.type            = type;
    this.requestId       = requestId;
    this.requester       = requester;
    this.logger          = logger;
}
util.inherits(Response, function(){});

// send
Response.prototype.send = function(data) {
    if (!this.webSocketClient.ws) {
        this.logger.debug('Response.send: WehSocket is not opened yet.');
        return false;
    }
    this.logger.debug('Response.send: data[' + util.inspect(data, {depth:null,breakLength:Infinity}) + ']');
    var sendData = data;
    sendData.requestId = this.requestId;
    sendData.requester = this.requester;
    this.webSocketClient.send(this.type, sendData);
    return true;
}

// exports
module.exports = Response;
