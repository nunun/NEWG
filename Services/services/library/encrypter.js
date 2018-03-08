var util = require('util');

// constructor
function Encrypter(config, logger) {
    this.init(config, logger);
}
util.inherits(Encrypter, function(){});

// init
Encrypter.prototype.init = function(config, logger) {
    this.config = config;
    this.logger = logger;

    // clear
    this.clear();
};

// clear
Encrypter.prototype.clear = function() {
    // NOTE
    // implement by inherit
}

// encrypt (outgoing)
Encrypter.prototype.encrypt = function(message) {
    return message;
}

// decrypt (incoming)
Encrypter.prototype.decrypt = function(message) {
    return message;
}

// exports
module.exports = Encrypter;
