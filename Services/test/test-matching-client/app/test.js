var assert         = require('assert');
var config         = require('services-library').config;
var logger         = require('services-library').logger;
var mindlinkClient = require('services-library').MindlinkClient.activate(config.mindlinkClient, logger.mindlinkClient);
var matchingClient = require('services-library').WebSocketClient.activate(config.matchingClient, logger.matchingClient);

describe('smoke test', function () {
    describe('smoke test', function () {
        this.timeout(20000);
        it('smoke test', function (done) {
            mindlinkClient.test([
                {connect: function() {
                    mindlinkClient.sendStatus({address:'example.com:7777', population:0, capacity:16}, function(err, responseData) {
                        assert.ok(!err,             'invalid response err (' + err + ')');
                        assert.ok(responseData._ok, 'invalid response responseData._ok');
                        matchingClient.start({user_id:'test'});
                    });
                }}
            ]);

            matchingClient.test([
                {connect: function() {}},
                {data_type0: function(data) {
                    assert.ok(!data.err,                          'invalid response data.err (' + data.err + ')');
                    assert.ok(data.address == 'example.com:7777', 'invalid response data.address');
                }},
                {disconnect: function() {
                    done();
                }},
            ]);

            mindlinkClient.start();
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
