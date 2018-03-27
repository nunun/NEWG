var util      = require('util');
var ModelData = require('./../library/model_data');
var consts    = require('./consts');
var models    = {};
// UserData
// ユーザデータ。サーバ上のみで扱われる公開されないユーザのデータ。
function UserData() {
    this.init();
}
util.inherits(UserData, ModelData);
ModelData.setupType(UserData, 'UserData', 'db_user_data');
UserData.prototype.init = function() {
    UserData.super_.prototype.init.call(this);
};
UserData.prototype.clear = function() {
    this.user_id = null; // 固有のユーザID
    this.player_id = null; // 固有のプレイヤID
    this.session_token = null; // セッショントークン
    this.signin_token = null; // ログイントークン
}
models.UserData = UserData;
// PlayerData
// プレイヤデータ。全ユーザに公開されるプレイヤのデータ。
function PlayerData() {
    this.init();
}
util.inherits(PlayerData, ModelData);
ModelData.setupType(PlayerData, 'PlayerData', 'db_player_data');
PlayerData.prototype.init = function() {
    PlayerData.super_.prototype.init.call(this);
};
PlayerData.prototype.clear = function() {
    this.player_id = null; // プレイヤー番号
    this.player_name = null; // プレイヤー名
}
models.PlayerData = PlayerData;
// SessionData
// セッションデータ。本人にのみ知らされるセッション維持に使用するデータ。
function SessionData() {
    this.init();
}
util.inherits(SessionData, ModelData);
ModelData.setupType(SessionData, 'SessionData', 'db_session_data');
SessionData.prototype.init = function() {
    SessionData.super_.prototype.init.call(this);
};
SessionData.prototype.clear = function() {
    this.session_token = null; // セッショントークン
}
models.SessionData = SessionData;
// CredentialData
// 認証データ。本人にのみ知らされるサインイン用データ。セッションが切れた時に使用。
function CredentialData() {
    this.init();
}
util.inherits(CredentialData, ModelData);
ModelData.setupType(CredentialData, 'CredentialData', 'db_credential_data');
CredentialData.prototype.init = function() {
    CredentialData.super_.prototype.init.call(this);
};
CredentialData.prototype.clear = function() {
    this.signin_token = null; // サインイントークン
}
models.CredentialData = CredentialData;
// SampleModel
// サンプルモデル
function SampleModel() {
    this.init();
}
util.inherits(SampleModel, ModelData);
ModelData.setupType(SampleModel, 'SampleModel', 'db_sample_model');
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
