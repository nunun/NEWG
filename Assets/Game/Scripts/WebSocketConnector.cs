using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

// ウェブソケットコネクタ
// WebSocket クラスを使って実際に通信を行うコンポーネント。
public class WebSocketConnector : MonoBehaviour {
    //-------------------------------------------------------------------------- 定義
    // 状態
    enum State { Init, Connecting, Connected }

    // 型パーサ
    class TypeParser {
        public int type;
    }

    // 型パーサプール最大数
    public static readonly int TYPE_PARSER_POOL_MAX_COUNT = 256;

    //-------------------------------------------------------------------------- 変数
    State       state     = State.Init; // 状態
    WebSocket   ws        = null;       // ウェブソケット
    IEnumerator connector = null;       // 接続制御用列挙子

    // イベント
    Action                         onConnect    = null; // 接続時イベントハンドラ
    Action<string>                 onDisconnect = null; // 切断時イベントハンドラ
    Func<string,int>               onParseType  = null; // 型判別時イベントハンドラ
    Dictionary<int,Action<string>> onRecv       = null; // 受信時イベントハンドラ (型別)

    // 型パーサプール
    static Queue<TypeParser> typeParserPool = new Queue<TypeParser>();

    // TODO
    // データの送信
    // リクエスト機能

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
        onRecv[type] = (string message) => {
            try {
                var data = UnityEngine.JsonUtility.FromJson<TRecv>(message);
                callback(data);
            } catch (Exception e) {
                Debug.LogError(e.ToString());
                return;
            }
        };
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

    void InvokeOnRecv(string message) {
        var type = InvokeOnParseType(message);
        if (type < 0) {
            Debug.LogError(string.Format("型番号が判別不可 ({0})", message));
            return;
        }
        if (!onRecv.ContainsKey(type) || onRecv[type] == null) {
            Debug.LogError(string.Format("型番号の登録がないため転送不可 ({0}, {1})", type, message));
            return;
        }
        onRecv[type](message);
    }

    //-------------------------------------------------------------------------- 実装 (MonoBehaviour)
    void Awake() {
        // NOTE
        // 初期化。
        // イベントハンドラは最初に Awake したときだけ初期化。
        Disconnect();
        this.onConnect    = null;
        this.onDisconnect = null;
        this.onParseType  = DefaultTypeParser;
        this.onRecv       = new Dictionary<int,Action<string>>();

        // NOTE
        // デフォルト停止, 接続時および切断時にオンオフ。
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
            InvokeOnRecv(message);
            break;
        default:
            Debug.LogError("不明な状態 (" + state + ")");
            break;
        }
    }

    //-------------------------------------------------------------------------- その他
    // 標準型パーサー
    static int DefaultTypeParser(message) {
        var typeParser = default(TypeParser);
        if (typeParserPool.Count > 0) {
            typeParser = TypeParserPool.Dequeue();
        } else {
            typeParser = new TypeParser();
        }
        typeParser.type = -1;
        try {
            JsonUtility.FromJsonOverwrite(message, typeParser);
        } catch (Exception e) [
            Debug.LogError(e.ToString());
            return -1;
        }
        var type = typeParser.type;
        if (typeParserPool.Count < TYPE_PARSER_POOL_MAX_COUNT) {
            TypeParser.Enqueue(typeParser);//再利用
        }
        return type;
    }
}

//if (!string.IsNullOrEmpty(connectKey)) {
//    uriBuilder.Query += ((string.IsNullOrEmpty(uriBuilder.Query))? "?" : "&") + "ck=" + connectKey;
//}
