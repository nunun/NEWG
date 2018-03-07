var util = require('util');

// constructor
function Response(webSocketClient, type, requestId, logger) {
    this.webSocketClient = webSocketClient;
    this.type            = type;
    this.requestId       = requestId;
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
    var sendData = {};
    sendData.type      = this.type;
    sendData.data      = data;
    sendData.requestId = this.requestId;
    sendData.response  = true;
    sendData.error     = null;
    this.webSocketClient.sendData(sendData);
    return true;
}

// error
Response.prototype.error = function(error) {
    if (!this.webSocketClient.ws) {
        this.logger.debug('Response.error: WehSocket is not opened yet.');
        return false;
    }
    this.logger.debug('Response.error: error[' + error + ']');
    var sendData = {};
    sendData.type      = this.type;
    sendData.data      = null;
    sendData.requestId = this.requestId;
    sendData.response  = true;
    sendData.error     = error;
    this.webSocketClient.sendData(sendData);
    return true;
}

// exports
module.exports = Response;
