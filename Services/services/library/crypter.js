var util = require('util');

// constructor
function Crypter(config, logger) {
    this.init(config, logger);
}
util.inherits(Crypter, function(){});

// init
Crypter.prototype.init = function(config, logger) {
    this.config = config;
    this.logger = logger;

    // clear
    this.clear();
};

// clear
Crypter.prototype.clear = function() {
    // NOTE
    // implement by inherit
}

// encrypt (outgoing)
Crypter.prototype.encrypt = function(message) {
    return message;
}

// decrypt (incoming)
Crypter.prototype.decrypt = function(message) {
    return message;
}

// exports
module.exports = Crypter;
