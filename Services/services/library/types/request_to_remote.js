var util    = require('util');
var Request = require('./request');

// request pool (RequestToRemote)
var pool = [];

// constructor
function RequestToRemote() {
    this.init();
}
util.inherits(RequestToRemote, Request);

// init
RequestToRemote.prototype.init = function() {
    RequestToRemote.super_.prototype.init.call(this);
}

// clear
RequestToRemote.prototype.clear = function() {
    RequestToRemote.super_.prototype.clear.call(this);
}

// set response
RequestToRemote.prototype.setResponse = function(err, data) {
    if (this.callback) {
        var callback = this.callback;
        this.callback = null;
        callback(err, data);
    }
}

// return to pool
RequestToRemote.prototype.returnToPool = function() {
    this.clear();
    pool.push(this);
}

// rent from pool
RequestToRemote.rentFromPool = function(requestId, callback, timeout) {
    var request = null;
    if (pool.length > 0) {
        request = pool.pop();
        request.Clear();
    } else {
        request = new RequestToRemote();
    }
    request.requestId = requestId;
    request.callback  = callback;
    request.timeout   = timeout;
    return request;
}

// exports
module.exports = RequestToRemote;
