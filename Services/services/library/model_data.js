var util        = require('util');
var CouchClient = require('./couch_client');
var RedisClient = require('./redis_client');

// key generation format on save ModelData.
var saveKeyRegex = /^(.*)%([0-9]+)s(.*)$/;

// constructor
function ModelData() {
    this.init();
}
util.inherits(ModelData, function(){});

// init
ModelData.prototype.init = function() {
    // clear
    this.clear();
};

// clear
ModelData.prototype.clear = function() {
    // NOTE
    // implement by inherit
}

// save
ModelData.prototype.save = function(fieldName, key, callback) {
    var self = this;
    if (key == undefined) {//when one argument
        callback  = fieldName;
        key       = null;
        fieldName = null;
    } else if (callback == undefined) {//when two arguments
        callback  = key;
        key       = fieldName;
        fieldName = null;
    }
    key = key || 8;
    var retryCount = 0;
    if (typeof(key) == "number" || saveKeyRegex.exec(key)) {
        retryCount = 3;
    }
    save(self, fieldName, key, callback, retryCount);
}
function save(self, fieldName, key, callback, retryCount) {
    var k = key;
    if (typeof(k) == "number") {
        k = keygen(k);
    } else {
        var m = saveKeyRegex.exec(k);
        if (m) {
            k = m[1] + keygen(parseInt(m[2])) + m[3];
        }
    }
    if (fieldName) {
        if (!self[fieldName]) {
            callback(new Error('no field'), null, null);
            return;
        }
        self[fieldName] = k;
    }
    self.getScope(function(err, scope) {
        if (err) {
            if (callback) {
                callback(err, null, null);
            }
            return;
        }
        scope.insert(self, k, function(err, body) {
            if (err) {
                if (retryCount >= 0) {
                    save(self, fieldName, key, callback, retryCount - 1);
                    return;
                }
                if (callback) {
                    callback(err, null, null);
                }
                return;
            }
            if (callback) {
                callback(null, body.id, body.rev);
            }
        });
    });
}
function keygen(len) {
    var sym = 'abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890';
    var key = '';
    for(var i = 0; i < len; i++) {
        key += sym[parseInt(Math.random() * sym.length)];
    }
    return key;
}

// setupType
ModelData.setupType = function(type, typeName, databaseName) {
    type.databaseName = databaseName;
    type.cacheKey     = typeName;

    // get database name (class method)
    type.getDatabaseName = function() {
        return type.databaseName;
    }

    // get cache key (class method)
    type.getCacheKey = function(key) {
        return type.cacheKey + ":" + key;
    }

    // get scope (class method)
    type.getScope = function(callback) {
        return CouchClient.getClient().getScope(type.getDatabaseName(), callback);
    }

    // get scope
    type.prototype.getScope = function(callback) {
        return type.getScope(callback);
    }

    // get
    type.get = function(key, callback) {
        type.getScope(function(err, scope) {
            if (err) {
                if (callback) {
                    callback(err, null);
                }
                return;
            }
            scope.get(key, function(err, body) {
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
        type.getScope(function(err, scope) {
            if (err) {
                if (callback) {
                    callback(err, null);
                }
                return;
            }
            scope.list(params, function(err, body) {
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
        });
    }

    // destory
    type.destroy = function(id, rev, callback) {
        type.getScope(function(err, scope) {
            if (err) {
                if (callback) {
                    callback(err, null);
                }
                return;
            }
            scope.destroy(id, rev, function(err, body) {
                if (callback) {
                    callback(err);
                }
            });
        });
    }

    // getCache
    type.getCache = function(key, callback) {
        var redis    = RedisClient.getClient().getConnection();
        var cacheKey = type.getCacheKey(key);
        redis.get(cacheKey, function(err, reply) {
            if (err) {
                if (callback) {
                    callback(err, null);
                }
                return;
            }
            if (callback) {
                var data = null;
                try {
                    var replyData = JSON.parse(reply);
                    var data = new type();
                    Object.assign(data, replyData);
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
        var redis     = RedisClient.getClient().getConnection();
        var cacheKey  = type.getCacheKey(key);
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
        var redis    = RedisClient.getClient().getConnection();
        var cacheKey = type.getCacheKey(key);
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
        var redis    = RedisClient.getClient().getConnection();
        var cacheKey = type.getCacheKey(key);
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
module.exports = ModelData;
