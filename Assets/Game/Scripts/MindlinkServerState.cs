using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

// マインドリンク サーバ状態
// サーバの状態をマインドリンクを経由して共有。
public partial class MindlinkServerState {
    //-------------------------------------------------------------------------- 定義
    // サーバ状態情報
    [Serializable]
    public class ServerStateInfo {
        // TODO
        // ダミーパラメータ
        public int parameter = 0;

        // 公開処理
        public void Publish() {
            MindlinkServerState.PublishServerState(this);
        }
    }

    // 送信データ用
    public struct SendData {
        public int             type;
        public ServerStateInfo service;
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
        var sendData = new SendData();
        sendData.type    = (int)MindlinkConnector.DataType.S;
        sendData.service = currentServerState;
        MindlinkConnector.Instance.Send<SendData>(sendData);
    }

    protected static void PublishDefferedServerState() {
        if (defferedServerStateHashSet.Count <= 0) {
            return;
        }
        var serverStates = defferedServerStateHashSet.ToArray();
        defferedServerStateHashSet.Clear();
        for (int i = serverStates.Length - 1; i >= 0; i--) {
            var serverStateInfo = serverStates[i];
            PublishServerState(serverStateInfo);
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
        if (MindlinkConnector.Instance == null) {
            return;
        }
        MindlinkConnector.Instance.RemoveConnectEventListner(OnConnect);
        MindlinkConnector.Instance.RemoveDisconnectEventListner(OnDisconnect);
    }

    //-------------------------------------------------------------------------- 接続時、切断時
    void OnConnect() {
        PublishDefferedServerState();
    }

    void OnDisconnect(string error) {
        // 今のところ処理なし
    }
}
