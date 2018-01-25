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

    // イベントハンドラ (内部システム向け)
    Action           connectEventHandler      = null; // 接続時イベントハンドラ
    Action<string>   disconnectEventHandler   = null; // 切断時イベントハンドラ
    Func<string,int> identifyTypeEventHandler = null; // 型識別時イベントハンドラ

    // イベントリスナ (外部ユーザ向け)
    Action                         connectEventListener    = null;                                 // 接続時イベントリスナ
    Action                         disconnectEventListener = null;                                 // 切断時イベントリスナ
    Dictionary<int,Action<string>> recvEventListener       = new Dictionary<int,Action<string>>(); // 受信時イベントリスナ (型別)

    // リクエストカウンター (リクエスト毎に +1)
    static int requestIdCounter = 0;

    // 接続済かどうか
    public bool IsConnected { get { return (state == State.Connected); }}

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
            InvokeDisconnectEvent(error);
        }
        if (ws != null) {
            ws.Close();
        }
        if (requestList != null) {
            for (int i = requestList.Count - 1; i >= 0; i--) {
                var request = requestList[i];
                request.Response(error ?? "abort.", null);
                request.Free();
                requestList.RemoveAt(i);
            }
            requestList.Clear();
        }
        state       = State.Init;
        ws          = null;
        connector   = null;
        requestList = null;
        enabled     = false;
    }

    //-------------------------------------------------------------------------- 送信とリクエスト
    // 送信
    public bool Send<TSend>(TSend data) {
        try {
            // 接続チェック
            if (state != State.Connected) {
                throw new Exception("WebSocket is not connected.");
            }

            // 文字列変換
            var message = JsonUtility.ToJson(data);

            // 送信
            ws.SendString(message);

        } catch (Exception e) {
            Debug.LogError(e.ToString());
            return false;
        }
        return true;
    }

    // レスポンスを期待する送信
    public bool Send<TSend,TRecv>(TSend data, Action<string,TRecv> callback, int timeout = 0) {
        var requestNew = default(Request<TRecv>);
        try {
            // 接続チェック
            if (state != State.Connected) {
                throw new Exception("WebSocket is not connected.");
            }

            // 文字列変換
            var message = JsonUtility.ToJson(data);

            // message に "_requestId" プロパティをねじ込む。
            // サーバが "_requestId" プロパティを返すと
            // callback が呼び出される仕組み。
            var requestId = requestIdCounter++;
            message  = message.Remove(message.Length -1);
            message += string.Format("\"_requestId\":{0}}", requestId);

            // リクエストを作成して追加
            // レスポンスを待つ。
            requestNew = Request<TRecv>.Alloc(callback);
            requestNew.requestId            = requestId;
            requestNew.timeoutRemainingTime = (float)((timeout > 0)? timeout : REQUEST_DEFAULT_TIMEOUT) / 1000.0f;
            requestList.Add(requestNew);

            // 送信
            ws.SendString(message);

        } catch (Exception e) {
            Debug.LogError(e.ToString());
            if (requestNew != null) {
                requestList.Remove(requestNew);
                requestNew.Free();
            }
            callback(e.ToString(), default(TRecv));
            return false;
        }
        return true;
    }

    //-------------------------------------------------------------------------- イベントハンドラ設定
    protected void SetConnectEventHandler(Action eventHandler) {
        connectEventHandler = eventHandler;
    }

    protected void SetDisconnectEventHandler(Action<string> eventHandler) {
        disconnectEventHandler = eventHandler;
    }

    protected void SetIdentifyTypeEventHandler(Func<string,int> eventHandler) {
        identifyTypeEventHandler = eventHandler;
    }

    //-------------------------------------------------------------------------- イベントリスナ設定
    public void AddConnectEventListner(Action eventListener) {
        connectEventListener += eventListener;
    }

    public void RemoveConnectEventListner(Action eventListener) {
        connectEventListener -= eventListener;
    }

    public void ClearConnectEventListner() {
        connectEventListener = null;
    }

    public void AddDisconnectEventListner(Action eventListener) {
        disconnectEventListener += eventListener;
    }

    public void RemoveDisconnectEventListner(Action eventListener) {
        disconnectEventListener -= eventListener;
    }

    public void ClearDisconnectEventListner() {
        disconnectEventListener = null;
    }

    public void SetRecvEventListener<TRecv>(int type, Action<TRecv> eventListener) {
        if (eventListener == null) {
            recvEventListener.Remove(type);
            return;
        }
        recvEventListener[type] = (string message) => {
            try {
                var data = JsonUtility.FromJson<TRecv>(message);
                eventListener(data);
            } catch (Exception e) {
                Debug.LogError(e.ToString());
                return;
            }
        };
    }

    //-------------------------------------------------------------------------- イベントコールバック発行
    void InvokeConnectEvent() {
        if (connectEventHandler != null) {
            connectEventHandler();
        }
        if (connectEventListener != null) {
            connectEventListener();
        }
    }

    void InvokeDisconnectEvent(string error = null) {
        if (disconnectEventHandler != null) {
            disconnectEventHandler(error);
        }
        if (disconnectEventListener != null) {
            disconnectEventListener(error);
        }
    }

    int InvokeIdentifyTypeEvent(string message) {
        if (identifyTypeEventHandler != null) {
            return identifyTypeEventHandler(message);
        }
        return MessagePropertyParser.IdentifyType(message);
    }

    void InvokeRecvEvent(int type, string message) {
        if (!recvEventListener.ContainsKey(type) || recvEventListener[type] == null) {
            // NOTE
            // OnRecv していない型は何もログ出力せずに消える。
            //Debug.LogError(string.Format("型番号の登録がないため転送不可 ({0}, {1})", type, message));
            return;
        }
        recvEventListener[type](message);
    }

    //-------------------------------------------------------------------------- 実装 (MonoBehaviour)
    void Update() {
        var deltaTime = Time.deltaTime;

        // リクエストのタイムアウトチェック
        if (requestList != null) {
            for (int i = requestList.Count - 1; i >= 0; i--) {
                var request = requestList[i];
                request.timeoutRemainingTime -= deltaTime;
                if (request.timeoutRemainingTime <= 0.0f) {
                    requestList.RemoveAt(i);
                    request.Response("timeout.", null);
                    request.Free();
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
            InvokeConnectEvent();
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
                for (int i = requestList.Count - 1; i >= 0; i--) {
                    var request = requestList[i];
                    if (request.requestId == requestId) {
                        requestList.RemoveAt(i);
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
            var type = InvokeIdentifyTypeEvent(message);
            if (type < 0) {
                Debug.LogError(string.Format("型番号識別不可 ({0})", message));
                return;
            }

            // ユーザコールバックに転送
            InvokeRecvEvent(type, message);
            break;

        default:
            Debug.LogError(string.Format("状態不明 ({0})", state));
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
        class TypePropertyContainer { public int type; }

        // リクエスト番号プロパティコンテナ
        class RequestIdPropertyContainer { public int _requestId; }

        // プロパティコンテナ用インスタンスプール
        class Pool<T> where T : class, new() {
            static readonly int MAX_POOL_COUNT = 8;
            static Queue<T> pool = null;
            public static T Alloc() {
                return (pool != null && pool.Count > 0)? pool.Dequeue() : new T();
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
            var propertyContainer = Pool<TypePropertyContainer>.Alloc();
            propertyContainer.type = -1;
            try {
                JsonUtility.FromJsonOverwrite(message, propertyContainer);
                propertyValue = propertyContainer.type;
                hasProperty   = true;
            } catch (Exception e) {
                Debug.LogError(e.ToString());
            }
            propertyContainer.type = -1;
            Pool<TypePropertyContainer>.Free(propertyContainer);
        }

        public static void ParseRequestIdProperty(string message, out int propertyValue, out bool hasProperty) {
            propertyValue = 0;
            hasProperty   = false;
            var propertyContainer = Pool<RequestIdPropertyContainer>.Alloc();
            propertyContainer._requestId = 0;
            try {
                JsonUtility.FromJsonOverwrite(message, propertyContainer);
                propertyValue = propertyContainer._requestId;
                hasProperty   = true;
            } catch (Exception e) {
                Debug.LogError(e.ToString());
            }
            propertyContainer._requestId = 0;
            Pool<RequestIdPropertyContainer>.Free(propertyContainer);
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
        public int   requestId            = 0;    // リクエスト番号
        public float timeoutRemainingTime = 0.0f; // タイムアウト残り時間

        // 転送および解放コールバック
        protected Action<Request,string,string> dispatch = null;
        protected Action<Request>               free     = null;

        //---------------------------------------------------------------------- リクエスト操作
        // レスポンス
        public void Response(string error, string message) {
            Debug.Assert(dispatch != null);
            dispatch(this, error, message);
        }

        // 解放
        public void Free() {
            Debug.Assert(free != null);
            free(this);
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
            req.timeoutRemainingTime = 0.0f;
            req.callback             = callback;
            req.dispatch             = Request<TRecv>.Dispatch;
            req.free                 = Request<TRecv>.Free;
            return req;
        }

        //---------------------------------------------------------------------- 内部コールバック用
        // 転送
        static void Dispatch(Request request, string error, string message) {
            var req = request as Request<TRecv>;
            Debug.Assert(req != null);
            Debug.Assert(req.callback != null);
            try {
                if (error != null) {
                    throw new Exception(error);
                }
                var data = JsonUtility.FromJson<TRecv>(message);
                req.callback(null, data);
            } catch (Exception e) {
                req.callback(e.ToString(), default(TRecv));
            }
        }

        // 解放
        static void Free(Request request) {
            var req = request as Request<TRecv>;
            Debug.Assert(req != null);
            req.requestId            = 0;
            req.timeoutRemainingTime = 0.0f;
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
