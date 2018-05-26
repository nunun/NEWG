using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Services.Protocols.Models;

// ゲームマッチングマネージャ
// マッチングを処理します。
[DefaultExecutionOrder(int.MinValue)]
public partial class GameMatchingManager : MonoBehaviour {
    // NOTE
    // パーシャルクラスを参照
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// 接続と切断
public partial class GameMatchingManager {
    //-------------------------------------------------------------------------- 定義
    public enum ConnectState { Disconnected, Connecting, Connected };

    //-------------------------------------------------------------------------- 変数
    ConnectState     connectState     = ConnectState.Disconnected; // 接続状態
    string           connectError     = null;                      // 接続エラー
    MatchConnectData matchConnectData = null;                      // 受信したセットアップリクエスト

    // TODO
    // IsConnecting
    // IsConnected

    public static string           ConnectError     { get { return instance.connectError;     }}
    public static MatchConnectData MatchConnectData { get { return instance.matchConnectData; }}

    //-------------------------------------------------------------------------- 接続と切断
    // 接続
    public static void Connect() {
        Debug.Assert(instance              != null,                      "GameMatchingManager がいない");
        Debug.Assert(instance.connectState == ConnectState.Disconnected, "既に接続中");
        instance.connectState     = ConnectState.Connecting;
        instance.connectError     = null;
        instance.matchConnectData = null;
        instance.StartCoroutine("StartConnecting");
    }

    // 切断
    public static void Disconnect(string error = "") {
        var connector = WebSocketConnector.GetConnector();
        connector.Disconnect(error);
    }

    //-------------------------------------------------------------------------- 接続の開始と停止
    // 接続開始
    IEnumerator StartConnecting() {
        // コネクタ取得
        var connector = WebSocketConnector.GetConnector();

        // 接続
        var matchingServerUrl = GameManager.MatchingServerUrl;
        Debug.LogFormat("マッチングサーバへ接続 ({0}) ...", matchingServerUrl);
        connector.url = matchingServerUrl;
        connector.Connect();

        // 接続を待つ
        Debug.Log("マッチングサーバへの接続をまっています ...");
        while (!connector.IsConnected) {
            yield return null;
        }

        // 接続終了
        StopConnecting();
    }

    // 接続停止
    void StopConnecting(string error = "") {
        if (error != null) {
            Debug.LogError(error);
        }
        StopCoroutine("StartConnecting");
        connectState       = ConnectState.Disconnected;
        connectError       = error;
        //matchConnectData = null;
    }

    //-------------------------------------------------------------------------- 実装 (MonoBehaviour)
    void Start() {
        var connector = WebSocketConnector.GetConnector();
        connector.AddConnectEventListner(() => {
            Debug.Log("マッチングサーバ接続完了");
        });
        connector.AddDisconnectEventListner((error) => {
            Debug.Log("マッチングサーバ切断");
            StopConnecting(error);
        });
        connector.SetDataEventListener<MatchConnectData>(0, (data) => {
            Debug.Log("マッチ接続データ受信");
            matchConnectData = data;
        });
    }
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// MonoBehaviour 実装
public partial class GameMatchingManager {
    //-------------------------------------------------------------------------- 変数
    // ゲームサービスインスタンス
    static GameMatchingManager instance = null;

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
