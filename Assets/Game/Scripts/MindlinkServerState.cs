using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

// マインドリンク サーバ状態
// サーバの状態をマインドリンクを経由して共有。
public class MindlinkServerState {
    //-------------------------------------------------------------------------- 定義
    // サーバ状態情報
    [Serializable]
    class ServerStateInfo {
        public int parameter = 0;

        // 公開処理
        public void Publish() {
            MindlinkServerState.PublishServerState(this);
        }
    }

    //-------------------------------------------------------------------------- 変数
    // 現在のサーバ状態情報
    static ServerStateInfo currentServerState = new ServerStateInfo();

    // 現在のサーバ状態情報の取得
    public static ServerStateInfo CurrentServerState { get { return currentServerState; }}

    // 接続前に送信された情報があるかどうかのフラグ。
    // 延期後、接続してすぐに送信するために使用。
    static HashSet<ServerStateInfo> defferedServerStateHashSet = new HashSet<ServerStateInfo>();

    //-------------------------------------------------------------------------- 操作
    protected static void PublishServerState(ServerStateInfo serverStateInfo) {
        if (!MindlinkConnector.Instance.IsConnected) {
            defferedServerStateHashSet.Add(serverStateInfo);
            return;
        }
        MindlinkConnector.Instance.Send<ServerStateInfo>(serverStateInfo);
    }

    protected static void PublishDefferedServerState() {
        var serverStates = defferedServerStateHashSet.ToArray();
        defferedServerStateHashSet.Clear();
        for (int i = serverStates.Length - 1; i >= 0; i--) {
            var serverStateInfo = serverStates[i];
            PublishDefferedServerState(serverStateInfo);
        }
    }
}

// MonoBehaviour 実装
public partial class MindlinkServerState : MonoBehaviour {
    //-------------------------------------------------------------------------- 実装 (MonoBehaviour)
    void Start() {
        MindlinkConnector.Instance.AddConnectEventListner(OnConnect);
        MindlinkConnector.Instance.AddDisconnectEventListner(OnDisconnect);

        // NOTE
        // 既に接続済の場合は接続イベントを発行する。
        if (MindlinkConnector.Instance.IsConnected) {
            OnConnect();
        }
    }

    void OnDestroy() {
        MindlinkConnector.Instance.RemoveConnectEventListner(OnConnect);
        MindlinkConnector.Instance.RemoveDisconnectEventListner(OnDisconnect);
    }

    //-------------------------------------------------------------------------- 接続時、切断時
    void OnConnect() {
        if (isPublishDeffered) {
            isPublishDeffered = false; // NOTE 送信延期を解除して、送信を実行
            PublishDefferedServerState();
        }
    }

    void OnDisconnect() {
        // 今のところ処理なし
    }
}
