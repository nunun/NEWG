var client = require('./../library/webapi_client').getClient();
exports = {};

// test
// テストインターフェイス
exports.test = function(test, callback, queries = null, forms = null, headers = null) {
    var data = {};
    data["reqValue"] = reqValue;
    return client.post("/test", data, function(err, responseData) {
        if (err) {
            if (callback != null) {
                callback(err, null);
            }
            return;
        }
        if (callback != null) {
            callback(responseData["test"]);
        }
    }, queries, forms, headers);
}

module.exports = exports;
