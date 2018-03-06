using System;
using System.Collections;
using System.Collections.Generic;
using Services.Protocols.Consts;
using UnityEngine;
namespace Services.Protocols.Models {
    // User
    // ユーザ情報
    [Serializable]
    public class User {
        public string uuid = null; // ユーザのUUID型
    }
    // SampleModel
    // テストモデル
    [Serializable]
    public class SampleModel {
        public int intValue = 100; // 整数型
        public string stringValue1 = "test"; // 文字列型
        public string stringValue2 = null; // 文字列型 (null)
        public string stringValue3 = ""; // 文字列型 (空)
        public SampleModel objectValue1 = null; // 型 (null)
        public User objectValue2 = new User(); // 型 (空)
        public User[] arrayValue1 = null; // 配列型 (null)
        public User[] arrayValue2 = new User[0]; // 配列型 (空)
        public List<User> listValue1 = null; // リスト型 (null)
        public List<User> listValue2 = new List<User>(); // リスト型 (空)
    }
}
