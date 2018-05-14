using System;
using System.Collections;
using System.Collections.Generic;
using Services.Protocols.Consts;
using UnityEngine;
namespace Services.Protocols.Models {
    // UserData
    // ユーザデータ。サーバ上のみで扱われる公開されないユーザのデータ。
    [Serializable]
    public class UserData : ModelData {
        public string userId; // 固有のユーザID
        public string playerId; // 固有のプレイヤID
        public string sessionToken; // セッショントークン
        public string signinToken; // ログイントークン
        public string role; // 役職
        protected override void Clear() {
            userId = null; // 固有のユーザID
            playerId = null; // 固有のプレイヤID
            sessionToken = null; // セッショントークン
            signinToken = null; // ログイントークン
            role = null; // 役職
        }
    }
    // PlayerData
    // プレイヤデータ。全ユーザに公開されるプレイヤのデータ。
    [Serializable]
    public class PlayerData : ModelData {
        public string playerId; // プレイヤID
        public string playerName; // プレイヤ名
        protected override void Clear() {
            playerId = null; // プレイヤID
            playerName = null; // プレイヤ名
        }
    }
    // SessionData
    // セッションデータ。本人にのみ知らされるセッション維持に使用するデータ。
    [Serializable]
    public class SessionData : ModelData {
        public string sessionToken; // セッショントークン
        protected override void Clear() {
            sessionToken = null; // セッショントークン
        }
    }
    // CredentialData
    // 認証データ。本人にのみ知らされるサインイン用データ。セッションが切れた時に使用。
    [Serializable]
    public class CredentialData : ModelData {
        public string signinToken; // サインイントークン
        protected override void Clear() {
            signinToken = null; // サインイントークン
        }
    }
    // MatchingData
    // マッチングデータ。マッチングするユーザ一覧などの情報。
    [Serializable]
    public class MatchingData : ModelData {
        public string matchingId; // マッチングID
        public string[] users; // マッチングするユーザID一覧
        protected override void Clear() {
            matchingId = null; // マッチングID
            users = new string[0]; // マッチングするユーザID一覧
        }
    }
    // MatchData
    // マッチデータ。マッチングなどで確定したマッチの情報。
    [Serializable]
    public class MatchData : ModelData {
        public string matchId; // マッチID
        public string matchState; // マッチ状態 (standby, ready, started, ended)
        public string serverAddress; // 接続先のゲームサーバアドレス
        public int serverPort; // 接続先のゲームサーバポート
        public string[] users; // このマッチに参加する全てのユーザID一覧
        protected override void Clear() {
            matchId = null; // マッチID
            matchState = "standby"; // マッチ状態 (standby, ready, started, ended)
            serverAddress = "localhost"; // 接続先のゲームサーバアドレス
            serverPort = 7777; // 接続先のゲームサーバポート
            users = new string[0]; // このマッチに参加する全てのユーザID一覧
        }
    }
    // MatchConnectData
    // マッチ接続データ。この情報をもってゲームサーバに接続。
    [Serializable]
    public class MatchConnectData : ModelData {
        public string serverAddress; // 接続先のゲームサーバアドレス
        public int serverPort; // 接続先のゲームサーバポート
        public string matchId; // マッチID
        protected override void Clear() {
            serverAddress = "localhost"; // 接続先のゲームサーバアドレス
            serverPort = 7777; // 接続先のゲームサーバポート
            matchId = null; // マッチID
        }
    }
    // APIServerStatusData
    // API サーバ状態データ。API サーバの状態を示すデータ。
    [Serializable]
    public class APIServerStatusData : ModelData {
        public string apiServerState; // サーバ状態 (standby, ready)
        public string alias; // サーバエイリアス
        protected override void Clear() {
            apiServerState = "standby"; // サーバ状態 (standby, ready)
            alias = "api"; // サーバエイリアス
        }
    }
    // MatchingServerStatusData
    // マッチングサーバ状態データ。マッチングサーバの状態を示すデータ。
    [Serializable]
    public class MatchingServerStatusData : ModelData {
        public string matchingServerState; // サーバ状態 (standby, ready)
        public string matchingServerUrl; // マッチングサーバへの URL
        public float load; // サーバ利用率。1.0 は満杯。サーバ人口とキャパシティから計算される。
        public string alias; // サーバエイリアス
        protected override void Clear() {
            matchingServerState = "standby"; // サーバ状態 (standby, ready)
            matchingServerUrl = "localhost:7755"; // マッチングサーバへの URL
            load = 0.0f; // サーバ利用率。1.0 は満杯。サーバ人口とキャパシティから計算される。
            alias = "matching"; // サーバエイリアス
        }
    }
    // ServerStatusData
    // サーバ状態データ。サーバの状態を示すデータ。
    [Serializable]
    public class ServerStatusData : ModelData {
        public string serverState; // サーバ状態 (standby, ready)
        public string serverAddress; // 接続先のゲームサーバアドレス
        public int serverPort; // 接続先のゲームサーバポート
        public float load; // サーバ利用率。1.0 は満杯。サーバ人口とキャパシティから計算される。
        public string alias; // サーバエイリアス
        protected override void Clear() {
            serverState = "standby"; // サーバ状態 (standby, ready)
            serverAddress = "localhost"; // 接続先のゲームサーバアドレス
            serverPort = 7777; // 接続先のゲームサーバポート
            load = 0.0f; // サーバ利用率。1.0 は満杯。サーバ人口とキャパシティから計算される。
            alias = "server"; // サーバエイリアス
        }
    }
    // ServerSetupRequestMessage
    // サーバセットアップリクエストメッセージ。API サーバが Unity サーバに対してサーバインスタンスをセットアップしたいときに送信。
    [Serializable]
    public class ServerSetupRequestMessage : ModelData {
        public string matchId; // サーバ起動をリクエストしたマッチID
        public string sceneName; // 起動するシーン名
        protected override void Clear() {
            matchId = null; // サーバ起動をリクエストしたマッチID
            sceneName = null; // 起動するシーン名
        }
    }
    // ServerSetupResponseMessage
    // サーバセットアップレスポンスメッセージ。ServerSetupRequest のレスポンス。
    [Serializable]
    public class ServerSetupResponseMessage : ModelData {
        public string matchId; // サーバ起動をリクエストしたマッチID
        protected override void Clear() {
            matchId = null; // サーバ起動をリクエストしたマッチID
        }
    }
    // ServerSetupDoneMessage
    // サーバセットアップ完了メッセージ。Unity サーバが API サーバに対してクライアント接続可能状態を通知するときに送信。
    [Serializable]
    public class ServerSetupDoneMessage : ModelData {
        public string matchId; // サーバ起動をリクエストしたマッチID
        protected override void Clear() {
            matchId = null; // サーバ起動をリクエストしたマッチID
        }
    }
    // UniqueKeyData
    // 固有キー生成用データ。CouchDB のキー重複制限を使って固有キーを生成するために使用。
    [Serializable]
    public class UniqueKeyData : ModelData {
        public string associatedDataType; // 固有キーデータに紐づけられたデータタイプ
        public string associatedDataKey; // 固有キーデータに紐づけられたデータキー
        protected override void Clear() {
            associatedDataType = null; // 固有キーデータに紐づけられたデータタイプ
            associatedDataKey = null; // 固有キーデータに紐づけられたデータキー
        }
    }
}
