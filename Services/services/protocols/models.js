var consts = require('./consts');
exports = {};
var models = exports;
// User
// ユーザ情報
exports.User = function() {
    var model = {};
    model.uuid = null; // ユーザのUUID型
    return model;
}
// SampleModel
// テストモデル
exports.SampleModel = function() {
    var model = {};
    model.intValue = 100; // 整数型
    model.stringValue1 = "test"; // 文字列型
    model.stringValue2 = null; // 文字列型 (null)
    model.stringValue3 = ""; // 文字列型 (空)
    model.objectValue1 = null; // 型 (null)
    model.objectValue2 = models.User(); // 型 (空)
    model.arrayValue1 = null; // 配列型 (null)
    model.arrayValue2 = []; // 配列型 (空)
    model.listValue1 = null; // リスト型 (null)
    model.listValue2 = []; // リスト型 (空)
    return model;
}
module.exports = exports;
