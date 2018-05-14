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
    this.playerId = null; // プレイヤID
    this.playerName = null; // プレイヤ名
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
// MatchingData
// マッチングデータ。マッチングするユーザ一覧などの情報。
function MatchingData() {
    this.init();
}
util.inherits(MatchingData, ModelData);
ModelData.setupType(MatchingData, 'MatchingData', 'db_matching_data');
MatchingData.prototype.init = function() {
    MatchingData.super_.prototype.init.call(this);
};
MatchingData.prototype.clear = function() {
    this.matchingId = null; // マッチングID
    this.users = []; // マッチングするユーザID一覧
}
models.MatchingData = MatchingData;
// MatchData
// マッチデータ。マッチングなどで確定したマッチの情報。
function MatchData() {
    this.init();
}
util.inherits(MatchData, ModelData);
ModelData.setupType(MatchData, 'MatchData', 'db_match_data');
MatchData.prototype.init = function() {
    MatchData.super_.prototype.init.call(this);
};
MatchData.prototype.clear = function() {
    this.matchId = null; // マッチID
    this.matchState = "standby"; // マッチ状態 (standby, ready, started, ended)
    this.serverAddress = "localhost"; // 接続先のゲームサーバアドレス
    this.serverPort = 7777; // 接続先のゲームサーバポート
    this.users = []; // このマッチに参加する全てのユーザID一覧
}
models.MatchData = MatchData;
// MatchConnectData
// マッチ接続データ。この情報をもってゲームサーバに接続。
function MatchConnectData() {
    this.init();
}
util.inherits(MatchConnectData, ModelData);
ModelData.setupType(MatchConnectData, 'MatchConnectData', 'db_match_connect_data');
MatchConnectData.prototype.init = function() {
    MatchConnectData.super_.prototype.init.call(this);
};
MatchConnectData.prototype.clear = function() {
    this.serverAddress = "localhost"; // 接続先のゲームサーバアドレス
    this.serverPort = 7777; // 接続先のゲームサーバポート
    this.matchId = null; // マッチID
}
models.MatchConnectData = MatchConnectData;
// APIServerStatusData
// API サーバ状態データ。API サーバの状態を示すデータ。
function APIServerStatusData() {
    this.init();
}
util.inherits(APIServerStatusData, ModelData);
ModelData.setupType(APIServerStatusData, 'APIServerStatusData', 'db_api_server_status_data');
APIServerStatusData.prototype.init = function() {
    APIServerStatusData.super_.prototype.init.call(this);
};
APIServerStatusData.prototype.clear = function() {
    this.apiServerState = "standby"; // サーバ状態 (standby, ready)
    this.alias = "api"; // サーバエイリアス
}
models.APIServerStatusData = APIServerStatusData;
// MatchingServerStatusData
// マッチングサーバ状態データ。マッチングサーバの状態を示すデータ。
function MatchingServerStatusData() {
    this.init();
}
util.inherits(MatchingServerStatusData, ModelData);
ModelData.setupType(MatchingServerStatusData, 'MatchingServerStatusData', 'db_matching_server_status_data');
MatchingServerStatusData.prototype.init = function() {
    MatchingServerStatusData.super_.prototype.init.call(this);
};
MatchingServerStatusData.prototype.clear = function() {
    this.matchingServerState = "standby"; // サーバ状態 (standby, ready)
    this.matchingServerUrl = "localhost:7755"; // マッチングサーバへの URL
    this.load = 0.0; // サーバ利用率。1.0 は満杯。サーバ人口とキャパシティから計算される。
    this.alias = "matching"; // サーバエイリアス
}
models.MatchingServerStatusData = MatchingServerStatusData;
// ServerStatusData
// サーバ状態データ。サーバの状態を示すデータ。
function ServerStatusData() {
    this.init();
}
util.inherits(ServerStatusData, ModelData);
ModelData.setupType(ServerStatusData, 'ServerStatusData', 'db_server_status_data');
ServerStatusData.prototype.init = function() {
    ServerStatusData.super_.prototype.init.call(this);
};
ServerStatusData.prototype.clear = function() {
    this.serverState = "standby"; // サーバ状態 (standby, ready)
    this.serverAddress = "localhost"; // 接続先のゲームサーバアドレス
    this.serverPort = 7777; // 接続先のゲームサーバポート
    this.load = 0.0; // サーバ利用率。1.0 は満杯。サーバ人口とキャパシティから計算される。
    this.alias = "server"; // サーバエイリアス
}
models.ServerStatusData = ServerStatusData;
// ServerSetupRequestMessage
// サーバセットアップリクエストメッセージ。API サーバが Unity サーバに対してサーバインスタンスをセットアップしたいときに送信。
function ServerSetupRequestMessage() {
    this.init();
}
util.inherits(ServerSetupRequestMessage, ModelData);
ModelData.setupType(ServerSetupRequestMessage, 'ServerSetupRequestMessage', 'db_server_setup_request_message');
ServerSetupRequestMessage.prototype.init = function() {
    ServerSetupRequestMessage.super_.prototype.init.call(this);
};
ServerSetupRequestMessage.prototype.clear = function() {
    this.matchId = null; // サーバ起動をリクエストしたマッチID
    this.sceneName = null; // 起動するシーン名
}
models.ServerSetupRequestMessage = ServerSetupRequestMessage;
// ServerSetupResponseMessage
// サーバセットアップレスポンスメッセージ。ServerSetupRequest のレスポンス。
function ServerSetupResponseMessage() {
    this.init();
}
util.inherits(ServerSetupResponseMessage, ModelData);
ModelData.setupType(ServerSetupResponseMessage, 'ServerSetupResponseMessage', 'db_server_setup_response_message');
ServerSetupResponseMessage.prototype.init = function() {
    ServerSetupResponseMessage.super_.prototype.init.call(this);
};
ServerSetupResponseMessage.prototype.clear = function() {
    this.matchId = null; // サーバ起動をリクエストしたマッチID
}
models.ServerSetupResponseMessage = ServerSetupResponseMessage;
// ServerSetupDoneMessage
// サーバセットアップ完了メッセージ。Unity サーバが API サーバに対してクライアント接続可能状態を通知するときに送信。
function ServerSetupDoneMessage() {
    this.init();
}
util.inherits(ServerSetupDoneMessage, ModelData);
ModelData.setupType(ServerSetupDoneMessage, 'ServerSetupDoneMessage', 'db_server_setup_done_message');
ServerSetupDoneMessage.prototype.init = function() {
    ServerSetupDoneMessage.super_.prototype.init.call(this);
};
ServerSetupDoneMessage.prototype.clear = function() {
    this.matchId = null; // サーバ起動をリクエストしたマッチID
}
models.ServerSetupDoneMessage = ServerSetupDoneMessage;
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
    this.associatedDataType = null; // 固有キーデータに紐づけられたデータタイプ
    this.associatedDataKey = null; // 固有キーデータに紐づけられたデータキー
}
models.UniqueKeyData = UniqueKeyData;
module.exports = models;
