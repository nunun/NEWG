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

// 接続初期化関連
// シーンに存在する MindlinkConnector を使って接続を開始します。
public partial class GameMindlinkManager {
    //-------------------------------------------------------------------------- 定義
    public enum State { Init, Connecting, Connected, Standby };

    //-------------------------------------------------------------------------- 変数
    State                     currentState = State.Init; // 現在の状態
    ServerSetupRequestMessage setupRequest = null;       // 受信したセットアップリクエスト

    // スタンバイ状態かどうか
    public static bool IsStandby { get { return (instance.currentState == State.Standby); }}

    // 受信したセットアップリクエストの取得
    public static ServerSetupRequestMessage SetupRequest { get { return instance.setupRequest; }}

    //-------------------------------------------------------------------------- 操作
    // 接続
    public static void Connect() {
        Debug.Assert(instance              != null,       "GameMindlinkManager がいない");
        Debug.Assert(instance.currentState == State.Init, "既に接続を開始した");
        instance.StartCoroutine("StartConnect");
    }

    //-------------------------------------------------------------------------- 内部処理
    // 接続開始
    IEnumerator StartConnect() {
        Debug.Assert(currentState == State.Init, "既に接続を開始した");
        currentState = State.Connecting;//接続中!

        // マインドリンクコネクタ取得
        var connector = MindlinkConnector.GetConnector();

        // イベント設定
        connector.AddConnectEventListner(() => {
            Debug.Log("マインドリンク接続完了");
        });
        connector.AddDisconnectEventListner((error) => {
            Debug.Log("マインドリンク切断");
            Debug.LogError(error);
            GameManager.Quit(); // NOTE マインドリンク切断でサーバ強制終了
        });
        connector.SetDataFromRemoteEventListener<ServerSetupRequestMessage,ServerSetupResponseMessage>(0, (req,res) => {
            Debug.Log("サーバ セットアップ リクエスト メッセージ受信");
            setupRequest = req; // NOTE リクエストを記録
            var serverSetupResponseMessage = new ServerSetupResponseMessage();
            serverSetupResponseMessage.matchId = req.matchId;
            res.Send(serverSetupResponseMessage);
        });

        // マインドリンクへ接続
        var mindlinkUrl = GameManager.MindlinkUrl;
        Debug.LogFormat("マインドリンクへ接続 ({0}) ...", mindlinkUrl);
        connector.url = mindlinkUrl;
        connector.Connect();

        // 接続を待つ
        Debug.Log("マインドリンクへの接続をまっています ...");
        while (!connector.IsConnected) {
            yield return null;
        }
        currentState = State.Connected;//接続完了!

        // サーバ状態を送信
        Debug.Log("サーバ状態を送信 (standby) ...");
        ServerStatusData.serverState   = "standby";
        ServerStatusData.serverAddress = GameManager.MindlinkServerAddress;
        ServerStatusData.serverPort    = 0;
        SendServerStatusData(() => {
            Debug.Log("サーバ状態送信完了");
            currentState = State.Standby;//スタンバイ!
        });
    }
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// 接続初期化関連
// シーンに存在する MindlinkConnector を使って接続を開始します。
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
