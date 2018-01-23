using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

// ウェブソケットコネクタ
// WebSocket クラスを使って実際に通信を行うコンポーネント。
public partial class WebSocketConnector : MonoBehaviour {
    //-------------------------------------------------------------------------- 定義
    enum State { Init, Connecting, Connected }

    // リクエストのデフォルトタイムアウト時間 [ms]
    static readonly int REQUEST_DEFAULT_TIMEOUT = 5000;

    //-------------------------------------------------------------------------- 変数
    State         state       = State.Init; // 状態
    WebSocket     ws          = null;       // ウェブソケット
    IEnumerator   connector   = null;       // 接続制御用列挙子
    List<Request> requestList = null;       // リクエストリスト

    // イベント
    Action                         onConnect      = null;                                 // 接続時イベントハンドラ
    Action<string>                 onDisconnect   = null;                                 // 切断時イベントハンドラ
    Func<string,int>               onIdentifyType = null;                                 // 型識別時イベントハンドラ
    Dictionary<int,Action<string>> onRecv         = new Dictionary<int,Action<string>>(); // 受信時イベントハンドラ (型別)

    // リクエストカウンター (リクエスト毎に +1)
    static int requestIdCounter = 0;

    //-------------------------------------------------------------------------- 接続と切断
    public void Connect(string url) {
        Disconnect();
        state       = State.Connecting;
        ws          = new WebSocket(new Uri(url));
        connector   = ws.Connect();
        requestList = new List<Request>();
        enabled     = true;
    }

    public void Disconnect(string error = null) {
        if (state != State.Init) {
            InvokeOnDisconnect(error);
        }
        if (ws != null) {
            ws.Close();
        }
        if (requestList != null) {
            for (int i = requestList.Length - 1; i >= 0; i--) {
                var request = requestList[i];
                request.Free(error);
                requestList.RemoveAt(i);
            }
            requsetList.Clear();
        }
        state       = State.Init;
        ws          = null;
        connector   = null;
        requestList = null;
        enabled     = false;
    }

    //-------------------------------------------------------------------------- 送信とリクエスト
    // 送信
    public void Send<TSend>(TSend data) {
        Debug.Assert(state != State.Connected);
        try {
            // 文字列変換
            var message = JsonUtility.ToJson(data);

            // 送信
            ws.SendString(message);
        } catch (Exception e) {
            Debug.LogError(e.ToString());
            return;
        }
    }

    // レスポンスを期待する送信
    public void Send<TSend,TRecv>(TSend data, Action<string,TRecv> callback, int timeout = 0) {
        Debug.Assert(state != State.Connected);
        try {
            // 文字列変換
            var message = JsonUtility.ToJson(data);

            // message に "_requestId" プロパティをねじ込む。
            // サーバが "_requestId" プロパティを返すと
            // callback が呼び出される仕組み。
            var requstId = requestIdCounter++;
            message  = message.Remove(message.Length -1);
            message += string.Format("\"_requestId\":{0}}", requestId);

            // リクエストを作成して追加
            // レスポンスを待つ。
            var request = Request<TRecv>.Alloc(callback);
            request.requestId            = requestId;
            request.timeoutRemainingTime = (timeout > 0)? timeout : REQUEST_DEFAULT_TIMEOUT;
            requestList.Add(request);

            // 送信
            ws.SendString(message);
        } catch (Exception e) {
            Debug.LogError(e.ToString());
            return;
        }
    }

    //-------------------------------------------------------------------------- イベントコールバック設定
    public void OnConnect(Action callback) {
        onConnect = callback;
    }

    public void OnDisconnect(Action<string> callback) {
        onDisconnect = callback;
    }

