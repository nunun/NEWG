var util = require('util');

// request pool (Request)
var pool = [];

// constructor
function Request() {
    this.init();
}
util.inherits(Request, function(){});

// init
Request.prototype.init = function() {
    this.clear();
}

// clear
Request.prototype.clear = function() {
    if (this.callback) {
        this.callback(new Error("abort."), null);
    }
    this.stopTimeout();
    this.requestId = 0;
    this.callback  = null;
    this.timeout   = 0;
    this.timeoutId = null;
}

// set response
Request.prototype.setResponse = function(err, data) {
    if (this.callback) {
        var callback = this.callback;
        this.callback = null;
        callback(err, data);
    }
}

// return to request pool
Request.prototype.returnToPool = function() {
    this.clear();
    pool.push(this);
}

// start timeout
Request.prototype.startTimeout = function(callback) {
    this.stopTimeout();
    this.timeoutId = setTimeout(callback, this.timeout);
}

// setop timeout
Request.prototype.stopTimeout = function() {
    if (!this.timeoutId) {
        return;
    }
    clearTimeout(this.timeoutId);
    this.timeoutId = null;
}

// get request
Request.getRequest = function(requestId, callback, timeout) {
    var request = null;
    if (pool.length > 0) {
        request = pool.pop();
        request.clear();
    } else {
        request = new Request();
    }
    request.requestId = requestId;
    request.callback  = callback;
    request.timeout   = timeout;
    return request;
}

// exports
module.exports = Request;
