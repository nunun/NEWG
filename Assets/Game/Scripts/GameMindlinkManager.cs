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
    ConnectState connectState         = ConnectState.Disconnected; // 現在の状態
    string       connectError         = null;                      // エラー
    string       recommendedSceneName = null;                      // 推薦シーン名

    public static bool   IsDisconnected       { get { return (instance.connectState == ConnectState.Disconnected); }}
    public static bool   IsConnecting         { get { return (instance.connectState == ConnectState.Connecting);   }}
    public static bool   IsConnected          { get { return (instance.connectState == ConnectState.Connected);    }}
    public static string ConnectError         { get { return instance.connectError; }}
    public static string RecommendedSceneName { get { return instance.recommendedSceneName; }}

    //-------------------------------------------------------------------------- 接続と切断
    // 接続開始
    public static void Connect() {
        Debug.Assert(instance              != null,                      "GameMindlinkManager がいない");
        Debug.Assert(instance.connectState == ConnectState.Disconnected, "既に接続中");
        instance.connectState         = ConnectState.Connecting;
        instance.connectError         = null;
        instance.recommendedSceneName = null;
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
        //recommendedSceneName = null;

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
        connector.SetDataFromRemoteEventListener<JoinRequestMessage>(0, (req, reqFrom) => {
            Debug.Log("GameMindlinkManager: 参加リクエストメッセージ受信");
            EnqueueJoinRequestMessage(reqFrom, req);//リクエストを記録
            if (recommendedSceneName == null) {
                recommendedSceneName = req.sceneName;//推薦シーン名だけは記録
            }
        });
    }
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// 参加リクエストキューの処理
// サーバに対する参加メッセージがキューに溜まるので、
// コルーチンを設定すると、そのコルーチンで参加メッセージを処理できます。
public partial class GameMindlinkManager {
    //-------------------------------------------------------------------------- 定義
    public class JoinRequest {
        public string             responseTo  = null;
        public string             serverToken = null;
        public JoinRequestMessage message     = null;
    }

    //-------------------------------------------------------------------------- 変数
    Queue<JoinRequest>                        joinRequestQueue          = null; // 受信した参加リクエストメッセージのキュー
    Func<string[],Action<string>,IEnumerator> joinRequestHandler        = null; // 参加リクエストメッセージハンドラメソッド
    Coroutine                                 joinRequestHandlerRunning = null; // 現在走っているハンドラコルーチン

    //-------------------------------------------------------------------------- メッセージハンドラ関連
    public void SetJoinRequestMessageHandler(Func<string[],Action<string>,IEnumerator> handler) {
        if (joinRequestHandlerRunning != null) {
            StopCoroutine(joinRequestHandlerRunning);
            joinRequestHandlerRunning = null;
        }
        joinRequestHandler = handler;
    }

    //-------------------------------------------------------------------------- 内部操作
    void EnqueueJoinRequestMessage(string responseTo, JoinRequestMessage message) {
        joinRequestQueue.Enqueue(new JoinRequest() {
            responseTo = responseTo,
            message    = message,
        });
    }

    void HandleNextJsonRequestMessage(JoinRequest joinRequest, string error) {
        if (joinRequestHandlerRunning != null) {
            StopCoroutine(joinRequestHandlerRunning);
            joinRequestHandlerRunning = null;
        }
        var connector = MindlinkConnector.GetConnector();
        var joinResponseMessage = new JoinResponseMessage();
        joinResponseMessage.joinId          = joinRequest.message.joinId;
        joinResponseMessage.serverToken     = joinRequest.serverToken;
        joinResponseMessage.serverSceneName = joinRequest.message.sceneName;
        joinResponseMessage.error           = error;
        connector.SendToRemote<JoinResponseMessage>(joinRequest.responseTo, 0, joinResponseMessage);
    }

    //-------------------------------------------------------------------------- 実装 (MonoBehaviour)
    void Update() {
        if (joinRequestHandlerRunning != null) {
            return;//既にコルーチンが走っている
        }
        if (joinRequestHandler == null) {
            return;//ハンドラの指定がない
        }
        if (joinRequestQueue.Count <= 0) {
            return;//参加メッセージなし
        }
        var joinRequest = joinRequestQueue.Dequeue();
        joinRequestHandlerRunning = StartCoroutine(joinRequestHandler(joinRequest.message.users, (error) => {
            HandleNextJsonRequestMessage(joinRequest, error);
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
}
