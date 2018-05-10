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
        public string playerId; // プレイヤー番号
        public string playerName; // プレイヤー名
        protected override void Clear() {
            playerId = null; // プレイヤー番号
            playerName = null; // プレイヤー名
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
    // UniqueKeyData
    // 固有キー生成用データ。CouchDB のキー重複制限を使って固有キーを生成するために使用。
    [Serializable]
    public class UniqueKeyData : ModelData {
        public string associatedKey; // 固有キーデータに紐づけられた関連キー
        protected override void Clear() {
            associatedKey = null; // 固有キーデータに紐づけられた関連キー
        }
    }
    // ServerStatus
    // サーバステータス
    [Serializable]
    public class ServerStatus : ModelData {
        public string state; // 現在状態 (Standby, Ready, Started, Ended)
        protected override void Clear() {
            state = null; // 現在状態 (Standby, Ready, Started, Ended)
        }
    }
    // ServerSetupRequest
    // サーバセットアップリクエスト。API サーバが Unity サーバに対してサーバインスタンスをセットアップしたいときに送信。
    [Serializable]
    public class ServerSetupRequest : ModelData {
        public string matchingId; // サーバ起動をリクエストしたマッチングID
        public string sceneName; // 起動するシーン名
        protected override void Clear() {
            matchingId = null; // サーバ起動をリクエストしたマッチングID
            sceneName = null; // 起動するシーン名
        }
    }
    // ServerSetupResponse
    // サーバセットアップレスポンス。ServerBootRequest のレスポンス。
    [Serializable]
    public class ServerSetupResponse : ModelData {
        public string matchingId; // サーバ起動をリクエストしたマッチングID
        protected override void Clear() {
            matchingId = null; // サーバ起動をリクエストしたマッチングID
        }
    }
    // ServerSetupDoneRequest
    // サーバセットアップ完了リクエスト。Unity サーバが API サーバに対してクライアント接続可能状態を通知するときに送信。
    [Serializable]
    public class ServerSetupDoneRequest : ModelData {
        public string matchingId; // サーバ起動をリクエストしたマッチングID
        protected override void Clear() {
            matchingId = null; // サーバ起動をリクエストしたマッチングID
        }
    }
    // ServerConnectData
    // サーバ接続データ
    [Serializable]
    public class ServerConnectData : ModelData {
        public string serverAddress; // 接続先のサーバアドレス
        public int serverPort; // 接続先のサーバポート
        public string serverToken; // 接続用サーバトークン
        protected override void Clear() {
            serverAddress = "localhost"; // 接続先のサーバアドレス
            serverPort = 7777; // 接続先のサーバポート
            serverToken = null; // 接続用サーバトークン
        }
    }
}
