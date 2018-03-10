var util        = require('util');
var CouchClient = require('./couch_client');

// constructor
function GameData() {
    this.init();
}
util.inherits(GameData, function(){});

// init
GameData.prototype.init = function() {
    // clear
    this.clear();
};

// clear
GameData.prototype.clear = function() {
    // NOTE
    // implement by inherit
}

// name
GameData.prototype.getName = function() {
    // NOTE
    // implement by inherti
    return 'GameData';
}

// find
GameData.find = function() {
    // TODO
}

// load
GameData.load = function() {
    // TODO
}

// save (callback)
GameData.prototype.save = function(callback) {
    this.save(null, callback);
}

// save (id, callback)
GameData.prototype.save = function(id, callback) {
    db = CouchClient.getClient().getConnection();
    db.insert(this, getName(), callback)
}

// remove (logical delete)
GameData.prototype.remove = function() {
    // TODO
}

// exports
module.exports = GameData;
