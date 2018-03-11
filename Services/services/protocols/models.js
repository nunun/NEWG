var util        = require('util');
var CouchClient = require('./../library/couch_client');
var GameData    = require('./../library/game_data');
var consts      = require('./consts');
function Models() {}
util.inherits(Models, function(){});
Models.prototype.migrate = function(callback, recreate) {
    names = [];
    var models = Object.values(this);
    for (var i in models) {
        var name = models[i].getDatabaseScopeName();
        if (name) {
            names.push(name);
        }
    }
    CouchClient.getClient().createDatabases(names, callback, recreate);
}
models = new Models();
// User
// ユーザ情報
function User() {
    this.init();
}
util.inherits(User, GameData);
GameData.setupType(User, 'User', 'users');
User.prototype.init = function() {
    User.super_.prototype.init.call(this);
};
User.prototype.clear = function() {
    this.uuid = null; // ユーザのUUID型
}
models.User = User;
// SampleModel
// サンプルモデル
function SampleModel() {
    this.init();
}
util.inherits(SampleModel, GameData);
GameData.setupType(SampleModel, 'SampleModel', null);
SampleModel.prototype.init = function() {
    SampleModel.super_.prototype.init.call(this);
};
SampleModel.prototype.clear = function() {
    this.intValue = 100; // 整数型
    this.stringValue1 = "test"; // 文字列型
    this.stringValue2 = null; // 文字列型 (null)
    this.stringValue3 = ""; // 文字列型 (空)
    this.objectValue1 = null; // 型 (null)
    this.objectValue2 = new User(); // 型 (空)
    this.arrayValue1 = null; // 配列型 (null)
    this.arrayValue2 = []; // 配列型 (空)
    this.listValue1 = null; // リスト型 (null)
    this.listValue2 = []; // リスト型 (空)
}
models.SampleModel = SampleModel;
module.exports = models;
