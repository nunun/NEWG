var util         = require('util');
var assert       = require('assert');
var config       = require('./services/library/config');
var logger       = require('./services/library/logger');
var webapiClient = require('./services/library/webapi_client').activate(config.webapiClient, logger.webapiClient);
var couchClient  = require('./services/library/couch_client').activate(config.couchClient, logger.couchClient);
var redisClient  = require('./services/library/redis_client').activate(config.redisClient, logger.redisClient);
var GameData     = require('./services/library/game_data');
var webapi       = require('./services/protocols/webapi');

describe('smoke test', function () {
    describe('smoke test', function () {
        this.timeout(20000);
        it('smoke test', function (done) {
            couchClient.test([
                {connect: function() {
                    var conn = couchClient.getConnection();
                    conn.server.db.destroy(conn.config.db, function(err, body) {
                        assert.ok(!err, 'db destroy error.');
                        conn.server.db.create(conn.config.db, function(err, body) {
                            assert.ok(!err, 'db create error.');
                            redisClient.start();
                        });
                    });
                }},
            ]);

            redisClient.test([
                {connect: function() {
                    testWebAPI();
                }},
            ]);

            function testWebAPI() {
                webapi.test(10, function(err, data) {
                    assert.ok(!err, 'invalid response err (' + err + ')');
                    logger.testClient.debug(err)
                    logger.testClient.debug(data)
                    testSaveModel();
                });
            }

            var testId  = null;
            var testRev = null;
            function testSaveModel() {
                var testData = new TestData();
                testData.num = 100;
                testData.save('test', function(err, id, rev) {
                    assert.ok(!err,         'invalid response err (' + err + ')');
                    assert.ok(id == 'test', 'invalid response id ('  + id  + ')');
                    assert.ok(rev,          'invalid response rev (' + rev + ')');
                    testId  = id;
                    testRev = rev;
                    testGetModel();
                });
            }

            function testGetModel() {
                TestData.get(testId, function(err, testData) {
                    assert.ok(!err,                     'invalid response err ('           + err           + ')');
                    assert.ok(testData._id  == testId,  'invalid response testData._id ('  + testData._id  + ')');
                    assert.ok(testData._rev == testRev, 'invalid response testData._rev (' + testData._rev + ')');
                    assert.ok(testData.num  == 100,     'invalid response testData.num ('  + testData.num  + ')');
                    testListModel();
                });
            }

            function testListModel() {
                TestData.list({num: 100}, function(err, list) {
                    assert.ok(!err,                    'invalid response err (' + err + ')');
                    assert.ok(list.length  == 1,       'invalid response list.length ('  + list.length  + ')');
                    assert.ok(list[0]._id  == testId,  'invalid response list[0]._id ('  + list[0]._id  + ')');
                    assert.ok(list[0]._rev == testRev, 'invalid response list[0]._rev (' + list[0]._rev + ')');
                    assert.ok(list[0].num  == 100,     'invalid response list[0].num ('  + list[0].num  + ')');

                    var testData = list[0].activate();
                    assert.ok(testData._id  == testId,  'invalid response testData[0]._id ('  + testData._id  + ')');
                    assert.ok(testData._rev == testRev, 'invalid response testData[0]._rev (' + testData._rev + ')');
                    assert.ok(testData.num  == 100,     'invalid response testData[0].num ('  + testData.num  + ')');

                    testDestroyModel();
                });
            }

            function testDestroyModel() {
                TestData.destroy(testId, testRev, function(err) {
                    assert.ok(!err, 'invalid response err (' + err + ')');
                    TestData.list({num: 100}, function(err, list) {
                        assert.ok(!err,              'invalid response err ('          + err          + ')');
                        assert.ok(list.length  == 0, 'invalid response list.length ('  + list.length  + ')');
                        done();
                    });
                });
            }

            couchClient.start();
        });
    });
});

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
var profiles = [{
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

// TestData
function TestData() {
    this.init();
}
util.inherits(TestData, GameData);
GameData.setupType('TestData', TestData);
TestData.prototype.init = function() {
    TestData.super_.prototype.init.call(this);
};
TestData.prototype.clear = function() {
    this.value = 0;
}

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
