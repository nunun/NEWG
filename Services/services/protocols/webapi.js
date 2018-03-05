var webapiClient = require('./../library/webapi_client');
var models       = require('./models');
exports = {};
// test
// テストインターフェイス
exports.test = function(reqValue, callback, queries = null, forms = null, headers = null) {
    var client = webapiClient.getClient();
    var data = {};
    data["reqValue"] = reqValue; // リクエストの値
    return client.post("/test", data, function(err, responseData) {
        if (err) {
            if (callback != null) {
                callback(err, null);
            }
            return;
        }
        if (callback != null) {
            callback(err, responseData);
        }
    }, queries, forms, headers);
}
module.exports = exports;
