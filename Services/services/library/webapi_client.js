var util            = require('util');
var assert          = require('assert');
var request         = require('request');
var clientContainer = require('./client_container').activate();

// constructor
function WebAPIClient(config, logger) {
    this.init(config, logger);
}
util.inherits(WebAPIClient, function(){});

// init
WebAPIClient.prototype.init = function(config, logger) {
    this.config = config; // config
    this.logger = logger; // logger

    // clear
    this.clear();
};

// clear
WebAPIClient.prototype.clear = function() {
    // nothing to do
};

// get
WebAPIClient.prototype.get = function(apiPath, callback, queries, forms, headers) {
    this.startRequest('GET', apiPath, null, callback, queries, forms, headers);
}

// post
WebAPIClient.prototype.post = function(apiPath, data, callback, queries, forms, headers) {
    this.startRequest('POST', apiPath, data, callback, queries, forms, headers);
}

// start request
WebAPIClient.prototype.startRequest = function(method, apiPath, data, callback, queries, forms, headers) {
    var self = this;

    // build options
    var options = {url:self.config.url + apiPath};
    if (method == 'POST') {
        options.method = 'POST';
    }
    if (queries || self.config.queries) {
        options.qs = {};
        if (self.config.queries) { options.qs = Object.assign(options.qs, self.config.queries); }
        if (queries)             { options.qs = Object.assign(options.qs, queries);             }
    }
    if (forms || self.config.forms) {
        options.forms = {};
        if (self.config.forms) { options.forms = Object.assign(options.forms, self.config.forms); }
        if (forms)             { options.forms = Object.assign(options.forms, forms);             }
    }
    if (headers || self.config.headers) {
        options.headers = {};
        if (self.config.headers) { options.headers = Object.assign(options.headers, self.config.headers); }
        if (headers)             { options.headers = Object.assign(options.headers, headers);             }
    }

    // body
    //var contentType = (options.headers && options.headers['content-type'])? options.headers['content-type'] : null;
    options.body = JSON.stringify(data);

    // create request
    var req = {};
    req.request       = null;
    req.timeoutId     = null;
    req.retryCount    = self.config.retryCount    || 10;
    req.retryInterval = self.config.retryInterval || 3000;
    req.abort = function() {
        if (req.request) {
            req.request.abort();
            req.request = null;
        }
        if (req.timeoutId) {
            clearTimeout(req.timeoutId);
            req.timeoutId = null;
        }
    }

    // startRequest
    startRequest(self, req, options, method, apiPath, data, callback, queries, forms, headers);

    // return request canceller.
    // req.abort() to cancel request.
    return req;
}
function startRequest(self, req, options, method, apiPath, data, callback, queries, forms, headers) {
    // request and response
    self.logger.debug("WebAPIClient: startRequest: outgoing: options[" + util.inspect(options, {depth:null, breakLength:Infinity}) + "]");
    req.request = request(options, function(error, response, body) {
        self.logger.debug("WebAPIClient: startRequest: incoming: error[" + error + "] response[" + util.inspect(options, {depth:null,breakLength:Infinity}) + "] body[" + util.inspect(body, {depth:null,breakLength:Infinity}) + "]");
        if (error) {
            if (error.code == 'ECONNREFUSED' && req.retryCount > 0) {
                self.logger.debug("WebAPIClient: startRequest: retry: " + req.retryCount + " times remaining.");
                req.retryCount--;
                req.timeoutId = setTimeout(function() {
                    startRequest(self, req, options, method, apiPath, data, callback, queries, forms, headers);
                }, req.retryInterval);
                return;
            }
            if (callback) {
                callback(error, null);
            }
            return;
        }

        // parse body
        var data = null;
        try {
            data = JSON.parse(body);
        } catch (e) {
            callback(e, null);
            return;
        }

        // callback
        if (callback) {
            callback(null, data);
        }
    });
}

// get client
WebAPIClient.getClient = function(clientName) {
    return clientContainer.find(clientName);
}

// activate
WebAPIClient.activate = function(config, logger) {
    client = new WebAPIClient(config, logger);
    if (config) {
        clientContainer.add(config.clientName, client);
    }
    return client;
}

// exports
module.exports = WebAPIClient;
