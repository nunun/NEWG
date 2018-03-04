exports = {}
exports.setup = function(router, connector) {
    // Test
    // テストインターフェイス
    var Test_impl = connector.Test;
    if (!Test_impl) {
        throw new Error('connector has no implement "Test" for route "/test".');
    }
    router.post("/test", function(req, res) {
        Test_impl(req, res);
    });
}
module.exports = exports;
