var webapiClient = require('./../library/webapi_client');
var models       = require('./models');
exports = {};
// signup
// サインアップAPI
exports.signup = function(playerName, callback, queries = null, forms = null, headers = null) {
    var client = webapiClient.getClient();
    var data = {};
    data["playerName"] = playerName; // プレイヤー名
    return client.post("/signup", data, function(err, responseData) {
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
// signin
// サインインAPI
exports.signin = function(signinToken, callback, queries = null, forms = null, headers = null) {
    var client = webapiClient.getClient();
    var data = {};
    data["signinToken"] = signinToken; // サインイントークン
    return client.post("/signin", data, function(err, responseData) {
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
// test
// ユニットテスト用インターフェイス
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
