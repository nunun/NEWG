var assert       = require('assert');
var config       = require('./services/library/config');
var logger       = require('./services/library/logger');
var couchClient  = require('./services/library/couch_client').activate(config.couchClient, logger.couchClient);
var redisClient  = require('./services/library/redis_client').activate(config.redisClient, logger.redisClient);
var webapiClient = require('./services/library/webapi_client').activate(config.webapiClient, logger.webapiClient);
var webapi       = require('./services/protocols/webapi');

describe('smoke test', function () {
    describe('smoke test', function () {
        this.timeout(20000);
        it('smoke test', function (done) {
            webapi.test(10, function(err, data) {
                assert.ok(!err, 'invalid response err (' + err + ')');
                logger.testClient.debug(err)
                logger.testClient.debug(data)
                done();
            });
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
