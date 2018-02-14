using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Text;

// ウェブソケットコネクタ
// WebSocket クラスを使って実際に通信を行うコンポーネント。
public partial class WebSocketConnector : MonoBehaviour {
    //-------------------------------------------------------------------------- 定義
    enum State { Init, Connecting, Connected }

    // リクエストのデフォルトタイムアウト時間 [ms]
    static readonly int REQUEST_DEFAULT_TIMEOUT = 5000;

    //-------------------------------------------------------------------------- 変数
    public string   url           = "ws://localhost:7766"; // 接続先URL
    public string[] queries       = null;                  // クエリ一覧
    public int      retryCount    = 10;                    // 接続リトライ回数
    public float    retryInterval = 3.0f;                  // 接続リトライ間隔

    State         state       = State.Init; // 状態
    WebSocket     ws          = null;       // ウェブソケット
    IEnumerator   connector   = null;       // 接続制御用列挙子
    List<Request> requestList = null;       // リクエストリスト

    // イベントリスナ
    Action                         connectEventListener     = null;                                 // 接続時イベントリスナ
    Action<string>                 disconnectEventListener  = null;                                 // 切断時イベントリスナ
    Dictionary<int,Action<string>> requestEventListener     = new Dictionary<int,Action<string>>(); // リクエスト受信時イベントリスナ (型別)
    Dictionary<int,Action<string>> dataEventListener        = new Dictionary<int,Action<string>>(); // データ受信時イベントリスナ     (型別)

    // 現在のリトライ回数
    int currentRetryCount = 0;

    // UUID
    string uuid = null;

    // UUID を取得する
    public string UUID { get { return uuid ?? (uuid = Guid.NewGuid().ToString()); }}

    // 接続済かどうか
    public bool IsConnected { get { return (state == State.Connected); }}

    // リクエストカウンター (リクエスト毎に +1)
    static int requestIdCounter = 0;

    // 次のリクエスト番号を取得する
    static int NextRequestId { get { return (++requestIdCounter == 0)? ++requestIdCounter : requestIdCounter; }}

    //-------------------------------------------------------------------------- 接続と切断
    public void Connect(params string[] queries) {
        this.queries           = queries;
        this.currentRetryCount = 0;
        Reconnect();
    }

    public void Reconnect() {
        // 初期状態でないなら一旦切断
        if (state != State.Init) {
            Disconnect();
        }

        // クエリパラメータ付与
        if (queries.Length > 0) {
            var sb = ObjectPool<StringBuilder>.GetObject();
            sb.Append(url);
            for (int i = 0; i < (queries.Length - 1); i += 2) {
                var key = queries[i + 0];
                var val = queries[i + 1];
                sb.Append((i == 0)? "?" : "&");
                sb.Append(key); sb.Append("="); sb.Append(val);
            }
            url = sb.ToString();
            ObjectPool<StringBuilder>.ReturnObject(sb);
        }

        // 接続
        state       = State.Connecting;
        ws          = new WebSocket(new Uri(url));
        connector   = ws.Connect();
        requestList = new List<Request>();
        enabled     = true;
    }

