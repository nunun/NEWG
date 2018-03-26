var util     = require('util');
var GameData = require('./../library/game_data');
var consts   = require('./consts');
var models   = {};
// UserData
// ユーザデータ。サーバ上のみで扱われる公開されないユーザのデータ。
function UserData() {
    this.init();
}
util.inherits(UserData, GameData);
GameData.setupType(UserData, 'UserData', 'db_user_data');
UserData.prototype.init = function() {
    UserData.super_.prototype.init.call(this);
};
UserData.prototype.clear = function() {
    this.uuid = null; // ユーザのUUID型
    this.pid = null; // プレイヤー番号
    this.session_token = null; // セッショントークン
    this.login_token = null; // ログイントークン
}
models.UserData = UserData;
// PlayerData
// プレイヤデータ。全ユーザに公開されるプレイヤーのデータ。
function PlayerData() {
    this.init();
}
util.inherits(PlayerData, GameData);
GameData.setupType(PlayerData, 'PlayerData', 'db_player_data');
PlayerData.prototype.init = function() {
    PlayerData.super_.prototype.init.call(this);
};
PlayerData.prototype.clear = function() {
    this.pid = null; // プレイヤー番号
    this.name = null; // プレイヤー名
}
models.PlayerData = PlayerData;
// SessionData
// セッションデータ
function SessionData() {
    this.init();
}
util.inherits(SessionData, GameData);
GameData.setupType(SessionData, 'SessionData', 'db_session_data');
SessionData.prototype.init = function() {
    SessionData.super_.prototype.init.call(this);
};
SessionData.prototype.clear = function() {
    this.session_token = null; // セッショントークン
}
models.SessionData = SessionData;
// LoginData
// ログインデータ
function LoginData() {
    this.init();
}
util.inherits(LoginData, GameData);
GameData.setupType(LoginData, 'LoginData', 'db_login_data');
LoginData.prototype.init = function() {
    LoginData.super_.prototype.init.call(this);
};
LoginData.prototype.clear = function() {
    this.login_token = null; // ログイントークン
}
models.LoginData = LoginData;
// SampleModel
// サンプルモデル
function SampleModel() {
    this.init();
}
util.inherits(SampleModel, GameData);
GameData.setupType(SampleModel, 'SampleModel', 'db_sample_model');
SampleModel.prototype.init = function() {
    SampleModel.super_.prototype.init.call(this);
};
SampleModel.prototype.clear = function() {
    this.intValue = 100; // 整数型
    this.stringValue1 = "test"; // 文字列型
    this.stringValue2 = null; // 文字列型 (null)
    this.stringValue3 = ""; // 文字列型 (空)
    this.objectValue1 = null; // 型 (null)
    this.objectValue2 = new UserData(); // 型 (空)
    this.arrayValue1 = null; // 配列型 (null)
    this.arrayValue2 = []; // 配列型 (空)
    this.listValue1 = null; // リスト型 (null)
    this.listValue2 = []; // リスト型 (空)
}
models.SampleModel = SampleModel;
module.exports = models;
