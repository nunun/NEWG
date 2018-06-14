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
    // マッチングデータ。APIサーバが作成しマッチングサーバが確認する内部連携データ。
    [Serializable]
    public class MatchingData : ModelData {
        public string matchingId; // マッチングID
        public string[] users; // マッチングするユーザID一覧
        protected override void Clear() {
            matchingId = null; // マッチングID
            users = new string[0]; // マッチングするユーザID一覧
        }
    }
    // MatchConnectData
    // マッチ接続データ。マッチングサーバがクライアントに返却するデータ。この情報をもってゲームサーバに接続。
    [Serializable]
    public class MatchConnectData : ModelData {
        public string matchId; // マッチID
        public string serverAddress; // 接続先のゲームサーバアドレス
        public int serverPort; // 接続先のゲームサーバポート
        public int serverToken; // 接続先に必要なサーバトークン
        public string serverSceneName; // 接続先のゲームサーバシーン名
        protected override void Clear() {
            matchId = null; // マッチID
            serverAddress = "localhost"; // 接続先のゲームサーバアドレス
            serverPort = 7777; // 接続先のゲームサーバポート
            serverToken = 7777; // 接続先に必要なサーバトークン
            serverSceneName = null; // 接続先のゲームサーバシーン名
        }
    }
    // JoinRequestMessage
    // 参加リクエストメッセージ。マッチングサーバがゲームサーバに送信します。
    [Serializable]
    public class JoinRequestMessage : ModelData {
        public string joinId; // 参加ID
        public string sceneName; // 起動を希望するシーン名
        public string[] users; // 参加を希望するユーザ情報
        protected override void Clear() {
            joinId = null; // 参加ID
            sceneName = null; // 起動を希望するシーン名
            users = new string[0]; // 参加を希望するユーザ情報
        }
    }
    // JoinResponseMessage
    // 参加レスポンスメッセージ。ゲームサーバがマッチングサーバに返信します。
    [Serializable]
    public class JoinResponseMessage : ModelData {
        public string joinId; // 参加ID
        public string serverToken; // サーバトークン
        public string serverSceneName; // サーバシーン名
        public string error; // 参加エラー
        protected override void Clear() {
            joinId = null; // 参加ID
            serverToken = null; // サーバトークン
            serverSceneName = null; // サーバシーン名
            error = null; // 参加エラー
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
    // ErrorData
    // エラーデータ。WebAPI やメッセージングでエラーになった場合のレスポンスデータ。
    [Serializable]
    public class ErrorData : ModelData {
        public ErrorDescriptionData err; // エラー詳細情報
        protected override void Clear() {
            err = null; // エラー詳細情報
        }
    }
    // ErrorDescriptionData
    // エラー詳細データ。実際のエラー情報。
    [Serializable]
    public class ErrorDescriptionData : ModelData {
        public int code; // エラーコード
        public string message; // エラーメッセージ
        public string stack; // スタックトレース
        protected override void Clear() {
            code = 0; // エラーコード
            message = null; // エラーメッセージ
            stack = null; // スタックトレース
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
