using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

// NOTE
// だいたいのやり取りは Request で行われるため、切断時はエラーになる。
// それを考えれば、あえて切断状態をとるひつようはないと考えられる。
// Send に送信エラーがついていれば良いのかもしれない。
// また、コールバックを期待する場合は即時エラーバックする。
// 切断状態か接続状態かを知る必要はない。
// ただ精神衛生上、OnRecv の後始末はしたいところ...
// onRecv は struct で管理する。
public partial class MindlinkClientConnector : WebSocketConnector {
    //-------------------------------------------------------------------------- 変数
    public string url; // 接続先URL

    //-------------------------------------------------------------------------- 実装 (MonoBehaviour)
    void Start() {
        var connector = gameObject.GetComponent<WebSocketConnector>();
        Debug.Assert(connector != null);
        connector.Connect(url);
    }
}

// BACKUP
//if (!string.IsNullOrEmpty(connectKey)) {
//    uriBuilder.Query += ((string.IsNullOrEmpty(uriBuilder.Query))? "?" : "&") + "ck=" + connectKey;
//}
