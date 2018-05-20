var assert                     = require('assert');
var config                     = require('./services/library/config');
var logger                     = require('./services/library/logger');
var webapi                     = require('./services/protocols/webapi');
var models                     = require('./services/protocols/models');
var mindlinkClient             = require('./services/library/mindlink_client').activate(config.mindlinkClient, logger.mindlinkClient);
var webapiClient               = require('./services/library/webapi_client').activate(config.webapiClient, logger.webapiClient);
var matchingClient             = require('./services/library/websocket_client').activate(config.matchingClient, logger.matchingClient);
var ServerStatusData           = models.ServerStatusData;
var ServerSetupResponseMessage = models.ServerSetupResponseMessage;
var statusData                 = new ServerStatusData();

describe('smoke test', function() {
    describe('smoke test', function () {
        this.timeout(180000);
        it('smoke test', function(done) {
            mindlinkClient.setDataFromRemoteEventListener(0, function(data,res) {
                assert.ok(!data.err, 'invalid response data.err (' + data.err + ')');
                assert.ok(res.to,    'invalid response res.to ('   + res.to   + ')');
                logger.testClient.debug('send ServerSetupResponseMessage to "' + res.to + '"')
                var serverSetupResponseMessage = new ServerSetupResponseMessage();
                serverSetupResponseMessage.matchId = data.matchId;
                mindlinkClient.sendToRemote(res.to, 0, serverSetupResponseMessage);
            });

            mindlinkClient.test([
                {connect: function() {
                    waitMatching();
                }},
            ]);

            function waitMatching() {
                mindlinkClient.sendQuery('.*{.alias=="matching"}', function(err,services) {
                    if (err || !services || services.length <= 0) {
                        logger.testClient.debug('waiting for matching server ...')
                        setTimeout(function() {
                            waitMatching();
                        }, 1000);
                        return;
                    }
                    logger.testClient.debug('matching server up detected.')
                    testMatching();
                });
            }

            function testMatching() {
                webapi.signup(function(err, data) {
                    assert.ok(!err, 'invalid response err (' + err + ')');
                    var statusData = new ServerStatusData();
                    statusData.serverState   = "standby";
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
        //mindlinkClient.setDataEventListener(mindlinkClient.DATA_TYPE.M, function(data) { check('M', data); });
    },
    unlisten: function() {
        mindlinkClient.setConnectEventListener(null);
        mindlinkClient.setDisconnectEventListener(null);
        mindlinkClient.setDataEventListener(mindlinkClient.DATA_TYPE.S, null);
        mindlinkClient.setDataEventListener(mindlinkClient.DATA_TYPE.Q, null);
        //mindlinkClient.setDataEventListener(mindlinkClient.DATA_TYPE.M, null);
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