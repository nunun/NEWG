var consts = require('./consts');
exports = {};
// MyModel
// テストモデル
exports.MyModel = function() {
    var model = {};
    model.intValue = 0; // 整数型
    model.stringValue = null; // 文字列型
    return model;
}
// User
// ユーザ情報
exports.User = function() {
    var model = {};
    model.uuid = null; // ユーザのUUID型
    return model;
}
module.exports = exports;
