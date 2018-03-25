using System;
using System.Collections;
using System.Collections.Generic;
using Services.Protocols.Consts;
using UnityEngine;
namespace Services.Protocols.Models {
    // User
    // ユーザ情報。サーバ上のユーザの情報。
    [Serializable]
    public class User : GameData {
        public string uuid; // ユーザのUUID型
        public string pid; // プレイヤー番号
        public string session_token; // セッショントークン
        public string login_token; // ログイントークン
        protected override void Clear() {
            uuid = null; // ユーザのUUID型
            pid = null; // プレイヤー番号
            session_token = null; // セッショントークン
            login_token = null; // ログイントークン
        }
    }
    // Player
    // プレイヤ情報。全ユーザに公開されるプレイヤーの情報。
    [Serializable]
    public class Player : GameData {
        public string pid; // プレイヤー番号
        public string name; // プレイヤー名
        protected override void Clear() {
            pid = null; // プレイヤー番号
            name = null; // プレイヤー名
        }
    }
    // SessionData
    // セッションデータ
    [Serializable]
    public class SessionData : GameData {
        public string session_token; // セッショントークン
        protected override void Clear() {
            session_token = null; // セッショントークン
        }
    }
    // LoginData
    // ログインデータ
    [Serializable]
    public class LoginData : GameData {
        public string login_token; // ログイントークン
        protected override void Clear() {
            login_token = null; // ログイントークン
        }
    }
    // SampleModel
    // サンプルモデル
    [Serializable]
    public class SampleModel : GameData {
        public int intValue; // 整数型
        public string stringValue1; // 文字列型
        public string stringValue2; // 文字列型 (null)
        public string stringValue3; // 文字列型 (空)
        public SampleModel objectValue1; // 型 (null)
        public User objectValue2; // 型 (空)
        public User[] arrayValue1; // 配列型 (null)
        public User[] arrayValue2; // 配列型 (空)
        public List<User> listValue1; // リスト型 (null)
        public List<User> listValue2; // リスト型 (空)
        protected override void Clear() {
            intValue = 100; // 整数型
            stringValue1 = "test"; // 文字列型
            stringValue2 = null; // 文字列型 (null)
            stringValue3 = ""; // 文字列型 (空)
            objectValue1 = null; // 型 (null)
            objectValue2 = new User(); // 型 (空)
            arrayValue1 = null; // 配列型 (null)
            arrayValue2 = new User[0]; // 配列型 (空)
            listValue1 = null; // リスト型 (null)
            listValue2 = new List<User>(); // リスト型 (空)
        }
    }
}