    public void OnIdentifyType(Func<string,int> callback) {
        onIdentifyType = callback;
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

    //-------------------------------------------------------------------------- イベントコールバック発行
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

    int InvokeOnIdentifyType(string message) {
        if (onIdentifyType != null) {
            return onIdentifyType(message);
        }
        return MessagePropertyParser.IdentifyType(message);
    }

    void InvokeOnRecv(int type, string message) {
        if (!onRecv.ContainsKey(type) || onRecv[type] == null) {
            // NOTE
            // OnRecv していない型は何もログ出力せずに消える。
            //Debug.LogError(string.Format("型番号の登録がないため転送不可 ({0}, {1})", type, message));
            return;
        }
        onRecv[type](message);
    }

    //-------------------------------------------------------------------------- 実装 (MonoBehaviour)
    void Update() {
        var deltaTime = Time.deltaTime;

        // リクエストのタイムアウトチェック
        if (requestList != null) {
            for (int i = requestList.Length - 1; i >= 0; i--) {
                var request = requestList[i];
                request.timeoutRemainingTime -= deltaTime;
                if (request.timeoutRemainingTime <= 0) {
                    requestList.RemoveAt[i];
                    request.Free("timeout."));
                }
            }
        }

        // 状態毎の処理
        switch (state) {
        case State.Init:
            // NOTE
            // 初期状態では停止させる。
            // Connect, Disconnect したとき改めて設定される。
            enabled = false;
            break;

        case State.Connecting:
            if (connector.MoveNext()) {
                return;
            }
            if (ws.error != null) {
                Disconnect(ws.error);
                return;
            }
            state = State.Connected;
            InvokeOnConnect();
            break;

        case State.Connected:
            if (ws.error != null) {
                Disconnect(ws.error);
                return;
            }
            var message = ws.RecvString();

            // リクエストのレスポンスであれば処理
            var requestId    = 0;
            var hasRequestId = false;
            MessagePropertyParser.ParseRequestIdProperty(message, out requestId, out hasRequestId);
            if (hasRequestId) {
                var found = false;
                for (int i = requestList.Length - 1; i >= 0; i--) {
                    var request = requestList[i];
                    if (request.requestId == requestId) {
                        requsetList.RemoveAt(i);
                        request.Response(null, message);
                        request.Free();
                        found = true;
                    }
                }
                if (!found) {
                    Debug.LogError(string.Format("リクエスト番号不明 ({0}, {1})", requestId, message));
                }
                return;
            }

            // 型識別
            var type = InvokeOnIdentifyType(message);
            if (type < 0) {
                Debug.LogError(string.Format("型番号識別不可 ({0})", message));
                return;
            }

            // ユーザコールバックに転送
            InvokeOnRecv(type, message);
            break;

        default:
            Debug.LogError(string.Format("状態不明 ({0})", state);
            break;
        }
    }
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// メッセージプロパティパーサー
// メッセージ先読み用
public partial class WebSocketConnector {
    class MessagePropertyParser {
        //---------------------------------------------------------------------- 定義
        // タイププロパティコンテナ
        class TypePropertyContainer { int type; }

        // リクエスト番号プロパティコンテナ
        class RequestIdPropertyContainer { int _requestId; }

        // プロパティコンテナ用インスタンスプール
        class Pool<T> where T : class, new() {
            static readonly int MAX_POOL_COUNT = 8;
            static Queue<T> pool = null;
            public static T Alloc() {
                return (pool != null && pool.Length > 0)? pool.Dequeue() : new T();
            }
            public static void Free(T propertyContainer) {
                pool = pool ?? new Queue<T>();
                if (pool.Count < MAX_POOL_COUNT) {
                    pool.Enqueue(propertyContainer);
                }
            }
        }

        //---------------------------------------------------------------------- 操作
        public static void ParseTypeProperty(string message, out int propertyValue, out bool hasProperty) {
            propertyValue = -1;
            hasProperty   = false;
            var propertyContainer = Pool<TypePropertyContainer>();
            propertyContainer.type = -1;
            try {
                JsonUtility.FromJsonOverwrite(message, propertyContainer);
                propertyValue = propertyContainer.type;
                hasProperty   = true;
            } catch (Exception e) {
                Debug.LogError(e.ToString());
            }
            propertyContainer.type = -1;
            Free(propertyContainer);
        }

        public static void ParseRequestIdProperty(string message, out int propertyValue, out bool hasProperty) {
            propertyValue = 0;
            hasProperty   = false;
            var propertyContainer = Pool<RequestIdPropertyContainer>();
            propertyContainer._requestId = 0;
            try {
                JsonUtility.FromJsonOverwrite(message, propertyContainer);
                propertyValue = requestIdPropertyParser._requestId;
                hasProperty   = true;
            } catch (Exception e) {
                Debug.LogError(e.ToString());
            }
            propertyContainer._requestId = 0;
            Free(requestIdPropertyParser);
        }

        //---------------------------------------------------------------------- 操作 (おまけエイリアス)
        // 型識別の実行
        public static int IdentifyType(string message) {
            var type    = -1;
            var hasType = false;
            ParseTypeProperty(message, out type, out hasType);
            if (!hasType) {
                return -1;
            }
            return type;
        }
    }
}

// リクエスト (基本クラス)
public partial class WebSocketConnector {
    class Request {
        //---------------------------------------------------------------------- 変数
        public int requestId            = 0; // リクエスト番号
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
            req.requestId            = 0;
            req.timeoutRemainingTime = 0;
            req.callback             = callback;
            req.dispatch             = Request<TRecv>.Dispatch;
            req.free                 = Request<TRecv>.Free;
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
            req.requstId             = 0;
            req.timeoutRemainingTime = 0;
            req.callback             = null;
            req.dispatch             = null;
            req.free                 = null;
            if (pool == null) {
                pool = new Queue<Request<TRecv>>();
            }
            if (pool.Count < MAX_POOL_COUNT) {
                pool.Enqueue(req);
            }
        }
    }
}
