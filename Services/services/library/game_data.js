var util = require('util');

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

// exports
module.exports = GameData;
