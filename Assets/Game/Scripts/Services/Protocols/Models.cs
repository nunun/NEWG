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
        public string user_id; // 固有のユーザID
        public string player_id; // 固有のプレイヤID
        public string session_token; // セッショントークン
        public string signin_token; // ログイントークン
        protected override void Clear() {
            user_id = null; // 固有のユーザID
            player_id = null; // 固有のプレイヤID
            session_token = null; // セッショントークン
            signin_token = null; // ログイントークン
        }
    }
    // PlayerData
    // プレイヤデータ。全ユーザに公開されるプレイヤのデータ。
    [Serializable]
    public class PlayerData : ModelData {
        public string player_id; // プレイヤー番号
        public string player_name; // プレイヤー名
        protected override void Clear() {
            player_id = null; // プレイヤー番号
            player_name = null; // プレイヤー名
        }
    }
    // SessionData
    // セッションデータ。本人にのみ知らされるセッション維持に使用するデータ。
    [Serializable]
    public class SessionData : ModelData {
        public string session_token; // セッショントークン
        protected override void Clear() {
            session_token = null; // セッショントークン
        }
    }
    // CredentialData
    // 認証データ。本人にのみ知らされるサインイン用データ。セッションが切れた時に使用。
    [Serializable]
    public class CredentialData : ModelData {
        public string signin_token; // サインイントークン
        protected override void Clear() {
            signin_token = null; // サインイントークン
        }
    }
    // SampleModel
    // サンプルモデル
    [Serializable]
    public class SampleModel : ModelData {
        public int intValue; // 整数型
        public string stringValue1; // 文字列型
        public string stringValue2; // 文字列型 (null)
        public string stringValue3; // 文字列型 (空)
        public SampleModel objectValue1; // 型 (null)
        public UserData objectValue2; // 型 (空)
        public UserData[] arrayValue1; // 配列型 (null)
        public UserData[] arrayValue2; // 配列型 (空)
        public List<UserData> listValue1; // リスト型 (null)
        public List<UserData> listValue2; // リスト型 (空)
        protected override void Clear() {
            intValue = 100; // 整数型
            stringValue1 = "test"; // 文字列型
            stringValue2 = null; // 文字列型 (null)
            stringValue3 = ""; // 文字列型 (空)
            objectValue1 = null; // 型 (null)
            objectValue2 = new UserData(); // 型 (空)
            arrayValue1 = null; // 配列型 (null)
            arrayValue2 = new UserData[0]; // 配列型 (空)
            listValue1 = null; // リスト型 (null)
            listValue2 = new List<UserData>(); // リスト型 (空)
        }
    }
}
