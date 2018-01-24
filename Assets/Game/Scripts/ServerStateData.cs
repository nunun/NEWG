using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

// サーバ状態データ
// MindlinkConnector を経由して状態を共有できるデータクラス。
[Serializable]
public partial class ServerStateData {
    //-------------------------------------------------------------------------- 変数
    public string parameter = 0;

    //-------------------------------------------------------------------------- 操作
    // サーバ状態公開
    public void Publish() {
        MindlinkConnector.Instance.Send<ServerStateData>(this);
    }
}

// サーバ状態データ一覧のリクエスト
[Serializable]
public partial class ServerStateData {
    // TODO
    // 一覧の定義とリクエスト
}

// 現在のサーバ状態データ
public partial class ServerStateData {
    //-------------------------------------------------------------------------- 変数
    // 現在のサーバ状態情報
    static ServerStateData current = new ServiceStateInfo();

    // 現在のサーバ状態情報の取得
    public static ServerStateData Current { get { return current; }}
}
