var util        = require('util');
var CouchClient = require('./couch_client');
var RedisClient = require('./redis_client');

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
    var self = this;
    if (callback == undefined) {
        callback = key;
        key = null;
    }
    var trycnt   = 0;
    var retrycnt = 0;
    var keylen   = 0;
    if (key != null && typeof(key) == "integer") {
        retrycnt = 3;
        keylen   = key;
    }
    save(self, key, callback, trycnt, retrycnt, keylen);
}
function save(self, key, callback, trycnt, retrycnt, keylen) {
    ++trycnt;
    if (keylen > 0) {
        var sym = 'abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890';
        key = '';
        for(var i = 0; i < keylen; i++) {
            key += sym[parseInt(Math.random() * sym.length)];
        }
    }
    self.getScope().insert(self, key, function(err, body) {
        if (err) {
            if (trycnt <= retrycnt) {
                save(self, key, callback, trycnt, retrycnt, keylen);
                return;
            }
            if (callback) {
                callback(err, null);
            }
            return;
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
    var getScope = function() {
        return CouchClient.getClient().getScope(typeName);
    }
    var getCacheKey = function(key) {
        return typeName + ":" + key;
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
    // params is CouchDB query options. see follow.
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

    // getCache
    type.getCache = function(key, callback) {
        var redis    = RedisClient.getClient();
        var cacheKey = getCacheKey(key);
        redis.get(cacheKey, function(err, reply) {
            if (err) {
                if (callback) {
                    callback(err, null);
                }
                return;
            }
            if (callback) {
                try {
                    var data = JSON.parse(reply);
                    data.prototype = type.prototype;
                } catch (e) {
                    callback(new Error(e.toString()), null);
                    return;
                }
                callback(null, data);
            }
        });
    });

    // setCache
    type.prototype.setCache = function(key, callback, ttl) {
        var redis     = RedisClient.getClient();
        var cacheKey  = getCacheKey(key);
        var cacheData = JSON.stringify(this);
        if (ttl != undefined) {
            redis.set(cacheKey, cacheData, 'NX', 'EX', ttl, function(err, reply) {
                if (err) {
                    if (callback) {
                        callback(err);
                    }
                    return;
                }
                if (callback) {
                    callback((reply == "OK")? null : new Error('invalid reply'));
                }
            });
        } else {
            redis.set(cacheKey, cacheData, 'NX', function(err, reply) {
                if (err) {
                    if (callback) {
                        callback(err);
                    }
                    return;
                }
                if (callback) {
                    callback((reply == "OK")? null : new Error('invalid reply'));
                }
            });
        }
    });

    // persistCache
    type.persistCache = function(key, callback) {
        var redis    = RedisClient.getClient();
        var cacheKey = getCacheKey(key);
        redis.persist(cacheKey, function(err, reply) {
            if (err) {
                if (callback) {
                    callback(err);
                }
                return;
            }
            if (callback) {
                callback(null);
            }
        });
    });

    // destroyCache
    type.destroyCache = function(key, callback) {
        var redis    = RedisClient.getClient();
        var cacheKey = getCacheKey(key);
        redis.del(cacheKey, function(err, reply) {
            if (err) {
                if (callback) {
                    callback(err);
                }
                return;
            }
            if (callback) {
                callback(null); //((reply == '1')? null : new Error('invalid reply'));
            }
        });
    });
}

// exports
module.exports = GameData;
