var util              = require('util');
var redis             = require('redis');
var assert            = require('assert');
var instanceContainer = require('./instance_container').activate();

// constructor
function RedisClient(config, logger) {
    this.init(config, logger);
}
util.inherits(RedisClient, function(){});

// init
RedisClient.prototype.init = function(config, logger) {
    this.config = config; // config
    this.logger = logger; // logger

    // event listeners
    this.startEventListener      = null; // start
    this.connectEventListener    = null; // connect
    this.disconnectEventListener = null; // disconnect

    // clear
    this.clear();
};

// clear
RedisClient.prototype.clear = function() {
    // redis client
    this.redis = null; // internal redis connection
};

// start
RedisClient.prototype.start = function() {
    var self = this;
    self.logger.info('start');
    var connectUrl     = self.config.url            || null;
    var retryCount     = self.config.retryCount     || 10;
    var retryInterval  = self.config.retryInterval  || 3000;
    var retryExtend    = self.config.retryExtend    || 1000;
    var retryTotal     = self.config.retryTotal     || (1000 * 60 * 60);

    // setup
    self.clear();

    // create redis connection
    self.redis = redis.createClient(connectUrl, {
        retry_strategy: function(options) {
            if (options.error && options.error.code === 'ECONNREFUSED') {
                return new Error('The server refused the connection');
            }
            if (options.total_retry_time > retryTotal) {
                return new Error('Retry time exhausted');
            }
            if (options.attempt > retryCount) {
                return undefined; // End reconnecting with built in error
            }
            self.logger.info('reconnecting ...');
            return Math.min(options.attempt * retryExtend, retryInterval); // reconnect after
        }
    });

    // on connect
    self.redis.on('connect', function() {
        self.logger.debug('on connect');
        if (self.connectEventListener) {
            self.connectEventListener();
        }
    });

    // on end
    self.redis.on('end', function() {
        self.logger.debug('on end');
        var disconnectEventListener = self.disconnectEventListener;
        self.clear();
        if (disconnectEventListener) {
            disconnectEventListener();
        }
    });

    // on error
    self.redis.on('error', function(e) {
        self.logger.error('on error: e[' + e.toString() + ']');
    });

    // start
    if (self.startEventListener) {
        self.startEventListener();
    }
}

// stop
RedisClient.prototype.stop = function() {
    if (!this.redis) {
        return;
    }
    this.redis.quit();
}

// get connection
RedisClient.prototype.getConnection = function() {
    return this.redis;
}

// flushdb
RedisClient.prototype.flushdb = function(callback) {
    var conn = this.getConnection();
    conn.flushdb(function(err, reply) {
        if (err) {
            if (callback) {
                callback(err);
            }
            return;
        }
        if (callback) {
            callback((reply == "OK")? null : new Error('flushdb failed'));
        }
    });
}

// set start event listener
RedisClient.prototype.setStartEventListener = function(eventListener) {
    this.startEventListener = eventListener;
}

// set connect event listener
RedisClient.prototype.setConnectEventListener = function(eventListener) {
    this.connectEventListener = eventListener;
}

// set disconnect event listener
RedisClient.prototype.setDisconnectEventListener = function(eventListener) {
    this.disconnectEventListener = eventListener;
}

// get client
RedisClient.getClient = function(clientName) {
    return instanceContainer.find(clientName);
}

// activator
RedisClient.activate = function(config, logger) {
    client = new RedisClient(config, logger);
    if (config) {
        instanceContainer.add(config.clientName, client);
    }
    return client;
}

module.exports = RedisClient;