    public void Disconnect(string error = null) {
        if (state == State.Connecting && !string.IsNullOrEmpty(error)) {
            Debug.LogError(error);
            if (currentRetryCount++ < retryCount) {
                Invoke("Reconnect", retryInterval); // NOTE 再接続
                return;
            }
        }
        InvokeDisconnectEvent(error);
        if (ws != null) {
            ws.Close();
        }
        if (requestList != null) {
            for (int i = requestList.Count - 1; i >= 0; i--) {
                var request = requestList[i];
                request.Response(error ?? "abort.", null);
                request.ReturnToPool();
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
    public bool Send<TSend>(int type, TSend data) {
        try {
            // 接続チェック
            if (state != State.Connected) {
                throw new Exception("WebSocket is not connected.");
            }

            // 文字列変換
            var message = JsonUtility.ToJson(data);

            // NOTE
            // メッセージに "type" プロパティをねじ込む。
            var sb = ObjectPool<StringBuilder>.GetObject();
            sb.Append(message);
            sb.Remove(message.Length - 1, 1); // "}" 消し
            sb.AppendFormat(",\"type\":{0}}", type);
            message = sb.ToString();
            sb.Length = 0;
            ObjectPool<StringBuilder>.ReturnObject(sb);

            // 送信
            ws.SendString(message);

        } catch (Exception e) {
            Debug.LogError(e.ToString());
            return false;
        }
        return true;
    }

    // リクエスト送信
    public bool SendRequest<TSend,TRecv>(int type, TSend data, Action<string,TRecv> callback, int timeout = 0) {
        var requestNew = default(Request<TRecv>);
        try {
            // 接続チェック
            if (state != State.Connected) {
                throw new Exception("WebSocket is not connected.");
            }

            // 文字列変換
            var message = JsonUtility.ToJson(data);

            // NOTE
            // メッセージに "requestId", "requester" プロパティをねじ込む。
            var requestId = NextRequestId;
            var requester = UUID;
            var sb = ObjectPool<StringBuilder>.GetObject();
            sb.Append(message);
            sb.Remove(message.Length - 1, 1); // "}" 消し
            sb.AppendFormat(",\"type\":{0},\"requestId\":{1},\"requester\":\"{2}\"}", type, requestId, requester);
            message = sb.ToString();
            ObjectPool<StringBuilder>.ReturnObject(sb);

            // リクエストを作成して追加
            // レスポンスを待つ。
            requestNew = Request<TRecv>.GetRequest(callback);
            requestNew.requestId            = requestId;
            requestNew.requester            = requester;
            requestNew.timeoutRemainingTime = (float)((timeout > 0)? timeout : REQUEST_DEFAULT_TIMEOUT) / 1000.0f;
            requestList.Add(requestNew);

            // 送信
            ws.SendString(message);

        } catch (Exception e) {
            Debug.LogError(e.ToString());
            if (requestNew != null) {
                requestList.Remove(requestNew);
                requestNew.ReturnToPool();
            }
            callback(e.ToString(), default(TRecv));
            return false;
        }
        return true;
    }

    // レスポンス送信
    public bool SendResponse<TRecv,TSend>(TRecv requestData, TSend responseData) {
        try {
            // 接続チェック
            if (state != State.Connected) {
                throw new Exception("WebSocket is not connected.");
            }

            // 文字列変換 (リクエスト)
            var requestMessage = JsonUtility.ToJson(requestData);
            var requestId  = default(int);
            var requester  = default(string);
            var hasRequest = default(bool);
            RequestPropertyParser.Parse(requestMessage, out requestId, out requester, out hasRequest);

            // 文字列変換 (レスポンス)
            var responseMessage = JsonUtility.ToJson(responseData);

            // メッセージに "requestId", "requester" プロパティをねじ込む。
            var responseRequestId = requestId;
            var responseRequester = requester;
            var sb = ObjectPool<StringBuilder>.GetObject();
            sb.Append(responseMessage);
            sb.Remove(responseMessage.Length - 1, 1); // "}" 消し
            sb.AppendFormat("\"requestId\":{0},\"requester\":\"{1}\"}", responseRequestId, responseRequester);
            message = sb.ToString();
            ObjectPool<StringBuilder>.ReturnObject(sb);

            // 送信
            ws.SendString(sb.ToString());

        } catch (Exception e) {
            Debug.LogError(e.ToString());
            return false;
        }
        return true;
    }

    // リクエストのキャンセル
    public void CancelRequest(int requestId) {
        for (int i = requestList.Count - 1; i >= 0; i--) {
            var request = requestList[i];
            if (request.requestId == requestId) {
                requestList.RemoveAt(i);
                request.Response("cancelled.", null);
                request.ReturnToPool();
            }
        }
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

    public void AddDisconnectEventListner(Action<string> eventListener) {
        disconnectEventListener += eventListener;
    }

    public void RemoveDisconnectEventListner(Action<string> eventListener) {
        disconnectEventListener -= eventListener;
    }

    public void ClearDisconnectEventListner() {
        disconnectEventListener = null;
    }

    public void SetRequestEventListener<TRecv>(int type, Action<TRecv> eventListener) {
        if (eventListener == null) {
            SetRequestEventListener(type, null);
            return;
        }
        SetRequestEventListener(type, (message) => {
            try {
                var data = JsonUtility.FromJson<TRecv>(message);
                eventListener(data);
            } catch (Exception e) {
                Debug.LogError(e.ToString());
                return;
            }
        });
    }

    public void SetDataEventListener<TRecv>(int type, Action<TRecv> eventListener) {
        if (eventListener == null) {
            SetDataEventListener(type, null);
            return;
        }
        SetDataEventListener(type, (message) => {
            try {
                var data = JsonUtility.FromJson<TRecv>(message);
                eventListener(data);
            } catch (Exception e) {
                Debug.LogError(e.ToString());
                return;
            }
        });
    }

    protected void SetRequestEventListener(int type, Action<string> eventListener) {
        if (eventListener == null) {
            requestEventListener.Remove(type);
            return;
        }
        requestEventListener[type] = eventListener;
    }

    protected void SetDataEventListener(int type, Action<string> eventListener) {
        if (eventListener == null) {
            dataEventListener.Remove(type);
            return;
        }
        dataEventListener[type] = eventListener;
    }

    //-------------------------------------------------------------------------- イベント発行
    void InvokeConnectEvent() {
        if (connectEventListener != null) {
            connectEventListener();
        }
    }

    void InvokeDisconnectEvent(string error = null) {
        if (disconnectEventListener != null) {
            disconnectEventListener(error);
        }
    }

    void InvokeRequestEvent(int type, string message) {
        if (!requestEventListener.ContainsKey(type) || requestEventListener[type] == null) {
            return; // NOTE サイレントディスカード
        }
        requestEventListener[type](message);
    }

    void InvokeDataEvent(int type, string message) {
        if (!dataEventListener.ContainsKey(type) || dataEventListener[type] == null) {
            return; // NOTE サイレントディスカード
        }
        dataEventListener[type](message);
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
                    request.ReturnToPool();
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
            if (message == null) {
                return;
            }

            // リクエストのレスポンスであれば処理
            var requestId  = 0;
            var requester  = default(string);
            var hasRequest = false;
            MessagePropertyParser.Parse(message, out requestId, out requester, out hasRequest);
            if (hasRequest) {
                if (requester == UUID) {
                    var found = false;
                    for (int i = requestList.Count - 1; i >= 0; i--) {
                        var request = requestList[i];
                        if (request.requestId == requestId && requester != UUID) {
                            requestList.RemoveAt(i);
                            request.Response(null, message);
                            request.ReturnToPool();
                            found = true;
                        }
                    }
                    if (!found) {
                        Debug.LogError(string.Format("リクエスト番号不明 ({0}, {1})", requestId, message));
                    }
                    return;
                }
                //FALLTHROUGH
                // リクエストだが、自分が送ったものではない。
                // = 他のクライアントからのリクエストがあった。
            }

            // 型識別
            var type = TypePropertyParser.IdentifyType(message);
            if (type < 0) {
                Debug.LogError(string.Format("型番号識別不可 ({0})", message));
                return;
            }

            // ユーザコールバックに転送
            if (hasRequest) {
                InvokeRequestEvent(type, message);
                return;
            }
            InvokeDataEvent(type, message);
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

// リクエスト (基本クラス)
public partial class WebSocketConnector {
    class Request {
        //---------------------------------------------------------------------- 変数
        public int    requestId            = 0;    // リクエスト番号
        public string requester            = null; // リクエスター
        public float  timeoutRemainingTime = 0.0f; // タイムアウト残り時間

        // 転送および解放コールバック
        protected Action<Request,string,string> response      = null;
        protected Action<Request>               returnRequest = null;

        //---------------------------------------------------------------------- リクエスト操作
        // レスポンス
        public void Response(string error, string message) {
            Debug.Assert(response != null);
            response(this, error, message);
        }

        // 解放
        public void ReturnToPool() {
            Debug.Assert(returnRequest != null);
            returnRequest(this);
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
        public static Request<TRecv> GetRequest(Action<string,TRecv> callback) {
            var req = ((pool != null) && (pool.Count > 0))? pool.Dequeue() : new Request<TRecv>();
            req.requestId            = 0;
            req.requester            = null;
            req.timeoutRemainingTime = 0.0f;
            req.callback             = callback;
            req.response             = Request<TRecv>.Response;
            req.returnRequest        = Request<TRecv>.ReturnRequest;
            return req;
        }

        //---------------------------------------------------------------------- 内部コールバック用
        // 転送
        static void Response(Request request, string error, string message) {
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
        static void ReturnRequest(Request request) {
            var req = request as Request<TRecv>;
            Debug.Assert(req != null);
            req.requestId            = 0;
            req.timeoutRemainingTime = 0.0f;
            req.callback             = null;
            req.response             = null;
            req.returnRequest        = null;
            if (pool == null) {
                pool = new Queue<Request<TRecv>>();
            }
            if (pool.Count < MAX_POOL_COUNT) {
                pool.Enqueue(req);
            }
        }
    }
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// タイププロパティパーサー
// メッセージ先読み用
public partial class WebSocketConnector {
    class TypePropertyParser {
        //---------------------------------------------------------------------- 定義
        class Container { public int type; }

        //---------------------------------------------------------------------- 操作
        public static void Parse(string message, out int type, out bool hasType) {
            type    = -1;
            hasType = false;
            var container = ObjectPool<Container>.GetObject();
            container.type = -1;
            try {
                JsonUtility.FromJsonOverwrite(message, container);
                type    = container.type;
                hasType = true;
            } catch (Exception e) {
                Debug.LogError(e.ToString());
            }
            container.type = -1;
            ObjectPool<Container>.ReturnObject(container);
        }

        //---------------------------------------------------------------------- 操作 (おまけエイリアス)
        // 型識別の実行
        public static int IdentifyType(string message) {
            var type    = -1;
            var hasType = false;
            Parse(message, out type, out hasType);
            if (!hasType) {
                return -1;
            }
            return type;
        }
    }
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// リクエストプロパティパーサー
// リクエスト先読み用
public partial class WebSocketConnector {
    class RequestPropertyParser {
        //---------------------------------------------------------------------- 定義
        class Container { public int requestId; public string requester; }

        //---------------------------------------------------------------------- 操作
        public static void Parse(string message, out int requestId, out string requester, out bool hasRequest) {
            requestId  = 0;
            requester  = default(string);
            hasRequest = false;
            var container = ObjectPool<Container>.GetObject();
            container.requestId = 0;
            try {
                JsonUtility.FromJsonOverwrite(message, container);
                requestId  = container.requestId;
                requester  = container.requester;
                hasRequest = (requestId != 0 && requester != default(string));
            } catch (Exception e) {
                Debug.LogError(e.ToString());
            }
            container.requestId = 0;
            container.requester = default(string);
            ObjectPool<Container>.ReturnObject(container);
        }
    }
}
