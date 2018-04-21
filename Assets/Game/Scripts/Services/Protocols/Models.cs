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
        protected override void Clear() {
            userId = null; // 固有のユーザID
            playerId = null; // 固有のプレイヤID
            sessionToken = null; // セッショントークン
            signinToken = null; // ログイントークン
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
}
