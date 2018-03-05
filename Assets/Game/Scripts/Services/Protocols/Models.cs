using System;
using Services.Protocols.Consts;
namespace Services.Protocols.Models {
    // MyModel
    // テストモデル
    [Serializable]
    public class MyModel {
        public int intValue = 0; // 整数型
        public string stringValue = null; // 文字列型
    }
    // User
    // ユーザ情報
    [Serializable]
    public class User {
        public string uuid = null; // ユーザのUUID型
    }
}
