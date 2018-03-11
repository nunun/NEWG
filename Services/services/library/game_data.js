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
    if (key != null && typeof(key) == "number") {
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
            callback(null, body.id, body.rev);
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
                var data = new type();
                Object.assign(data, body);
                callback(err, data);
            }
        });
    }

    // list
    // params is CouchDB query options. see follow.
    // https://wiki.apache.org/couchdb/HTTP_view_API#Querying_Options
    // {startkey:'cat', limit:3}
    type.list = function(params, callback) {
        if (!params.include_docs) {
            params.include_docs = true;
        }
        getScope().list(params, function(err, body) {
            if (err) {
                if (callback) {
                    callback(err);
                }
                return;
            }
            if (callback) {
                var proto = {activate: function() {
                    var data = new type();
                    Object.assign(data, this);
                    return data;
                }};
                var rows = body.rows;
                var list = [];
                for (var i in rows) {
                    var doc = rows[i].doc;
                    doc.__proto__ = proto;
                    list.push(doc);
                }
                callback(null, list);
            }
        });
    }

    // destory
    type.destroy = function(id, rev, callback) {
        getScope().destroy(id, rev, function(err, body) {
            if (callback) {
                callback(err);
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
    }

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
    }

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
    }

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
    }
}

// exports
module.exports = GameData;
