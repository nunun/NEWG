var assert          = require('assert');
var config          = require('./config');
var logger          = require('./logger');
var webSocketClient = require('libservices').WebSocketClient.activate(config.webSocketClient, logger.webSocketClient);
var redisClient     = require('libservices').RedisClient.activate(config.redisClient, logger.redisClient);

describe('test client', function () {
    describe('websocket client', function () {
        this.timeout(10000);
        it('smoke test', function (done) {
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
                    redisClient.stop();
                    done();
                }},
            ]);

            redisClient.start();
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
        //webSocketClient.on('start',    function()     { check('start',      null); });
        webSocketClient.on('connect',    function()     { check('connect',    null); });
        webSocketClient.on('disconnect', function()     { check('disconnect', null); });
        webSocketClient.on('data',       function(data) { check('data',       data); });
    },
    unlisten: function() {
        //webSocketClient.removeAllListeners('start');
        webSocketClient.removeAllListeners('connect');
        webSocketClient.removeAllListeners('disconnect');
        webSocketClient.removeAllListeners('data');
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
