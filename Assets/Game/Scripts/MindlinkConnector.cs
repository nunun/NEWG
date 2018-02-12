using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

// マインドリンクコネクタ
public partial class MindlinkConnector : WebSocketConnector {
    //-------------------------------------------------------------------------- 変数
    // マインドリンクデータタイプ
    public enum DataType {
        S = 1,
        Q = 2,
        M = 3,
    }
}

// connectKey 調整
// 環境変数からシークレットを取得して付与。
//var connectKeyValue = Environment.GetEnvironmentVariable("CONNECT_KEY");
//if (!string.IsNullOrEmpty(connectKeyValue)) {
//    connectKey = connectKeyValue;
//}
//var connectKeyFileValue = Environment.GetEnvironmentVariable("CONNECT_KEY_FILE");
//if (!string.IsNullOrEmpty(connectKeyFileValue)) {
//    try {
//        connectKey = File.ReadAllText(connectKeyFileValue).Trim();
//    } catch (Exception e) {
//        Debug.LogError(e.ToString());
//    }
//}
