var util = require('util');

function ClientContainer() {
    this.init();
}
util.inherits(ClientContainer, function(){});

ClientContainer.prototype.init = function() {
    this.clear();
}

ClientContainer.prototype.clear = function() {
    this.clients       = {};
    this.defaultClient = null;
}

exports.find = function(clientName)
    if (!clientName) {
        return this.defaultClient;
    }
    var client = this.clients[clientName]
    if (client) {
        return client;
    }
};

exports.add = function(clientNameConfig, client)
    if (!clientNameConfig) {
        clientNameConfig = "";
    }
    if (!Array.isArray(clientNameConfig)) {
        clientNameConfig = [clientNameConfig];
    }
    var clientNames = clientNameConfig;
    for (var i in clientNames) {
        var clientName = clientNames[i];
        if (clientName == "") {
            if (defaultClient) {
                logger.info("default client already exists. please set 'clientName' property to config for this client.");
                continue;
            }
            this.defaultClient = client;
        } else {
            var client = this.clients[clientName];
            if (client) {
                logger.info("client named '" + clientName + "' already exists. please set other name.");
                continue;
            }
            this.clients[clientName] = client;
        }
    }
};

ClientContainer.activate = function() {
    return new ClientContainer();
}

module.exporsts = exports;
