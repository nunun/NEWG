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
    ConnectState              connectState = ConnectState.Disconnected; // 現在の状態
    string                    connectError = null;                      // エラー
    ServerSetupRequestMessage setupRequest = null;                      // 受信したセットアップリクエスト

    public static bool                      IsDisconnected { get { return (instance.connectState == ConnectState.Disconnected); }}
    public static bool                      IsConnecting   { get { return (instance.connectState == ConnectState.Connecting);   }}
    public static bool                      IsConnected    { get { return (instance.connectState == ConnectState.Connected);    }}
    public static string                    ConnectError   { get { return instance.connectError; }}
    public static ServerSetupRequestMessage SetupRequest   { get { return instance.setupRequest; }}

    //-------------------------------------------------------------------------- 接続と切断
    // 接続開始
    public static void Connect() {
        Debug.Assert(instance              != null,                      "GameMindlinkManager がいない");
        Debug.Assert(instance.connectState == ConnectState.Disconnected, "既に接続中");
        instance.connectState = ConnectState.Connecting;
        instance.connectError = null;
        instance.setupRequest = null;
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
        // コネクタ取得
        var connector = MindlinkConnector.GetConnector();

        // 接続
        var mindlinkUrl = GameManager.MindlinkUrl;
        Debug.LogFormat("GameMindlinkManager: StartConnecting(): マインドリンクへ接続 ({0}) ...", mindlinkUrl);
        connector.url = mindlinkUrl;
        connector.Connect();

        // 接続を待つ
        Debug.Log("GameMindlinkManager:  StartConnecting(): マインドリンクへの接続をまっています ...");
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
        connectState   = ConnectState.Disconnected;
        connectError   = error;
        //setupRequest = null;

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
        connector.SetDataFromRemoteEventListener<ServerSetupRequestMessage,ServerSetupResponseMessage>(0, (req,res) => {
            Debug.Log("GameMindlinkManager: サーバセットアップリクエストメッセージ受信");
            setupRequest = req; // NOTE リクエストを記録
            var serverSetupResponseMessage = new ServerSetupResponseMessage();
            serverSetupResponseMessage.matchId = req.matchId;
            res.Send(serverSetupResponseMessage);
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
