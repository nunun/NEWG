using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

// ウェブソケットコネクタ
// WebSocket クラスを使って実際に通信を行うコンポーネント。
public partial class WebSocketConnector : MonoBehaviour {
    //-------------------------------------------------------------------------- 定義
    enum State { Init, Connecting, Connected }

    //-------------------------------------------------------------------------- 変数
    State       state     = State.Init; // 状態
    WebSocket   ws        = null;       // ウェブソケット
    IEnumerator connector = null;       // 接続制御用列挙子

    // イベント
    Action                         onConnect    = null; // 接続時イベントハンドラ
    Action<string>                 onDisconnect = null; // 切断時イベントハンドラ
    Func<string,int>               onParseType  = null; // 型判別時イベントハンドラ
    Dictionary<int,Action<string>> onRecv       = null; // 受信時イベントハンドラ (型別)

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
        Debug.Assert(state != State.Connected);
        try {
            var message = JsonUtility.ToJson(data);
            ws.SendString(message);
        } catch (Exception e) {
            Debug.LogError(e.ToString());
            return;
        }
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
                var data = JsonUtility.FromJson<TRecv>(message);
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

    void InvokeOnRecv(int type, string message) {
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
        this.onParseType  = DefaultTypeParser.ParseType;
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
            var requestId    = 0;
            var hasRequestId = false;
            DefaultRequestIdParser.ParseRequestId(message, out requestId, out hasRequestId);
            if (hasRequestId) {
                // TODO
                // リクエストレスポンス
                return;
            }
            var type = InvokeOnParseType(message);
            if (type < 0) {
                Debug.LogError(string.Format("型番号が判別不可 ({0})", message));
                return;
            }
            InvokeOnRecv(type, message);
            break;
        default:
            Debug.LogError("不明な状態 (" + state + ")");
            break;
        }
    }
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// 標準型パーサ
// 受け取ったメッセージの "type" プロパティで型番号取得。
public partial class WebSocketConnector {
    class DefaultTypeParser {
        //---------------------------------------------------------------------- 定義
        static readonly int MAX_POOL_COUNT = 16;

        //---------------------------------------------------------------------- 変数
        // 判別した型番号
        public int type;

        // 型パーサプール
        static Queue<DefaultTypeParser> pool = null;

        //---------------------------------------------------------------------- 操作
        public static int ParseType(string message) {
            var value = -1;
            var defaultTypeParser = Alloc();
            try {
                JsonUtility.FromJsonOverwrite(message, defaultTypeParser);
                value = defaultTypeParser.type;
            } catch (Exception e) {
                Debug.LogError(e.ToString());
            }
            Free(defaultTypeParser);
            return value;
        }

        //---------------------------------------------------------------------- 確保と解放
        // 確保
        static DefaultTypeParser Alloc() {
            var defaultTypeParser = ((pool != null) && (pool.Count > 0))? pool.Dequeue() : new DefaultTypeParser();
            defaultTypeParser.type = -1;
            return defaultTypeParser;
        }

        // 解放
        static void Free(DefaultTypeParser defaultTypeParser) {
            defaultTypeParser.type = -1;
            if (pool == null) {
                pool = new Queue<DefaultTypeParser>();
            }
            if (pool.Count < MAX_POOL_COUNT) {
                pool.Enqueue(defaultTypeParser);
            }
        }
    }
}

// リクエスト番号パーサ
// 受け取ったメッセージの "requestId" プロパティでリクエスト番号取得。
public partial class WebSocketConnector {
    class DefaultRequestIdParser {
        //---------------------------------------------------------------------- 定義
        static readonly int MAX_POOL_COUNT = 16;

        //---------------------------------------------------------------------- 変数
        // 判別したリクエスト番号
        public int requestId;

        // リクエスト番号パーサプール
        static Queue<DefaultRequestIdParser> pool = null;

