var assert          = require('assert');
var config          = require('./config');
var logger          = require('./logger');
var MindlinkClient  = require('./mindlink_client');

var mindlinkClient1 = new MindlinkClient(config.mindlinkClient1);
var mindlinkClient2 = new MindlinkClient(config.mindlinkClient2);
var mindlinkClient3 = new MindlinkClient(config.mindlinkClient3);
var mindlinkClients = [mindlinkClient1, mindlinkClient2, mindlinkClient3];

describe('mindlink', function () {
    describe('mindlink client', function () {
        this.timeout(10000);
        it('smoke test', function (done) {
            mindlinkClient1.test([
                {join: function() {
                    mindlinkClient1.send({data:'test'});
                }},
                {data: function(data) {
                    assert.ok(data.ok, 'invalid response data.ok');
                    mindlinkClient1.send({cmd:'q', jspath:'.*'});
                }},
                {data: function(data) {
                    assert.ok(data.ok,                         'invalid response data.ok');
                    assert.ok(data.clientUuid,                 'invalid response data.clientUuid');
                    assert.ok(data.serverUuid,                 'invalid response data.serverUuid');
                    assert.ok(data.services,                   'invalid response data.services');
                    assert.ok(data.services[0].clientUuid,     'invalid response data.services.clientUuid');
                    assert.ok(data.services[0].serverUuid,     'invalid response data.services.serverUuid');
                    assert.ok(data.services[0],                'invalid response data.services[0]');
                    assert.ok(data.services[0].data == 'test', 'invalid response data.services[0].data');
                    mindlinkClient1.uuid = data.clientUuid;
                    mindlinkClient2.start();
                }},
                {data: function(data) {
                    assert.ok(data.ok,                         'invalid response data.ok');
                    assert.ok(data.cmd == 'm',                 'invalid response data.cmd');
                    assert.ok(data.to == mindlinkClient1.uuid, 'invalid response data.to');
                    assert.ok(data.from,                       'invalid response data.from');
                    assert.ok(data.message == 'test1',         'invalid response data.message');
                    mindlinkClient3.start();
                }},
                {data: function(data) {
                    assert.ok(data.ok,                         'invalid response data.ok');
                    assert.ok(data.cmd == 'm',                 'invalid response data.cmd');
                    assert.ok(data.to == mindlinkClient1.uuid, 'invalid response data.to');
                    assert.ok(data.from,                       'invalid response data.from');
                    assert.ok(data.message == 'test2',         'invalid response data.message');
                    done();
                }},
            ]);

            mindlinkClient2.test([
                {join: function() {
                    mindlinkClient2.send({cmd:'q', jspath:'.*'});
                }},
                {data: function(data) {
                    assert.ok(data.ok,                         'invalid response data.ok');
                    assert.ok(data.services,                   'invalid response data.services');
                    assert.ok(data.services[0].clientUuid,     'invalid response data.services.clientUuid');
                    assert.ok(data.services[0].serverUuid,     'invalid response data.services.serverUuid');
                    assert.ok(data.services[0],                'invalid response data.services[0]');
                    assert.ok(data.services[0].data == 'test', 'invalid response data.services[0].data');
                    mindlinkClient2.send({cmd:'m', to:data.services[0].clientUuid, message:'test1'});
                }},
            ]);

            mindlinkClient3.test([
                {join: function() {
                    mindlinkClient3.send({cmd:'q', jspath:'.*'});
                }},
                {data: function(data) {
                    assert.ok(data.ok,                         'invalid response data.ok');
                    assert.ok(data.services,                   'invalid response data.services');
                    assert.ok(data.services[0].clientUuid,     'invalid response data.services.clientUuid');
                    assert.ok(data.services[0].serverUuid,     'invalid response data.services.serverUuid');
                    assert.ok(data.services[0],                'invalid response data.services[0]');
                    assert.ok(data.services[0].data == 'test', 'invalid response data.services[0].data');
                    mindlinkClient3.send({cmd:'m', to:data.services[0].clientUuid, message:'test2'});
                }},
            ]);

            mindlinkClient1.start();
        });
    });
});

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

before(function (done) {
    logger.test.debug('[describe] before test')
    this.timeout(20000);

    // initialize clients
    for (var i in mindlinkClients) {
        var client = mindlinkClients[i];
        client.removeAllListeners('join');
        client.removeAllListeners('leave');
        client.removeAllListeners('data');
        client.test = function(sequence) {
            this.sequence = sequence;
        }
        client.check = function(on, data) {
            if (!this.sequence || !this.sequence[0]) {
                return;
            }
            var s = this.sequence.shift();
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
        client.on('join', function() {
            this.check('join', null);
        })
        client.on('leave', function() {
            this.check('leave', null);
        })
        client.on('data', function(data) {
            this.check('data', data);
        })
    }

    // warm up
    // wait for mindlink server and flush redis.
    mindlinkClient1.test([
        {join: function() {
            mindlinkClient1.stop();
        }},
        {leave: done},
    ]);
    mindlinkClient1.start();
});

after(function (done) {
    logger.test.debug('[describe] after test')
    for (var i in mindlinkClients) {
        var client = mindlinkClients[i];
        client.stop();
        client.removeAllListeners('join');
        client.removeAllListeners('leave');
        client.removeAllListeners('data');
    }
    done();
});

beforeEach(function (done) {
    logger.test.debug('[it] before every test');
    for (var i in mindlinkClients) {
        var client = mindlinkClients[i];
        client.stop();
        client.test([]);
    }
    done();
});

afterEach(function (done) {
    logger.test.debug('[it] after every test')
    for (var i in mindlinkClients) {
        var client = mindlinkClients[i];
        client.stop();
        client.test([]);
    }
    done();
});
