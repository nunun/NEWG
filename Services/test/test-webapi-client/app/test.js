var assert = require('assert');
var config = require('./services/library/config');
var logger = require('./services/library/logger');

describe('smoke test', function () {
    describe('smoke test', function () {
        this.timeout(20000);
        it('smoke test', function (done) {
            // TODO
            // mindlinkClient.test([
            //     {connect: function() {
            //         mindlinkClient.sendStatus({address:'example.com:7777', population:0, capacity:16}, function(err, responseData) {
            //             assert.ok(!err,         'invalid response err (' + err + ')');
            //             assert.ok(responseData, 'invalid response responseData');
            //             matchingClient.start({user_id:'test'});
            //         });
            //     }}
            // ]);
            //
            // matchingClient.test([
            //     {connect: function() {}},
            //     {data_type0: function(data) {
            //         assert.ok(!data.err,                          'invalid response data.err (' + data.err + ')');
            //         assert.ok(data.address == 'example.com:7777', 'invalid response data.address');
            //     }},
            //     {disconnect: function() {
            //         done();
            //     }},
            // ]);
            //
            // mindlinkClient.start();
            done();
        });
    });
});

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
before(function (done) {
    logger.testClient.debug('[describe] before test')
    this.timeout(20000);
    done();
});

after(function (done) {
    logger.testClient.debug('[describe] after test')
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