//// get random resource key
//RedisClient.prototype.getRandomResourceKey = function(callback) {
//    logger.redisClient.debug('[randomkey]');
//    this.redis.RANDOMKEY([], function(err, reply) {
//        if (err || !reply || reply == "") {
//            callback(null);
//            return;
//        }
//        logger.redisClient.debug(reply);
//        callback(reply);
//    });
//}
//// set resource
//RedisClient.prototype.setResource = function(name, uuid, callback) {
//    logger.redisClient.debug('[setnx] name="' + name + '", uuid="' + uuid + '".');
//    assert.ok((name.indexOf(':') !== 0), 'invalid resource name prefixed ":".');
//    this.redis.set(name, uuid, 'NX', function(err, reply) {
//        if (err) {
//            logger.redisClient.error(err);
//            if (callback) {
//                callback(false);
//            }
//            return;
//        }
//        logger.redisClient.debug(reply);
//        if (callback) {
//            callback((reply == "OK")? true : false);
//        }
//    });
//}
//// delete resources
//RedisClient.prototype.deleteResources = function(names, callback) {
//    logger.redisClient.debug('[del] names="' + names + '".');
//    for (var i in names) {
//        if (names[i].indexOf(':') === 0) {
//            assert.ok(false, 'invalid resource name prefixed ":".');
//        }
//    }
//    this.redis.del(names, function(err, reply) {
//        if (err) {
//            logger.redisClient.error(err);
//            if (callback) {
//                callback(false);
//            }
//            return;
//        }
//        logger.redisClient.debug(reply);
//        if (callback) {
//            callback((reply == "1")? true : false); // always true with no error
//        }
//    });
//}
//// set resource ttl
//RedisClient.prototype.setResourceTTL = function(name) {
//    var self = this;
//
//    // try mark 'checked'.
//    var checkTTL = config.redisClient.resourceCleaning.checkTTL;
//    logger.redisClient.debug('[setnx] name="' + name + '", checkTTL="' + checkTTL + '".');
//    assert.ok((name.indexOf(':') !== 0), 'invalid resource name prefixed ":".');
//    self.redis.set(':check:' + name, 'checked', 'NX', 'EX', checkTTL, function(err, reply) {
//        if (err) {
//            logger.redisClient.error(err);
//            return;
//        }
//        logger.redisClient.debug(reply);
//        if (reply != "OK") {
//            return; // NOTE you could not set ttl because check already started.
//        }
//
//        // set delete ttl.
//        var deleteTTL = config.redisClient.resourceCleaning.deleteTTL;
//        logger.redisClient.debug('[expire] name="' + name + '", deleteTTL="' + deleteTTL + '".');
//        self.redis.expire(name, deleteTTL, function(err, reply) {
//            if (err) {
//                logger.redisClient.error(err);
//                return;
//            }
//            logger.redisClient.debug(reply);
//            if (reply != "1") {
//                return; // NOTE failed to get uuid.
//            }
//
//            // get uuid and broadcast.
//            logger.redisClient.debug('[get] name="' + name + '".');
//            self.redis.get(name, function(err, reply) {
//                if (err) {
//                    logger.redisClient.error(err);
//                    return;
//                }
//                logger.redisClient.debug(reply);
//                var uuid = reply;
//            });
//        });
//    });
//}
//// cancel resource ttl
//RedisClient.prototype.cancelResourceTTL = function(name) {
//    logger.redisClient.debug('[persist] name="' + name + '".');
//    assert.ok((name.indexOf(':') !== 0), 'invalid resource name prefixed ":".');
//    this.redis.persist(name, function(err, reply) {
//        if (err) {
//            logger.redisClient.debug(err);
//            return;
//        }
//        logger.redisClient.debug(reply);
//    });
//}
//// flushdb
//RedisClient.prototype.flushdb = function(callback) {
//    logger.redisClient.debug('[flushdb]');
//    this.redis.flushdb(function(err, reply) {
//        if (err) {
//            logger.redisClient.debug(err);
//            if (callback) {
//                callback(false);
//            }
//            return;
//        }
//        logger.redisClient.debug(reply);
//        callback((reply == "OK")? true : false);
//    });
//}
//// start resource cleaning
//RedisClient.prototype.startResourceCleaning = function() {
//    var self          = this;
//    var checkInterval = config.redisClient.resourceCleaning.checkInterval;
//
//    // stop resource cleaning
//    self.stopResourceCleaning();
//
//    // check interval
//    setTimeout(function() {
//        setInterval(function() {
//            logger.redisClient.debug('resource cleaning ...');
//            self.getRandomResourceKey(function(key) {
//                if (!key) {
//                    return;
//                }
//                if (key.indexOf(":") === 0) {
//                    return;
//                }
//                self.setResourceTTL(key);
//            });
//        }, checkInterval);
//    }, (Math.random() * checkInterval));
//}
//// stop resource cleaning
//RedisClient.prototype.stopResourceCleaning = function() {
//    if (this.checkIntervalId) {
//        clearInterval(this.checkIntervalId);
//    }
//    this.checkIntervalId = null;
//}
