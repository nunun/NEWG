var assert          = require('assert');
var config          = require('libservices').config;
var logger          = require('libservices').logger;
var webSocketClient = require('libservices').WebSocketClient.activate(config.webSocketClient, logger.webSocketClient);
var couchClient     = require('libservices').CouchClient.activate(config.couchClient, logger.couchClient);
var redisClient     = require('libservices').RedisClient.activate(config.redisClient, logger.redisClient);
var mindlinkClient  = require('libservices').MindlinkClient.activate(config.mindlinkClient, logger.mindlinkClient);
var matchingClient  = require('libservices').WebSocketClient.activate(config.matchingClient, logger.matchingClient);

describe('test client', function () {
    describe('websocket client', function () {
        this.timeout(20000);
        it('smoke test', function (done) {
            couchClient.test([
                {connect: function() {
                    var conn = couchClient.getConnection();
                    conn.server.db.destroy(conn.config.db, function(err, body) {
                        assert.ok(!err, 'db destroy error.');
                        conn.server.db.create(conn.config.db, function(err, body) {
                            assert.ok(!err, 'db create error.');
                            conn.insert({happy: true}, function(err, body) {
                                assert.ok(!err, 'document insert error.');
                                redisClient.start();
                            });
                        });
                    });
                }},
            ]);

            redisClient.test([
                {connect: function() {
                    webSocketClient.start();
                }},
            ]);

            webSocketClient.test([
                {connect: function() {
                    webSocketClient.stop();
                }},
                //{data: function(data) {
                //    assert.ok(data.ok, 'invalid response data.ok');
                //    webSocketClient.send({type:webSocketClient.DATA_TYPE.Q, jspath:'.*'});
                //}},
                {disconnect: function() {
                    mindlinkClient.start();
                }},
            ]);

            mindlinkClient.test([
                {connect: function() {
                    mindlinkClient.sendState({address:'example.com:7777', population:0, capacity:16}, function(err, responseData) {
                        assert.ok(!err,            'invalid response err');
                        assert.ok(responseData.ok, 'invalid response responseData.ok');
                        matchingClient.start({user_id:'test'});
                    });
                }}
            ]);

            matchingClient.test([
                {connect: function() {}},
                {data: function(data) {
                    assert.ok(!data.err,                          'invalid response data.err');
                    assert.ok(data.address == 'example.com:7777', 'invalid response data.address');
                }},
                {disconnect: function() {
                    mindlinkClient.stop();
                    redisClient.stop();
                    done();
                }},
            ]);

            couchClient.start();
        });
    });
});

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
var profiles = [{
    // websocket client
    testee: webSocketClient,
    listen: function(check) {
        webSocketClient.on('connect',    function()     { check('connect',    null); });
        webSocketClient.on('disconnect', function()     { check('disconnect', null); });
        webSocketClient.on('data',       function(data) { check('data',       data); });
    },
    unlisten: function() {
        webSocketClient.removeAllListeners('connect');
        webSocketClient.removeAllListeners('disconnect');
        webSocketClient.removeAllListeners('data');
    }
}, {
    // couch client
    testee: couchClient,
    listen: function(check) {
        couchClient.on('connect', function() { check('connect', null); });
    },
    unlisten: function() {
        couchClient.removeAllListeners('connect');
    }
}, {
    // redis client
    testee: redisClient,
    listen: function(check) {
        redisClient.on('connect',    function() { check('connect',    null); });
        redisClient.on('disconnect', function() { check('disconnect', null); });
    },
    unlisten: function() {
        redisClient.removeAllListeners('connect');
        redisClient.removeAllListeners('disconnect');
    }
}, {
    // mindlink client
    testee: mindlinkClient,
    listen: function(check) {
        mindlinkClient.on('connect',    function()     { check('connect',    null); });
        mindlinkClient.on('disconnect', function()     { check('disconnect', null); });
        mindlinkClient.on('data',       function(data) { check('data',       data); });
    },
    unlisten: function() {
        mindlinkClient.removeAllListeners('connect');
        mindlinkClient.removeAllListeners('disconnect');
        mindlinkClient.removeAllListeners('data');
    }
}, {
    // matching client
    testee: matchingClient,
    listen: function(check) {
        matchingClient.on('connect',    function()     { check('connect',    null); });
        matchingClient.on('disconnect', function()     { check('disconnect', null); });
        matchingClient.on('data',       function(data) { check('data',       data); });
    },
    unlisten: function() {
        matchingClient.removeAllListeners('connect');
        matchingClient.removeAllListeners('disconnect');
        matchingClient.removeAllListeners('data');
    }
}];

before(function (done) {
    logger.testClient.debug('[describe] before test')
    this.timeout(20000);

    // set profile testee checkable
    var checkable = function(testee) {
        testee.test = function(sequence) {
            this.sequence = sequence;
        }
        return function(on, data) {
            if (!testee.sequence || !testee.sequence[0]) {
                return;
            }
            var s = testee.sequence.shift();
            var exon = null;
            var excb = null;
            for (var k in s) {
                exon = k;
                excb = s[k]
            }
            assert.ok(exon === on, 'event expected "' + exon + '", but "' + on + '".');
            if (excb) {
                excb(data);
            }
        }
    }

    // initialize profiles
    for (var i in profiles) {
        var profile = profiles[i];
        var testee  = profile.testee;
        profile.unlisten();
        profile.listen(checkable(testee));
    }
    done();
});

after(function (done) {
    logger.testClient.debug('[describe] after test')
    for (var i in profiles) {
        var profile = profiles[i];
        profile.unlisten();
    }
    done();
});

beforeEach(function (done) {
    logger.testClient.debug('[it] before every test');
    done();
});

afterEach(function (done) {
    logger.testClient.debug('[it] after every test')
    done();
});
