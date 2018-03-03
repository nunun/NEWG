var client = require('services_library').WebAPIClient.getClient();
exports = {};

// Test
// テストインターフェイス
exports.test = function(value, callback) {
    var req_data = {};
    req_data["value"] = value;
    return client.post("/test", req_data, function(err, res_data) {
        if (err) {
            if (callback != null) {
                callback(err, null);
            }
            return;
        }
        if (callback != null) {
            callback(res_data["value"]);
        }
    });
}

module.exports = exports;
