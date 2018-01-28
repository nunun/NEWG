var assert          = require('assert');
var config          = require('./config');
var logger          = require('./logger');
var webSocketClient = require('libmindlink').WebSocketClient.activate();
var clients = [webSocketClient];

webSocketClient.setConfig(config.webSocketClient);
webSocketClient.setLogger(logger.webSocketClient);

describe('websocket', function () {
    describe('websocket client', function () {
        this.timeout(10000);
        it('smoke test', function (done) {
            webSocketClient.test([
                {connect: function() {
                    webSocketClient.stop();
                }},
                //{data: function(data) {
                //    assert.ok(data.ok, 'invalid response data.ok');
                //    webSocketClient.send({type:webSocketClient.DATA_TYPE.Q, jspath:'.*'});
                //}},
                {disconnect: function() {
                    done();
                }},
            ]);

            webSocketClient.start();
        });
    });
});

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

before(function (done) {
    logger.testClient.debug('[describe] before test')
    this.timeout(20000);

    // initialize clients
    for (var i in clients) {
        var client = clients[i];
        client.removeAllListeners('start');
        client.removeAllListeners('connect');
        client.removeAllListeners('disconnect');
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
        //client.on('start', function() {
        //    this.check('start', null);
        //});
        client.on('connect', function() {
            this.check('connect', null);
        });
        client.on('disconnect', function() {
            this.check('disconnect', null);
        });
        client.on('data', function(data) {
            this.check('data', data);
        });
    }

    // warm up
    var client = clients[0];
    client.test([
        {connect: function() {
            client.stop();
        }},
        {disconnect: done},
    ]);
    client.start();
});

after(function (done) {
    logger.testClient.debug('[describe] after test')
    for (var i in clients) {
        var client = clients[i];
        client.stop();
        client.removeAllListeners('start');
        client.removeAllListeners('connect');
        client.removeAllListeners('disconnect');
        client.removeAllListeners('data');
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
