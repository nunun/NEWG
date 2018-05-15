var assert           = require('assert');
var config           = require('./services/library/config');
var logger           = require('./services/library/logger');
var webapi           = require('./services/protocols/webapi');
var models           = require('./services/protocols/models');
var couchClient      = require('./services/library/couch_client').activate(config.couchClient, logger.couchClient);
var redisClient      = require('./services/library/redis_client').activate(config.redisClient, logger.redisClient);
var mindlinkClient   = require('./services/library/mindlink_client').activate(config.mindlinkClient, logger.mindlinkClient);
var webapiClient     = require('./services/library/webapi_client').activate(config.webapiClient, logger.webapiClient);
var matchingClient   = require('./services/library/websocket_client').activate(config.matchingClient, logger.matchingClient);
var ServerStatusData = models.ServerStatusData;
var statusData       = new ServerStatusData();

describe('smoke test', function () {
    describe('smoke test', function () {
        this.timeout(180000);
        it('smoke test', function (done) {
            couchClient.test([
                {connect: function() {
                    couchClient.flushdb(function(err) {
                        assert.ok(!err, 'invalid response err (' + err + ')');
                        redisClient.start();
                    }, true);
                }},
            ]);

            redisClient.test([
                {connect: function() {
                    redisClient.flushdb(function(err) {
                        assert.ok(!err, 'invalid response err (' + err + ')');
                        mindlinkClient.start();
                    });
                }},
            ]);

            mindlinkClient.test([
                {connect: function() {
                    testMatching();
                }}
            ]);

            function testMatching() {
                webapi.signup(function(err, data) {
                    assert.ok(!err, 'invalid response err (' + err + ')');
                    var statusData = new ServerStatusData();
                    statusData.serverState   = "ready";
                    statusData.serverAddress = "example.com";
                    statusData.serverPort    = 7777;
                    statusData.load          = 0.0;
                    statusData.alias         = "server";
                    mindlinkClient.sendStatus(statusData, function(err, responseData) {
                        assert.ok(!err,         'invalid response err (' + err + ')');
                        assert.ok(responseData, 'invalid response responseData (' + responseData + ')');
                        var headers = {SessionToken: data.activeData.sessionData.data.sessionToken};
                        webapi.matching(function(err, data) {
                            assert.ok(!err, 'invalid response err (' + err + ')');
                            var matchingServerUrl = data.matchingServerUrl;
                            matchingClient.start(matchingServerUrl);
                        }, null, null, headers);
                    });
                });
            }

            matchingClient.test([
                {connect: function() {}},
                {data_type0: function(data) {
                    assert.ok(!data.err,                           'invalid response data.err (' + data.err + ')');
                    assert.ok(data.serverAddress == 'example.com', 'invalid response data.serverAddress (' + data.serverAddress + ')');
                    assert.ok(data.serverPort    == '7777',        'invalid response data.serverPort (' + data.serverPort + ')');
                }},
                {disconnect: function() {
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
    // mindlink client
    testee: mindlinkClient,
    listen: function(check) {
        mindlinkClient.setConnectEventListener(function() { check('connect', null); });
        mindlinkClient.setDisconnectEventListener(function() { check('disconnect', null); });
        mindlinkClient.setDataEventListener(mindlinkClient.DATA_TYPE.S, function(data) { check('S', data); });
        mindlinkClient.setDataEventListener(mindlinkClient.DATA_TYPE.Q, function(data) { check('Q', data); });
        mindlinkClient.setDataEventListener(mindlinkClient.DATA_TYPE.M, function(data) { check('M', data); });
    },
    unlisten: function() {
        mindlinkClient.setConnectEventListener(null);
        mindlinkClient.setDisconnectEventListener(null);
        mindlinkClient.setDataEventListener(mindlinkClient.DATA_TYPE.S, null);
        mindlinkClient.setDataEventListener(mindlinkClient.DATA_TYPE.Q, null);
        mindlinkClient.setDataEventListener(mindlinkClient.DATA_TYPE.M, null);
        mindlinkClient.stop();
    }
}, {
    // couch client
    testee: couchClient,
    listen: function(check) {
        couchClient.setConnectEventListener(function() { check('connect', null); });
    },
    unlisten: function() {
        couchClient.setConnectEventListener(null);
    }
}, {
    // redis client
    testee: redisClient,
    listen: function(check) {
        redisClient.setConnectEventListener(function() { check('connect', null); });
        redisClient.setDisconnectEventListener(function() { check('disconnect', null); });
    },
    unlisten: function() {
        redisClient.setConnectEventListener(null);
        redisClient.setDisconnectEventListener(null);
        redisClient.stop();
    }
}, {
    // matching client
    testee: matchingClient,
    listen: function(check) {
        matchingClient.setConnectEventListener(function()     { check('connect',    null); });
        matchingClient.setDisconnectEventListener(function()  { check('disconnect', null); });
        matchingClient.setDataEventListener(0, function(data) { check('data_type0', data); });
    },
    unlisten: function() {
        matchingClient.setConnectEventListener(null);
        matchingClient.setDisconnectEventListener(null);
        matchingClient.setDataEventListener(0, null);
        matchingClient.stop();
    },
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
