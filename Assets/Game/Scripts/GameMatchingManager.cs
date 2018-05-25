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

// マッチング関連
// シーンに存在する WebSocketConnector を使ってマッチングを開始します。
public partial class GameMatchingManager {
    //-------------------------------------------------------------------------- 定義
    public enum State { Init, Busy, Done };

    //-------------------------------------------------------------------------- 変数
    State            currentState     = State.Init; // 現在の状態
    string           currentError     = null;       // エラー
    MatchConnectData matchConnectData = null;       // 受信したセットアップリクエスト

    public static bool             IsDone           { get { return (instance.currentState == State.Done); }}
    public static string           Error            { get { return instance.currentError; }}
    public static MatchConnectData MatchConnectData { get { return instance.matchConnectData; }}

    //-------------------------------------------------------------------------- マッチング開始
    // マッチング開始
    public static void StartMatching() {
        Debug.Assert(instance              != null,       "GameMatchingManager がいない");
        Debug.Assert(instance.currentState != State.Busy, "既にマッチング中");
        instance.currentState     = State.Busy;
        instance.currentError     = null;
        instance.matchConnectData = null;
        instance.StartCoroutine("Matching");
    }

    // マッチング停止
    void StopMatching(string error) {
        Debug.LogError(error);
        StopCoroutine("Matching");
        currentState = State.Done;
        currentError = error;
        //instance.matchConnectData = matchConnectData;
    }

    // マッチング処理
    IEnumerator Matching() {
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

        // 接続データを待つ
        while (matchConnectData == null ){
            yield return null;
        }

        // 完了
        Debug.Log("マッチ接続データ受信完了");
        currentState = State.Done;
    }

    //-------------------------------------------------------------------------- マッチング中止
    public static void CancelMatching(string error = null) {
        var connector = WebSocketConnector.GetConnector();
        connector.Disconnect(error);
    }

    //-------------------------------------------------------------------------- 実装 (MonoBehaviour)
    void Start() {
        var connector = WebSocketConnector.GetConnector();
        connector.AddConnectEventListner(() => {
            Debug.Log("マッチングサーバ接続完了");
        });
        connector.AddDisconnectEventListner((error) => {
            Debug.Log("マッチングサーバ切断");
            StopMatching(error);
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
