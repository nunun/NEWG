exports = {}
exports.setup = function(router, connector) {
    
    // Test
    // テストインターフェイス
    var impl_Test = connector.Test;
    if (!impl_Test) {
        throw new Error('connector has no implement "Test" for route "/test".');
    }
    router.post("/test", function(req, res) {
        var req_data = req.body;
        var res_data = impl_Test(req_data);
        res.json(res_data);
    });
    
}
module.exports = exports;
