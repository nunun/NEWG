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
    ConnectState connectState      = ConnectState.Disconnected; // 現在の状態
    string       connectError      = null;                      // エラー
    string       reservedSceneName = null;                      // 推薦シーン名

    public static bool   IsDisconnected       { get { return (instance.connectState == ConnectState.Disconnected); }}
    public static bool   IsConnecting         { get { return (instance.connectState == ConnectState.Connecting);   }}
    public static bool   IsConnected          { get { return (instance.connectState == ConnectState.Connected);    }}
    public static string ConnectError         { get { return instance.connectError; }}
    public static string RecommendedSceneName { get { return instance.reservedSceneName; }}

    //-------------------------------------------------------------------------- 接続と切断
    // 接続開始
    public static void Connect() {
        Debug.Assert(instance              != null,                      "GameMindlinkManager がいない");
        Debug.Assert(instance.connectState == ConnectState.Disconnected, "既に接続中");
        instance.connectState         = ConnectState.Connecting;
        instance.connectError         = null;
        instance.reservedSceneName = null;
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
        connectState           = ConnectState.Disconnected;
        connectError           = error;
        //reservedSceneName = null;

        // NOTE
        // マインドリンクからの切断は強制終了
        GameManager.Quit();
    }

    //-------------------------------------------------------------------------- 初期化と更新
    void StartConnector() {
        var connector = MindlinkConnector.GetConnector();
        connector.AddConnectEventListner(() => {
            Debug.Log("GameMindlinkManager: マインドリンク接続完了");
        });
        connector.AddDisconnectEventListner((error) => {
            Debug.Log("GameMindlinkManager: マインドリンク切断");
            StopConnect(error);
        });
        connector.SetDataFromRemoteEventListener<ReserveRequestMessage>(0, (req, reqFrom) => {
            Debug.Log("GameMindlinkManager: 予約リクエストメッセージ受信");
            EnqueueReserveRequestMessage(reqFrom, req);//リクエストを記録
            if (reservedSceneName == null) {
                reservedSceneName = req.sceneName;//予約シーン名だけは記録
            }
        });
    }
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// 予約リクエストキューの処理
// サーバに対する予約リクエストがキューに溜まるので、
// コルーチンを設定すると、そのコルーチンで予約メッセージを処理できます。
public partial class GameMindlinkManager {
    //-------------------------------------------------------------------------- 定義
    public class ReserveRequest {
        public string                responseTo  = null;
        public string                serverToken = null;
        public ReserveRequestMessage message     = null;
    }

    //-------------------------------------------------------------------------- 変数
    Queue<ReserveRequest>                     reserveRequestQueue          = new Queue<ReserveRequest>(); // 受信した予約リクエストのキュー
    Func<string[],Action<string>,IEnumerator> reserveRequestHandler        = null;                        // 予約リクエストハンドラメソッド
    Coroutine                                 reserveRequestHandlerRunning = null;                        // 現在走っているハンドラコルーチン

    //-------------------------------------------------------------------------- 操作
    public static void SetReserveRequestMessageHandler(Func<string[],Action<string>,IEnumerator> handler) {
        if (instance.reserveRequestHandlerRunning != null) {
            instance.StopCoroutine(instance.reserveRequestHandlerRunning);
            instance.reserveRequestHandlerRunning = null;
        }
        instance.reserveRequestHandler = handler;
    }

    //-------------------------------------------------------------------------- 内部操作
    void EnqueueReserveRequestMessage(string responseTo, ReserveRequestMessage message) {
        Debug.Log("GameMindlinkManager: 予約リクエストメッセージ受信");
        reserveRequestQueue.Enqueue(new ReserveRequest() {
            responseTo = responseTo,
            message    = message,
        });
    }

    void HandleNextReserveRequest(ReserveRequest reserveRequest, string error) {
        if (reserveRequestHandlerRunning != null) {
            StopCoroutine(reserveRequestHandlerRunning);
            reserveRequestHandlerRunning = null;
        }
        var reserveResponseMessage = new ReserveResponseMessage();
        reserveResponseMessage.reserveId       = reserveRequest.message.reserveId;
        reserveResponseMessage.serverToken     = reserveRequest.serverToken;
        reserveResponseMessage.serverSceneName = reserveRequest.message.sceneName;
        reserveResponseMessage.error           = error;
        var connector = MindlinkConnector.GetConnector();
        connector.SendToRemote<ReserveResponseMessage>(reserveRequest.responseTo, 0, reserveResponseMessage);
    }

    //-------------------------------------------------------------------------- 初期化と更新
    void UpdateHandleReserveRequest() {
        if (reserveRequestHandlerRunning != null) {
            return;//既にコルーチンが走っている
        }
        if (reserveRequestHandler == null) {
            return;//ハンドラの指定がない
        }
        if (reserveRequestQueue.Count <= 0) {
            return;//予約メッセージなし
        }
        var reserveRequest = reserveRequestQueue.Dequeue();
        reserveRequestHandlerRunning = StartCoroutine(reserveRequestHandler(reserveRequest.message.users, (error) => {
            HandleNextReserveRequest(reserveRequest, error);
        }));
    }
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// サーバステータスの送信
// 接続中のマインドリンク接続から、サーバステータスを送信します。
public partial class GameMindlinkManager {
    //-------------------------------------------------------------------------- 変数
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

    void Start() {
        StartConnector();
    }

    void Update() {
        UpdateHandleReserveRequest();
    }
}
