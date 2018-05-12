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

// save options 'keyProperty'
ModelData.prototype.keyProperty = function(propertyName) {
    this._saveOptions = this._saveOptions || {};
    this._saveOptions.keyProperty = propertyName;
    return this;
}

// save options 'cacheTTL'
ModelData.prototype.cacheTTL = function(seconds) {
    this._saveOptions = this._saveOptions || {};
    this._saveOptions.cacheTTL = seconds;
    return this;
}

// save
ModelData.prototype.save = function(key, callback) {
    var self = this;
    if (key === undefined) {
        callback = key;
        key      = null;
    }
    if (!key) {
        if (self.hasOwnProperty(self._saveOptions.keyProperty) && self[self._saveOptions.keyProperty]) {
            key = self[self._saveOptions.keyProperty];
        } else if (self.hasOwnProperty("_id")) {
            key = self._id;
        }
    }
    if (!key) {
        if (callback) {
            callback(new Error('no id'));
        }
        return;
    }
    var saveOptions = self._saveOptions;
    if (self._saveOptions) {
        delete self._saveOptions;
    }
    save(self, key, callback, (saveOptions || {}), ((saveKeyRegex.exec(key))? 3 : 0));
}
function save(self, key, callback, saveOptions, retryCount) {
    var k = key;
    var m = saveKeyRegex.exec(k);
    if (m) {
        k = m[1] + keygen(parseInt(m[2])) + m[3];
    }
    var keyProperty = saveOptions.keyProperty;
    if (keyProperty) {
        if (!self.hasOwnProperty(keyProperty)) {
            if (callback) {
                callback(new Error('no field'), null, null);
            }
            return;
        }
        self[keyProperty] = k;
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
                    save(self, key, callback, retryCount - 1);
                    return;
                }
                if (callback) {
                    callback(err || new Error('retry exceeded.'), null, null);
                }
                return;
            }
            if (callback) {
                self._id  = body.id;
                self._rev = body.rev;
                callback(null, body.id, body.rev);
            }
        });
    });
}

// promiseSave
ModelData.prototype.promiseSave = function(key) {
    var self = this;
    if (key === undefined) {
        key = null;
    }
    return new Promise((resolve, reject) => {
        self.save(key, (err) => {
            if (err) {
                throw err;
            }
            resolve();
        });
    });
}

