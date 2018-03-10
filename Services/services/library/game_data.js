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

// save
GameData.prototype.save = function(key, callback) {
    if (callback == undefined) {
        callback = key;
        key = null;
    }
    this.getScope().insert(this, key, function(err, body) {
        if (err) {
            if (callback) {
                callback(err, null);
            }
        }
        if (callback) {
            callback(null, body._id);
        }
    });
}

// destory
GameData.prototype.destroy = function(callback) {
    if (!this._id || !this._rev) {
        callback(new Error('_id or _rev of this object is empty.'));
        return;
    }
    this.getScope().destroy(this._id, this._rev, function(err, body) {
        if (callback) {
            callback(err);
        }
    });
}

// setupType
GameData.setupType = function(typeName, type) {
    // internal get scope
    var getScope = function() {
        return CouchClient.getClient().getScope(typeName);
    }

    // get scope
    type.prototype.getScope = function() {
        return getScope();
    }

    // get
    type.get = function(key, callback) {
        getScope().get(key, function(err, body) {
            if (err) {
                if (callback) {
                    callback(err, null);
                }
                return;
            }
            if (callback) {
                body.prototype = type.prototype;
                callback(err, body);
            }
        });
    }

    // list
    // https://wiki.apache.org/couchdb/HTTP_view_API#Querying_Options
    type.list = function(params, callback) {
        getScope().list(params, function(err, body) {
            if (err) {
                if (callback) {
                    callback(err);
                }
                return;
            }
            if (callback) {
                var rows = body.rows;
                for (var i in rows) {
                    var row = rows[i];
                    row.prototype = type.prototype;
                }
                callback(null, rows);
            }
        });
    }
}

// exports
module.exports = GameData;
