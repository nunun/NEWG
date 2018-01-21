using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

// ウェブソケットコネクタ
// WebSocket クラスを使って実際に通信を行うコンポーネント。
public class WebSocketConnector : MonoBehaviour {
    //-------------------------------------------------------------------------- 定義
    enum State { Init, Connecting, Connected, Error }

    //-------------------------------------------------------------------------- 変数
    State       state         = State.Init; // 状態
    WebSocket   ws            = null;       // ウェブソケット
    IEnumerator connector     = null;       // 接続制御用列挙子

    // イベント
    Action           onConnect    = null; // 接続時
    Action<string>   onDisconnect = null; // 切断時
    Func<string,int> onParseType  = null; // 型判別時

    // TODO
    // リクエスト機能
    // 受信機能 (型ディスパッチ)

    //-------------------------------------------------------------------------- 接続と切断
    public void Connect(string url) {
        Disconnect();
        state        = State.Connecting;
        ws           = new WebSocket(new Uri(url));
        connector    = ws.Connect();
        enabled      = true;
    }

    public void Disconnect(string error = null) {
        if (state != State.Init) {
            InvokeOnDisconnect(error);
        }
        if (ws != null) {
            ws.Close();
        }
        state     = State.Init;
        ws        = null;
        connector = null;
        enabled   = false;
    }

    //-------------------------------------------------------------------------- 送信
    public void Send<TSend>(TSend data) {
        // TODO
        //ws.SendString("Hi there");
    }

    public void Send<TSend,TRecv>(TSend data, Action<TRecv> callback = null, int timeout = 0) {
        // TODO
        // 最後の "}" の前に ",_reqId:10}" のように挿入する。
        // 戻ってきたら _reqId をチェックする。
    }

    //-------------------------------------------------------------------------- 受信
    public void OnConnect(Action callback) {
        onConnect = callback;
    }

    public void OnDisconnect(Action<string> callback) {
        onDisconnect = callback;
    }

    public void OnParseType(Func<string,int> callback) {
        onParseType = callback;
    }

    public void OnRecv<TRecv>(int type, Action<TRecv> callback) {
        // TODO
    }

    //-------------------------------------------------------------------------- イベント発行
    void InvokeOnConnect() {
        if (onConnect != null) {
            onConnect();
        }
    }

    void InvokeOnDisconnect(string error = null) {
        if (onDisconnect != null) {
            onDisconnect(error);
        }
    }

    int InvokeOnParseType(string message) {
        if (onParseType != null) {
            return onParseType(message);
        }
        return -1;
    }

    //-------------------------------------------------------------------------- 実装 (MonoBehaviour)
    void Start() {
        // NOTE
        // 更新は標準で停止。
        // Connect, Disconnect 時にオンオフ。
        this.enabled = false;
    }

    void Update() {
        switch (state) {
        case State.Init:
            break;
        case State.Connecting:
            if (connector.MoveNext()) {
                return;
            }
            state = State.Connected;
            InvokeOnConnect();
            break;
        case State.Connected:
            if (ws.error != null) { // NOTE 接続エラーもここで処理
                Disconnect(ws.error);
                return;
            }
            var message = ws.RecvString();
            Debug.Log(message); // TODO
            break;
        default:
            Debug.LogError("不明な状態 (" + state + ")");
            break;
        }
    }
}

//if (!string.IsNullOrEmpty(connectKey)) {
//    uriBuilder.Query += ((string.IsNullOrEmpty(uriBuilder.Query))? "?" : "&") + "ck=" + connectKey;
//}
