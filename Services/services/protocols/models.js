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
    this.userId = null; // 固有のユーザID
    this.playerId = null; // 固有のプレイヤID
    this.sessionToken = null; // セッショントークン
    this.signinToken = null; // ログイントークン
    this.role = null; // 役職
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
    this.playerId = null; // プレイヤー番号
    this.playerName = null; // プレイヤー名
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
    this.sessionToken = null; // セッショントークン
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
    this.signinToken = null; // サインイントークン
}
models.CredentialData = CredentialData;
// UniqueKeyData
// 固有キー生成用データ。CouchDB のキー重複制限を使って固有キーを生成するために使用。
function UniqueKeyData() {
    this.init();
}
util.inherits(UniqueKeyData, ModelData);
ModelData.setupType(UniqueKeyData, 'UniqueKeyData', 'db_unique_key_data');
UniqueKeyData.prototype.init = function() {
    UniqueKeyData.super_.prototype.init.call(this);
};
UniqueKeyData.prototype.clear = function() {
    this.associatedKey = null; // 固有キーデータに紐づけられた関連キー
}
models.UniqueKeyData = UniqueKeyData;
// ServerStatus
// サーバステータス
function ServerStatus() {
    this.init();
}
util.inherits(ServerStatus, ModelData);
ModelData.setupType(ServerStatus, 'ServerStatus', 'db_server_status');
ServerStatus.prototype.init = function() {
    ServerStatus.super_.prototype.init.call(this);
};
ServerStatus.prototype.clear = function() {
    this.state = null; // 現在状態 (Standby, Ready, Started, Ended)
}
models.ServerStatus = ServerStatus;
// ServerSetupRequest
// サーバセットアップリクエスト。API サーバが Unity サーバに対してサーバインスタンスをセットアップしたいときに送信。
function ServerSetupRequest() {
    this.init();
}
util.inherits(ServerSetupRequest, ModelData);
ModelData.setupType(ServerSetupRequest, 'ServerSetupRequest', 'db_server_setup_request');
ServerSetupRequest.prototype.init = function() {
    ServerSetupRequest.super_.prototype.init.call(this);
};
ServerSetupRequest.prototype.clear = function() {
    this.matchingId = null; // サーバ起動をリクエストしたマッチングID
    this.sceneName = null; // 起動するシーン名
}
models.ServerSetupRequest = ServerSetupRequest;
// ServerSetupResponse
// サーバセットアップレスポンス。ServerBootRequest のレスポンス。
function ServerSetupResponse() {
    this.init();
}
util.inherits(ServerSetupResponse, ModelData);
ModelData.setupType(ServerSetupResponse, 'ServerSetupResponse', 'db_server_setup_response');
ServerSetupResponse.prototype.init = function() {
    ServerSetupResponse.super_.prototype.init.call(this);
};
ServerSetupResponse.prototype.clear = function() {
    this.matchingId = null; // サーバ起動をリクエストしたマッチングID
}
models.ServerSetupResponse = ServerSetupResponse;
// ServerSetupDoneRequest
// サーバセットアップ完了リクエスト。Unity サーバが API サーバに対してクライアント接続可能状態を通知するときに送信。
function ServerSetupDoneRequest() {
    this.init();
}
util.inherits(ServerSetupDoneRequest, ModelData);
ModelData.setupType(ServerSetupDoneRequest, 'ServerSetupDoneRequest', 'db_server_setup_done_request');
ServerSetupDoneRequest.prototype.init = function() {
    ServerSetupDoneRequest.super_.prototype.init.call(this);
};
ServerSetupDoneRequest.prototype.clear = function() {
    this.matchingId = null; // サーバ起動をリクエストしたマッチングID
}
models.ServerSetupDoneRequest = ServerSetupDoneRequest;
// ServerConnectData
// サーバ接続データ
function ServerConnectData() {
    this.init();
}
util.inherits(ServerConnectData, ModelData);
ModelData.setupType(ServerConnectData, 'ServerConnectData', 'db_server_connect_data');
ServerConnectData.prototype.init = function() {
    ServerConnectData.super_.prototype.init.call(this);
};
ServerConnectData.prototype.clear = function() {
    this.serverAddress = "localhost"; // 接続先のサーバアドレス
    this.serverPort = 7777; // 接続先のサーバポート
    this.serverToken = null; // 接続用サーバトークン
}
models.ServerConnectData = ServerConnectData;
module.exports = models;
