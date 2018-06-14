using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Services.Protocols.Models;

// ゲームマインドリンクマネージャ
// ゲームと他のサービスとのクラスタ内情報交換を管理します。
[DefaultExecutionOrder(int.MinValue)]
public partial class GameMindlinkManager : MonoBehaviour {
    // NOTE
    // パーシャルクラスを参照
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// 接続関連
// シーンに存在する MindlinkConnector を使って接続を開始します。
// 接続開始後切断する方法はありません。
// 切断はアプリの強制終了を意味します。
public partial class GameMindlinkManager {
    //-------------------------------------------------------------------------- 定義
    public enum ConnectState { Disconnected, Connecting, Connected };

    //-------------------------------------------------------------------------- 変数
    ConnectState       connectState       = ConnectState.Disconnected; // 現在の状態
    string             connectError       = null;                      // エラー
    JoinRequestMessage joinRequestMessage = null;                      // 受信した参加リクエスト (TODO 恐らくキューになる)

    public static bool               IsDisconnected     { get { return (instance.connectState == ConnectState.Disconnected); }}
    public static bool               IsConnecting       { get { return (instance.connectState == ConnectState.Connecting);   }}
    public static bool               IsConnected        { get { return (instance.connectState == ConnectState.Connected);    }}
    public static string             ConnectError       { get { return instance.connectError; }}
    public static JoinRequestMessage JoinRequestMessage { get { return instance.joinRequestMessage; }}

    //-------------------------------------------------------------------------- 接続と切断
    // 接続開始
    public static void Connect() {
        Debug.Assert(instance              != null,                      "GameMindlinkManager がいない");
        Debug.Assert(instance.connectState == ConnectState.Disconnected, "既に接続中");
        instance.connectState       = ConnectState.Connecting;
        instance.connectError       = null;
        instance.joinRequestMessage = null;
        instance.StartCoroutine("StartConnecting");
    }

    // 切断
    public static void Disconnect(string error = null) {
        var connector = WebSocketConnector.GetConnector();
        connector.Disconnect(error);
    }

    //-------------------------------------------------------------------------- 接続と切断
    // 接続開始
    IEnumerator StartConnecting() {
        // 接続
        Debug.LogFormat("GameMindlinkManager: マインドリンクへ接続 ...");
        var connector = MindlinkConnector.GetConnector();
        connector.Connect();

        // 接続を待つ
        Debug.Log("GameMindlinkManager:  マインドリンクへの接続待ち ...");
        while (!connector.IsConnected) {
            yield return null;
        }

        // 接続済にする
        connectState = ConnectState.Connected;
    }

    // 接続停止
    void StopConnect(string error = null) {
        if (error != null) {
            Debug.LogError(error);
        }
        StopCoroutine("StartConnecting");
        connectState         = ConnectState.Disconnected;
        connectError         = error;
        //joinRequestMessage = null;

        // NOTE
        // マインドリンクからの切断は強制終了
        GameManager.Quit();
    }

    //-------------------------------------------------------------------------- 実装 (MonoBehaviour)
    void Start() {
        var connector = MindlinkConnector.GetConnector();
        connector.AddConnectEventListner(() => {
            Debug.Log("GameMindlinkManager: マインドリンク接続完了");
        });
        connector.AddDisconnectEventListner((error) => {
            Debug.Log("GameMindlinkManager: マインドリンク切断");
            StopConnect(error);
        });
        connector.SetDataFromRemoteEventListener<JoinRequestMessage,JoinResponseMessage>(0, (req,res) => {
            Debug.Log("GameMindlinkManager: 参加リクエストメッセージ受信");
            joinRequestMessage = req;//リクエストを記録
            var joinResponseMessage = new JoinResponseMessage();
            joinResponseMessage.joinId          = joinRequestMessage.joinId;
            joinResponseMessage.serverToken     = null;                         // TODO サーバトークン
            joinResponseMessage.serverSceneName = joinRequestMessage.sceneName; // TODO サーバシーン名
            joinResponseMessage.error           = null;                         // TODO 満員とゲーム開始済エラー
            res.Send(joinResponseMessage);
        });
    }
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// サーバステータスの送信
// 接続中のマインドリンク接続から、サーバステータスを送信します。
public partial class GameMindlinkManager {
    //-------------------------------------------------------------------------- 状態
    ServerStatusData serverStatusData = new ServerStatusData(); // サーバ状態データ

    // サーバ状態データの取得
    public static ServerStatusData ServerStatusData { get { return instance.serverStatusData; }}

    //-------------------------------------------------------------------------- 操作
    public static void SendServerStatusData(Action callback = null) {
        Debug.Assert(instance != null, "GameMindlinkManager なし");
        Debug.LogFormat("GameMindlinkManager: SendServerStatusData(): サーバ状態送信 (serverState = '{0}', serverAddress = '{1}', serverPort = {2}) ...", ServerStatusData.serverState, ServerStatusData.serverAddress, ServerStatusData.serverPort);
        var connector = MindlinkConnector.GetConnector();
        connector.SendStatus(instance.serverStatusData, (error) => {
            if (error != null) {
                #if DEBUG
                Debug.Log("GameMindlinkManager: SendServerStatusData(): サーバ状態送信失敗を無視 (" + error + ")");
                #else
                Debug.LogError(error);
                return;
                #endif
            }
            Debug.Log("GameMindlinkManager: SendServerStatusData(): サーバ状態送信完了");
            if (callback != null) {
                callback();
            }
        });
    }
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// MonoBehaviour 実装
public partial class GameMindlinkManager {
    //-------------------------------------------------------------------------- 変数
    // ゲームサービスインスタンス
    static GameMindlinkManager instance = null;

    //-------------------------------------------------------------------------- 実装 (MonoBehaviour)
    void Awake() {
        if (instance != null) {
            GameObject.Destroy(this.gameObject);
            return;
        }
        instance = this;
    }

    void OnDestroy() {
        if (instance != this) {
            return;
        }
        instance = null;
    }
}