// export
ModelData.prototype.export = function() {
    if (this._id) {
        delete this._id;
    }
    if (this._rev) {
        delete this._rev;
    }
    if (this._saveOptions) {
        delete this._saveOptions;
    }
    return this;
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

    // promiseGet
    type.promiseGet = function(key) {
        return new Promise((resolve, reject) => {
            type.get(key, (err, data) => {
                if (err) {
                    throw err;
                }
                resolve(data);
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
                        callback(err, null);
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

    // promiseList
    type.promiseList = function(key) {
        return new Promise((resolve, reject) => {
            type.list(key, (err, list) => {
                if (err) {
                    throw err;
                }
                resolve(list);
            });
        });
    }

    // destory
    type.destroy = function(id, rev, callback) {
        if (callback !== undefined) {//when 3 arguments
            // nothing to do
        } else if (rev !== undefined) {//when 2 arguments
            callback = rev;
            rev      = null;
        } else {//when 1 argument
            callback = null;
            rev      = null;
        }
        if (!rev) {
            type.get(id, function(err, data) {
                if (err) {
                    if (callback) {
                        callback(err);
                    }
                    return;
                }
                if (!data._id || !data._rev) {
                    if (callback) {
                        callback(new Error('no id or rev'));
                    }
                    return;
                }
                type.destroy(data._id, data._rev, callback);
            });
            return;
        }
        type.getScope(function(err, scope) {
            if (err) {
                if (callback) {
                    callback(err);
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

    // promiseDestroy
    type.promiseDestroy = function(id, rev) {
        return new Promise((resolve, reject) => {
            type.destroy(id, rev, (err) => {
                if (err) {
                    throw err;
                }
                resolve();
            });
        });
    }

    // destory (prototype)
    type.prototype.destroy = function(callback) {
        var self = this;
        type.destroy(self._id, self._rev, (err) => {
            if (callback) {
                callback(err);
            }
        });
    }

    // promiseDestroy (prototype)
    type.prototype.promiseDestroy = function(callback) {
        var self = this;
        return new Promise((resolve, reject) => {
            self.destroy((err) => {
                if (err) {
                    throw err;
                }
                resolve();
            });
        });
    }

    // saveCache
    ModelData.prototype.saveCache = function(key, callback) {
        var self = this;
        if (key === undefined) {
            callback = key;
            key      = null;
        }
        if (!key) {
            if (self.hasOwnProperty(self._saveOptions.keyProperty) && self[self._saveOptions.keyProperty]) {
                key = self[self._saveOptions.keyProperty];
            }
        }
        if (!key) {
            if (callback) {
                callback(new Error('no id'));
            }
            return;
        }
        var saveOptions = self._saveOptions;
        if (self._saveOptions) {
            delete self._saveOptions;
        }
        saveCache(self, key, callback, (saveOptions || {}), ((saveKeyRegex.exec(key))? 3 : 0));
    }
    function saveCache(self, key, callback, saveOptions, retryCount) {
        var k = key;
        var m = saveKeyRegex.exec(k);
        if (m) {
            k = m[1] + keygen(parseInt(m[2])) + m[3];
        }
        var keyProperty = saveOptions.keyProperty;
        if (keyProperty) {
            if (!self.hasOwnProperty(keyProperty)) {
                if (callback) {
                    callback(new Error('no field'), null, null);
                }
                return;
            }
            self[keyProperty] = k;
        }
        var setArgs = [type.getCacheKey(k), JSON.stringify(self)];
        if (m) {
            setArgs.push('NX');
        }
        if (saveOptions.cacheTTL) {
            setArgs.push('EX', saveOptions.cacheTTL);
        }
        setArgs.push(function(err, reply) {
            if (err || reply != "OK") { // NOTE reply become null if key always exists with 'NX'.
                if (retryCount >= 0) {
                    saveCache(self, key, callback, saveOptions, retryCount - 1);
                    return;
                }
                if (callback) {
                    callback(err || new Error('retry exceeded.'));
                }
                return;
            }
            if (callback) {
                callback(null);
            }
        });
        var redis = RedisClient.getClient().getConnection();
        redis.set.apply(redis, setArgs);
    }

    // promiseSaveCache
    ModelData.prototype.promiseSaveCache = function(key) {
        var self = this;
        if (key === undefined) {
            key = null;
        }
        return new Promise((resolve, reject) => {
            self.saveCache(key, (err) => {
                if (err) {
                    throw err;
                }
                resolve();
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
                if (reply == null) {
                    callback(null, null);
                    return;
                }
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

    // promiseGetCache
    type.promiseGetCache = function(key) {
        return new Promise((resolve, reject) => {
            type.getCache(key, (err, data) => {
                if (err) {
                    throw err;
                }
                resolve(data);
            });
        });
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

    // promisePersistCache
    type.promisePersistCache = function(key) {
        return new Promise((resolve, reject) => {
            type.persistCache(key, (err) => {
                if (err) {
                    throw err;
                }
                resolve();
            });
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

    // promiseDestroyCache
    type.promiseDestroyCache = function(key) {
        return new Promise((resolve, reject) => {
            type.destroyCache(key, (err) => {
                if (err) {
                    throw err;
                }
                resolve();
            });
        });
    }
}

// generate random key
function keygen(len) {
    var sym = 'abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890';
    var key = '';
    for(var i = 0; i < len; i++) {
        key += sym[parseInt(Math.random() * sym.length)];
    }
    return key;
}

// exports
module.exports = ModelData;
