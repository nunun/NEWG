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
    public enum State { Init, Busy, Done };

    //-------------------------------------------------------------------------- 変数
    State                     currentState = State.Init; // 現在の状態
    string                    currentError = null;       // エラー
    ServerSetupRequestMessage setupRequest = null;       // 受信したセットアップリクエスト

    public static bool                      IsDone       { get { return (instance.currentState == State.Done); }}
    public static string                    Error        { get { return instance.currentError; }}
    public static ServerSetupRequestMessage SetupRequest { get { return instance.setupRequest; }}

    //-------------------------------------------------------------------------- 接続
    // 接続開始
    public static void StartConnect() {
        Debug.Assert(instance              != null,       "GameMindlinkManager がいない");
        Debug.Assert(instance.currentState != State.Busy, "既に接続中");
        instance.currentState = State.Busy;
        instance.currentError = null;
        instance.setupRequest = null;
        instance.StartCoroutine("Connect");
    }

    // 接続停止
    void StopConnect(string error) {
        Debug.LogError(error);
        //StopCoroutine("Connect");
        //currentState = State.Done;
        //currentError = error;
        //setupRequest = null;
        GameManager.Quit(); // NOTE マインドリンク切断で有無を言わさず強制終了
    }

    // 接続
    IEnumerator Connect() {
        // コネクタ取得
        var connector = MindlinkConnector.GetConnector();

        // 接続
        var mindlinkUrl = GameManager.MindlinkUrl;
        Debug.LogFormat("マインドリンクへ接続 ({0}) ...", mindlinkUrl);
        connector.url = mindlinkUrl;
        connector.Connect();

        // 接続を待つ
        Debug.Log("マインドリンクへの接続をまっています ...");
        while (!connector.IsConnected) {
            yield return null;
        }

        // サーバ状態を送信
        Debug.Log("サーバ状態を送信 (standby) ...");
        ServerStatusData.serverState   = "standby";
        ServerStatusData.serverAddress = GameManager.MindlinkServerAddress;
        ServerStatusData.serverPort    = 0;
        SendServerStatusData(() => {
            Debug.Log("サーバ状態送信完了");
            currentState = State.Done;
        });
    }

    //-------------------------------------------------------------------------- 実装 (MonoBehaviour)
    void Start() {
        var connector = MindlinkConnector.GetConnector();
        connector.AddConnectEventListner(() => {
            Debug.Log("マインドリンク接続完了");
        });
        connector.AddDisconnectEventListner((error) => {
            Debug.Log("マインドリンク切断");
            StopConnect(error);
        });
        connector.SetDataFromRemoteEventListener<ServerSetupRequestMessage,ServerSetupResponseMessage>(0, (req,res) => {
            Debug.Log("サーバセットアップリクエストメッセージ受信");
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
        var connector = MindlinkConnector.GetConnector();
        connector.SendStatus(instance.serverStatusData, (error) => {
            if (error != null) {
                #if DEBUG
                Debug.Log("サーバ状態送信失敗を無視 (" + error + ")");
                #else
                Debug.LogError(error);
                return;
                #endif
            }
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
