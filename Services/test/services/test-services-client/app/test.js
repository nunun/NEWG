var assert          = require('assert');
var config          = require('./services/library/config');
var logger          = require('./services/library/logger');
var mindlinkClient1 = require('./services/library/mindlink_client').activate(config.mindlinkClient1, logger.mindlinkClient);
var mindlinkClient2 = require('./services/library/mindlink_client').activate(config.mindlinkClient2, logger.mindlinkClient);
var mindlinkClient3 = require('./services/library/mindlink_client').activate(config.mindlinkClient3, logger.mindlinkClient);
var webSocketClient = require('./services/library/websocket_client').activate(config.webSocketClient, logger.webSocketClient);
var couchClient     = require('./services/library/couch_client').activate(config.couchClient, logger.couchClient);
var redisClient     = require('./services/library/redis_client').activate(config.redisClient, logger.redisClient);

describe('mindlink', function () {
    describe('mindlink client', function () {
        this.timeout(20000);
        it('smoke test', function (done) {
            mindlinkClient1.test([
                {connect: function() {
                    mindlinkClient1.send(mindlinkClient1.DATA_TYPE.S, {data:'init', alias:'a_client'});
                }},
                {S: function(data) {
                    mindlinkClient1.sendStatus({data:'test', alias:'a_client'}, function(err, responseData) {
                        assert.ok(!err,         'invalid response err (' + err + ')');
                        assert.ok(responseData, 'invalid response responseData');
                        mindlinkClient1.send(mindlinkClient1.DATA_TYPE.Q, {jspath:'.*'});
                    });
                }},
                {Q: function(data) {
                    assert.ok(data.services,                   'invalid response data.services');
                    assert.ok(data.services[0],                'invalid response data.services[0]');
                    assert.ok(data.services[0].clientUuid,     'invalid response data.services[0].clientUuid');
                    assert.ok(data.services[0].serverUuid,     'invalid response data.services[0].serverUuid');
                    assert.ok(data.services[0].data == 'test', 'invalid response data.services[0].data');
                    mindlinkClient2.start();
                }},
                {data_type0: function(data) {
                    assert.ok(data.message == 'test1', 'invalid response data.message');
                    mindlinkClient3.start();
                }},
                {data_type1: function(data) {
                    assert.ok(data.message == 'test2', 'invalid response data.message');
                    mindlinkClient1.sendToRemote('client3', 2, {value:123}, function(err, data) {
                        assert.ok(!err,              'invalid response err (' + err + ')');
                        assert.ok(data,              'invalid response data');
                        assert.ok(data.value == 123, 'invalid response data.value');
                        couchClient.start();
                    });
                }},
            ]);

            mindlinkClient2.test([
                {connect: function() {
                    mindlinkClient2.send(mindlinkClient2.DATA_TYPE.Q, {jspath:'.*'});
                }},
                {Q: function(data) {
                    assert.ok(data.services,                   'invalid response data.services');
                    assert.ok(data.services[0],                'invalid response data.services[0]');
                    assert.ok(data.services[0].clientUuid,     'invalid response data.services[0].clientUuid');
                    assert.ok(data.services[0].serverUuid,     'invalid response data.services[0].serverUuid');
                    assert.ok(data.services[0].data == 'test', 'invalid response data.services[0].data');
                    mindlinkClient2.sendToRemote(data.services[0].clientUuid, 0, {message:'test1'});
                }},
            ]);

            mindlinkClient3.test([
                {connect: function() {
                    mindlinkClient3.sendStatus({alias:'client3'}, function(err, responseData) {
                        mindlinkClient3.send(mindlinkClient3.DATA_TYPE.Q, {jspath:'.*'});
                    });
                }},
                {Q: function(data) {
                    assert.ok(data.services,                   'invalid response data.services');
                    assert.ok(data.services[0],                'invalid response data.services[0]');
                    assert.ok(data.services[0].clientUuid,     'invalid response data.services[0].clientUuid');
                    assert.ok(data.services[0].serverUuid,     'invalid response data.services[0].serverUuid');
                    assert.ok(data.services[0].data == 'test', 'invalid response data.services[0].data');
                    mindlinkClient3.sendToRemote('a_client', 1, {message:'test2'});
                }},
                {data_type2: function(receivedData) {
                    var data = receivedData[0];
                    var res  = receivedData[1];
                    res.send(data);
                }},
            ]);

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
                    webSocketClient.send(0, {value: 10}, function(err, responseData) {
                        assert.ok(!err,                     'invalid response err');
                        assert.ok(responseData,             'invalid response responseData');
                        assert.ok(responseData.value == 10, 'invalid response responseData.value');
                        webSocketClient.send(1, {value: 20});
                    });
                }},
                {data_type1: function(responseData) {
                    assert.ok(responseData,             'invalid response responseData');
                    assert.ok(responseData.value == 20, 'invalid response responseData.value');
                    webSocketClient.stop();
                }},
                {disconnect: function() {
                    done();
                }},
            ]);

            mindlinkClient1.start();
        });
    });
});

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
var profiles = [{
    // mindlink client 1
    testee: mindlinkClient1,
    listen: function(check) {
        mindlinkClient1.setConnectEventListener(function() { check('connect', null); });
        mindlinkClient1.setDisconnectEventListener(function() { check('disconnect', null); });
        mindlinkClient1.setDataEventListener(mindlinkClient1.DATA_TYPE.S, function(data) { check('S', data); });
        mindlinkClient1.setDataEventListener(mindlinkClient1.DATA_TYPE.Q, function(data) { check('Q', data); });
        mindlinkClient1.setDataFromRemoteEventListener(0, function(data, res) { check('data_type0', data); });
        mindlinkClient1.setDataFromRemoteEventListener(1, function(data, res) { check('data_type1', data); });
    },
    unlisten: function() {
        mindlinkClient1.setConnectEventListener(null);
        mindlinkClient1.setDisconnectEventListener(null);
        mindlinkClient1.setDataEventListener(mindlinkClient1.DATA_TYPE.S, null);
        mindlinkClient1.setDataEventListener(mindlinkClient1.DATA_TYPE.Q, null);
        //mindlinkClient1.setDataEventListener(mindlinkClient1.DATA_TYPE.M, null); // NOTE this remove default event listener ...
        mindlinkClient1.stop();
    }
}, {
    // mindlink client 2
    testee: mindlinkClient2,
    listen: function(check) {
        mindlinkClient2.setConnectEventListener(function() { check('connect', null); });
        mindlinkClient2.setDisconnectEventListener(function() { check('disconnect', null); });
        mindlinkClient2.setDataEventListener(mindlinkClient2.DATA_TYPE.S, function(data) { check('S', data); });
        mindlinkClient2.setDataEventListener(mindlinkClient2.DATA_TYPE.Q, function(data) { check('Q', data); });
        //mindlinkClient2.setDataEventListener(mindlinkClient2.DATA_TYPE.M, function(data) { check('M', data); });
    },
    unlisten: function() {
        mindlinkClient2.setConnectEventListener(null);
        mindlinkClient2.setDisconnectEventListener(null);
        mindlinkClient2.setDataEventListener(mindlinkClient2.DATA_TYPE.S, null);
        mindlinkClient2.setDataEventListener(mindlinkClient2.DATA_TYPE.Q, null);
        //mindlinkClient2.setDataEventListener(mindlinkClient2.DATA_TYPE.M, null);
        mindlinkClient2.stop();
    }
}, {
    // mindlink client 3
    testee: mindlinkClient3,
    listen: function(check) {
        mindlinkClient3.setConnectEventListener(function() { check('connect', null); });
        mindlinkClient3.setDisconnectEventListener(function() { check('disconnect', null); });
        mindlinkClient3.setDataEventListener(mindlinkClient3.DATA_TYPE.S, function(data) { check('S', data); });
        mindlinkClient3.setDataEventListener(mindlinkClient3.DATA_TYPE.Q, function(data) { check('Q', data); });
        //mindlinkClient3.setDataEventListener(mindlinkClient3.DATA_TYPE.M, function(data) { check('M', data); });
        mindlinkClient3.setDataFromRemoteEventListener(2, function(data, res) { check('data_type2', [data, res]); });
    },
    unlisten: function() {
        mindlinkClient3.setConnectEventListener(null);
        mindlinkClient3.setDisconnectEventListener(null);
        mindlinkClient3.setDataEventListener(mindlinkClient3.DATA_TYPE.S, null);
        mindlinkClient3.setDataEventListener(mindlinkClient3.DATA_TYPE.Q, null);
        //mindlinkClient3.setDataEventListener(mindlinkClient3.DATA_TYPE.M, null);
        mindlinkClient3.stop();
    }
}, {
    // websocket client
    testee: webSocketClient,
    listen: function(check) {
        webSocketClient.setConnectEventListener(function() { check('connect', null); });
        webSocketClient.setDisconnectEventListener(function() { check('disconnect', null); });
        webSocketClient.setDataEventListener(1, function(data) { check('data_type1', data); });
    },
    unlisten: function() {
        webSocketClient.setConnectEventListener(null);
        webSocketClient.setDisconnectEventListener(null);
        webSocketClient.setDataEventListener(1, null);
        webSocketClient.stop();
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
