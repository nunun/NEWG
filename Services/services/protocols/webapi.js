var webapiClient = require('./../library/webapi_client');
var models       = require('./models');
exports = {};
// signup
// サインアップAPI
exports.signup = function(name, callback, queries = null, forms = null, headers = null) {
    var client = webapiClient.getClient();
    var data = {};
    data["name"] = name; // プレイヤー名
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
// ログインAPI
exports.signin = function(login_token, callback, queries = null, forms = null, headers = null) {
    var client = webapiClient.getClient();
    var data = {};
    data["login_token"] = login_token; // ログイントークン
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
