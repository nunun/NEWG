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
// マッチングデータ。APIサーバが作成しマッチングサーバが確認する内部連携データ。
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
// MatchConnectData
// マッチ接続データ。マッチングサーバがクライアントに返却するデータ。この情報をもってゲームサーバに接続。
function MatchConnectData() {
    this.init();
}
util.inherits(MatchConnectData, ModelData);
ModelData.setupType(MatchConnectData, 'MatchConnectData', 'db_match_connect_data');
MatchConnectData.prototype.init = function() {
    MatchConnectData.super_.prototype.init.call(this);
};
MatchConnectData.prototype.clear = function() {
    this.matchId = null; // マッチID
    this.serverAddress = "localhost"; // 接続先のゲームサーバアドレス
    this.serverPort = 7777; // 接続先のゲームサーバポート
    this.serverToken = null; // 接続先に必要なサーバトークン
    this.serverSceneName = null; // 接続先のゲームサーバシーン名
}
models.MatchConnectData = MatchConnectData;
// ReserveRequestMessage
// 予約リクエストメッセージ。マッチングサーバがゲームサーバに送信します。
function ReserveRequestMessage() {
    this.init();
}
util.inherits(ReserveRequestMessage, ModelData);
ModelData.setupType(ReserveRequestMessage, 'ReserveRequestMessage', 'db_reserve_request_message');
ReserveRequestMessage.prototype.init = function() {
    ReserveRequestMessage.super_.prototype.init.call(this);
};
ReserveRequestMessage.prototype.clear = function() {
    this.reserveId = null; // 予約ID
    this.sceneName = null; // 起動を希望するシーン名
    this.users = []; // 参加を希望するユーザ情報
}
models.ReserveRequestMessage = ReserveRequestMessage;
// ReserveResponseMessage
// 予約レスポンスメッセージ。ゲームサーバがマッチングサーバに返信します。
function ReserveResponseMessage() {
    this.init();
}
util.inherits(ReserveResponseMessage, ModelData);
ModelData.setupType(ReserveResponseMessage, 'ReserveResponseMessage', 'db_reserve_response_message');
ReserveResponseMessage.prototype.init = function() {
    ReserveResponseMessage.super_.prototype.init.call(this);
};
ReserveResponseMessage.prototype.clear = function() {
    this.reserveId = null; // 予約ID
    this.serverToken = null; // サーバトークン
    this.serverSceneName = null; // サーバシーン名
    this.error = null; // 参加エラー
}
models.ReserveResponseMessage = ReserveResponseMessage;
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
// ErrorData
// エラーデータ。WebAPI やメッセージングでエラーになった場合のレスポンスデータ。
function ErrorData() {
    this.init();
}
util.inherits(ErrorData, ModelData);
ModelData.setupType(ErrorData, 'ErrorData', 'db_error_data');
ErrorData.prototype.init = function() {
    ErrorData.super_.prototype.init.call(this);
};
ErrorData.prototype.clear = function() {
    this.err = null; // エラー詳細情報
}
models.ErrorData = ErrorData;
// ErrorDescriptionData
// エラー詳細データ。実際のエラー情報。
function ErrorDescriptionData() {
    this.init();
}
util.inherits(ErrorDescriptionData, ModelData);
ModelData.setupType(ErrorDescriptionData, 'ErrorDescriptionData', 'db_error_description_data');
ErrorDescriptionData.prototype.init = function() {
    ErrorDescriptionData.super_.prototype.init.call(this);
};
ErrorDescriptionData.prototype.clear = function() {
    this.code = 0; // エラーコード
    this.message = null; // エラーメッセージ
    this.stack = null; // スタックトレース
}
models.ErrorDescriptionData = ErrorDescriptionData;
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
