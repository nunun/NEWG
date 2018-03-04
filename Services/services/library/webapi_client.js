var util    = require('util');
var assert  = require('assert');
var request = require('request');

// clients
var clients       = null;
var defaultClient = null;

// constructor
function WebAPIClient(config, logger) {
    this.init(config, logger);
}
util.inherits(WebAPIClient, function(){});

// init
WebAPIClient.prototype.init = function(config, logger) {
    this.config = config; // config
    this.logger = logger; // logger
    //this.uuid = uuid(); // server  uuid

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

// request
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

    // request
    var req = request(options, function(error, response, body) {
        if (error) {
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

    // return request canceller.
    // call req.abort() to cancel request.
    return req;
}

// get webapi client
WebAPIClient.getClient = function(name) {
    if (!name) {
        return defaultClient;
    }
    if (clients && clients[name]) {
        return clients[name];
    }
    return null;
}

// activate
WebAPIClient.activate = function(config, logger) {
    assert.ok(!clients);
    assert.ok(!defaultClient);
    clients       = {};
    defaultClient = null;
    if (!Array.isArray(config)) {
        config = [config];
    }
    for (var i in config) {
        var configEntry = config[i];
        var client = new WebAPIClient(configEntry, logger);
        if (configEntry.name) {
            clients[configEntry.name] = client;
        } else {
            defaultClient = client;
        }
    }
}

// exports
module.exports = WebAPIClient;
