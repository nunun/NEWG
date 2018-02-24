var util = require('util');

// constructor
function RequestContext(logger) {
    this.init(logger);
}
util.inherits(RequestContext, function(){});

// init
RequestContext.prototype.init = function(logger) {
    this.logger = logger; // logger

    // clear
    this.clear();
}

// clear
RequestContext.prototype.clear = function() {
    // abort all
    for (var i in this.requestMap) {
        var request = this.requestMap[i];
        this.requestMap[i] = null;
        request.setResponse(new Error('abort.'), null);
        request.returnToPool();
    }

    // request context
    this.requestMap     = {};
    this.requestIdCount = 0;
}

// next requestId
RequestContext.prototype.nextRequestId = function() {
    return (++this.requestIdCount == 0)? ++this.requestIdCount : this.requestIdCount;
}

// set request
RequestContext.prototype.setRequest = function(request) {
    var self = this;
    self.logger.debug("setRequest: request[" + util.inspect(request, {depth:null,breakLength:Infinity}) + "]");
    self.requestMap[request.requestId] = request;
    request.startTimeout(function() {
        self.logger.debug("setRequest: timeout: request[" + util.inspect(request, {depth:null,breakLength:Infinity}) + "]");
        delete self.requestMap[request.requestId];
        request.setResponse(new Error("timeout."), null);
        request.returnToPool();
    });
}

// set response
RequestContext.prototype.setResponse = function(requestId, err, data) {
    this.logger.debug("setResponse: requestId[" + requestId + "] err[" + util.inspect(err, {depth:null,breakLength:Infinity}) + "] data[" + util.inspect(data, {depth:null,breakLength:Infinity}) + "]");
    if (!this.requestMap[requestId]) {
        return;
    }
    var request = this.requestMap[requestId];
    delete this.requestMap[requestId];
    request.stopTimeout();
    request.setResponse(err, data);
    request.returnToPool();
}

// cancel request
RequestContext.prototype.cancelRequest = function(requestId) {
    this.logger.debug("cancelRequest: requestId[" + requestId + "]");
    this.setResponse(requestId, new Error('cancelled.'), null);
}

// exports
module.exports = RequestContext;
