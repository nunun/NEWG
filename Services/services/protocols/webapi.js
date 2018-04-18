var webapiClient = require('./../library/webapi_client');
var models       = require('./models');
exports = {};
// signup
// サインアップAPI
exports.signup = function(callback, queries = null, forms = null, headers = null) {
    var client = webapiClient.getClient();
    var data = {};
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
// matching
// マッチングをリクエスト
exports.matching = function(callback, queries = null, forms = null, headers = null) {
    var client = webapiClient.getClient();
    var data = {};
    return client.post("/matching", data, function(err, responseData) {
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
// player
// プレイヤー情報の取得
exports.player = function(callback, queries = null, forms = null, headers = null) {
    var client = webapiClient.getClient();
    var data = {};
    return client.post("/player", data, function(err, responseData) {
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
// ユニットテスト用
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