        //---------------------------------------------------------------------- 操作
        public static void ParseRequestId(string message, out int value, out bool hasValue) {
            value    = 0;
            hasValue = false;
            var defaultRequestIdParser = Alloc();
            try {
                JsonUtility.FromJsonOverwrite(message, defaultRequestIdParser);
                value    = defaultRequestIdParser.requestId;
                hasValue = true;
            } catch (Exception e) {
                Debug.LogError(e.ToString());
            }
            Free(defaultRequestIdParser);
        }

        //---------------------------------------------------------------------- 確保と解放
        // 確保
        static DefaultRequestIdParser Alloc() {
            var defaultRequestIdParser = ((pool != null) && (pool.Count > 0))? pool.Dequeue() : new DefaultRequestIdParser();
            defaultRequestIdParser.requestId = 0;
            return defaultRequestIdParser;
        }

        // 解放
        static void Free(DefaultRequestIdParser defaultRequestIdParser) {
            defaultRequestIdParser.requestId = 0;
            if (pool == null) {
                pool = new Queue<DefaultRequestIdParser>();
            }
            if (pool.Count < MAX_POOL_COUNT) {
                pool.Enqueue(defaultRequestIdParser);
            }
        }
    }
}

// リクエスト
public partial class WebSocketConnector {
    class Request {
        //---------------------------------------------------------------------- 変数
        public int timeoutRemainingTime = 0; // タイムアウト残り時間

        // 転送および解放コールバック
        protected Action<Request,string> dispatch = null;
        protected Action<Request,string> free     = null;

        //---------------------------------------------------------------------- リクエスト操作
        // レスポンス
        public void Response(string message) {
            Debug.Assert(dispatch != null);
            dispatch(this, message);
        }

        // 解放
        public void Free(string error = null) {
            Debug.Assert(free != null);
            free(this, error);
        }
    }
}

// リクエスト (型別)
public partial class WebSocketConnector {
    class Request<TRecv> : Request {
        //---------------------------------------------------------------------- 定義
        static readonly int MAX_POOL_COUNT = 16;

        //---------------------------------------------------------------------- 変数
        // 受信コールバック
        protected Action<string,TRecv> callback = null;

        // リクエストプール
        static Queue<Request<TRecv>> pool = null;

        //---------------------------------------------------------------------- 確保
        // 確保
        public static Request<TRecv> Alloc(Action<string,TRecv> callback) {
            var req = ((pool != null) && (pool.Count > 0))? pool.Dequeue() : new Request<TRecv>();
            req.timeoutRemainingTime = 0;
            req.dispatch             = Request<TRecv>.Dispatch;
            req.free                 = Request<TRecv>.Free;
            req.callback             = callback;
            return req;
        }

        //---------------------------------------------------------------------- 内部コールバック用
        // 転送
        static void Dispatch(Request request, string message) {
            var req = request as Request<TRecv>;
            Debug.Assert(req != null);
            Debug.Assert(req.callback != null);
            try {
                var data = JsonUtility.FromJson<TRecv>(message);
                req.callback(null, data);
            } catch (Exception e) {
                req.callback(e.ToString(), default(TRecv));
            }
        }

        // 解放
        static void Free(Request request, string error) {
            var req = request as Request<TRecv>;
            Debug.Assert(req != null);
            if (req.callback != null) {
                req.callback(error, default(TRecv));
            }
            req.timeoutRemainingTime = 0;
            req.dispatch             = null;
            req.free                 = null;
            req.callback             = null;
            if (pool == null) {
                pool = new Queue<Request<TRecv>>();
            }
            if (pool.Count < MAX_POOL_COUNT) {
                pool.Enqueue(req);
            }
        }
    }
}

//if (!string.IsNullOrEmpty(connectKey)) {
//    uriBuilder.Query += ((string.IsNullOrEmpty(uriBuilder.Query))? "?" : "&") + "ck=" + connectKey;
//}
