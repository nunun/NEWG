using UnityEngine;
using System.Collections;
using System;

// ウェブソケットコネクタ
// WebSocket クラスを使って実際に通信を行うコンポーネント。
public class WebSocketConnector : MonoBehaviour {
    //-------------------------------------------------------------------------- 変数
    // TODO
    //WebSocket   ws          = null; // ウェブソケット
    //IEnumerator connection  = null; // 接続制御用列挙子
    //bool        isConnected = null; // 接続したかどうか

    //-------------------------------------------------------------------------- 接続と切断、送受信
    public void Connect(string url) {
        // TODO
        //WebSocket w = new WebSocket(new Uri("ws://echo.websocket.org"));
        //yield return StartCoroutine(w.Connect());
    }

    public void Disconnect() {
        // TODO
        //w.Close();
    }

    public void Send<TSend>(TSend data) {
        // TODO
        //ws.SendString("Hi there");
    }

    public void Send<TSend,TRecv>(TSend data, Action<TRecv> callback = null, int timeout = 0) {
        // TODO
        // 最後の "}" の前に ",_reqId:10}" のように挿入する。
        // 戻ってきたら _reqId をチェックする。
    }

    public void OnRecv<TRecv>(int type, Action<TRecv> callback = null) {
        // TODO
    }

    public void OnParseType(Func<string,int> typeParser) {
        // TODO
    }

    //-------------------------------------------------------------------------- 実装 (MonoBehaviour)
    void Update() {
        // TODO
        //string reply = w.RecvString();
        //if (reply != null) {
        //    Debug.Log ("Received: "+reply);
        //    w.SendString("Hi there"+i++);
        //}
        //if (w.error != null) {
        //    Debug.LogError ("Error: "+w.error);
        //    break;
        //}
    }
}
