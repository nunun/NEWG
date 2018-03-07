var util = require('util');

function InstanceContainer() {
    this.init();
}
util.inherits(InstanceContainer, function(){});

InstanceContainer.prototype.init = function() {
    this.clear();
}

InstanceContainer.prototype.clear = function() {
    this.instances       = {};
    this.defaultInstance = null;
}

InstanceContainer.prototype.find = function(name) {
    if (!name) {
        return this.defaultInstance;
    }
    var instance = this.instances[name]
    if (instance) {
        return instance;
    }
};

InstanceContainer.prototype.add = function(nameConfig, instance) {
    if (!nameConfig) {
        nameConfig = "";
    }
    if (!Array.isArray(nameConfig)) {
        nameConfig = [nameConfig];
    }
    var names = nameConfig;
    for (var i in names) {
        var name = names[i];
        if (name == "") {
            if (this.defaultInstance) {
                logger.info("default instance already exists. please set name to this instance.");
                continue;
            }
            this.defaultInstance = instance;
        } else {
            var instance = this.instances[name];
            if (instance) {
                logger.info("instance named '" + name + "' already exists. please set other name to this instance.");
                continue;
            }
            this.instances[name] = instance;
        }
    }
};

InstanceContainer.prototype.remove = function(instance) {
    for (var i in this.instances) {
        var instance = this.instances[i];
        if (instance == instance) {
            delete this.instances[i];
        }
    }
    if (defaultInstance == instance) {
        defaultInstance = null;
    }
}

InstanceContainer.activate = function() {
    return new InstanceContainer();
}

module.exports = InstanceContainer;
