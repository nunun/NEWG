exports = {}
exports.setup = function(router, binder, client, logger) {
    if (!client) {
        logger.debug('routes.setup: binding "Test" for route "/test".');
        // Test
        // テストインターフェイス
        var Test_impl = binder.Test;
        if (!Test_impl) {
            logger.error('routes.setup: binder has no implement "Test" for route "/test".');
            return;
        }
        router.post("/test", function(req, res) {
            Test_impl(req, res);
        });
    }
}
module.exports = exports;
