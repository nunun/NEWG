exports = {}
exports.setup = function(router, connector) {
    
    // Test
    // テストインターフェイス
    var Test_impl = connector.Test;
    if (!Test_impl) {
        throw new Error('connector has no implement "Test" for route "/test".');
    }
    router.post("/test", function(req, res) {
        // TODO
        // HttpStatus & Auth
        var data = req.body;
        var responseData = Test_impl(data);
        res.json(responseData);
    });
    
}
module.exports = exports;
