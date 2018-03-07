var util = require('util');

function InstanceContainer() {
    this.init();
}
util.inherits(InstanceContainer, function(){});

InstanceContainer.prototype.init = function() {
    this.clear();
}

InstanceContainer.prototype.clear = function() {
    this.clients       = {};
    this.defaultClient = null;
}

InstanceContainer.prototype.find = function(clientName) {
    if (!clientName) {
        return this.defaultClient;
    }
    var client = this.clients[clientName]
    if (client) {
        return client;
    }
};

InstanceContainer.prototype.add = function(clientNameConfig, client) {
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
            if (this.defaultClient) {
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

InstanceContainer.prototype.remove = function(client) {
    for (var i in this.clients) {
        var client = this.clients[i];
        if (client == client) {
            delete this.clients[i];
        }
    }
    if (defaultClient == client) {
        defaultClient = null;
    }
}

InstanceContainer.activate = function() {
    return new InstanceContainer();
}

module.exports = InstanceContainer;
