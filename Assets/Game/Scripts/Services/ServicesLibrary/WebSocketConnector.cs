using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ウェブソケットコネクタ
// WebSocket クラスを使って実際に通信を行うコンポーネント。
public partial class WebSocketConnector : MonoBehaviour {
    //-------------------------------------------------------------------------- 定義
    // ウェブソケット状態
    public enum State { Init, Connecting, Connected }

    //-------------------------------------------------------------------------- 変数
    protected State          state             = State.Init; // 状態
    protected WebSocket      ws                = null;       // WebSocket
    protected IEnumerator    connector         = null;       // 接続制御用列挙子
    protected int            currentRetryCount = 0;          // 現在のリトライ回数
    protected string         uuid              = null;       // UUID
    protected string[]       options           = null;       // 接続時オプション
    protected RequestContext requestContext    = null;       // リクエストコンテキスト

    // イベントリスナ
    Action                         connectEventListener    = null;                                 // 接続時イベントリスナ
    Action<string>                 disconnectEventListener = null;                                 // 切断時イベントリスナ
    Dictionary<int,Action<string>> dataEventListener       = new Dictionary<int,Action<string>>(); // データ受信時イベントリスナ (型別)

    // 設定値
    public string   url           = "ws://localhost:7766"; // 接続先URL
    public string[] queries       = null;                  // クエリ一覧
    public int      retryCount    = 10;                    // 接続リトライ回数
    public float    retryInterval = 3.0f;                  // 接続リトライ間隔

    // 接続済かどうか
    public bool IsConnected { get { return (state == State.Connected); }}

    // UUID を取得する
    public string UUID { get { return uuid; }}

    //-------------------------------------------------------------------------- 実装ポイント
    // 初期化
    protected virtual void Init() {
        this.uuid    = Guid.NewGuid().ToString(); // UUID
        this.options = null;
        Clear();
    }

    // クリア
    protected virtual void Clear() {
        if (requestContext != null) {
            requestContext.Clear();
        }
        if (ws != null) {
            ws.Close();
        }

        // インスタンスメンバ
        state             = State.Init;
        ws                = null;
        connector         = null;
        currentRetryCount = 0;

        // リクエストコンテキスト
        requestContext = new RequestContext();

        // NOTE
        // Upate 無効化
        enabled = false;
    }

    //-------------------------------------------------------------------------- 接続と切断
    // 接続
    public void Connect(params string[] options) {
        this.options           = options;
        this.currentRetryCount = 0;
        Reconnect();
    }

    // 切断
    public void Disconnect(string error = null) {
        // NOTE
        // 接続中にエラーになった場合は再接続
        if (state == State.Connecting && !string.IsNullOrEmpty(error)) {
            Debug.LogError(error);
            if (currentRetryCount++ < retryCount) {
                Invoke("Reconnect", retryInterval);
                return;
            }
        }

        // クリアして切断
        var eventListener = disconnectEventListener;
        Clear();
        if (eventListener != null) {
            eventListener(error);
        }
    }

    // 再接続
    private void Reconnect() {
        // 初期状態でないなら一旦切断
        if (state != State.Init) {
            Disconnect();
        }

        // セットアップ
        Clear();

        // URL 作成
        if (   (options != null && options.Length > 0)
            || (queries != null && queries.Length > 0)) {
            var sb = ObjectPool<StringBuilder>.GetObject();
            sb.Append(url);
            if (options != null && options.Length > 0) {
                for (int i = 0; i < (options.Length - 2); i += 2) {
                    sb.Append((i == 0)? "?" : "&");
                    sb.Append(options[i + 0]); sb.Append("="); sb.Append(options[i + 1]);
                }
            }
            if (queries != null && queries.Length > 0) {
                for (int i = 0; i < (queries.Length - 2); i += 2) {
                    sb.Append((i == 0)? "?" : "&");
                    sb.Append(queries[i + 0]); sb.Append("="); sb.Append(queries[i + 1]);
                }
            }
            url = sb.ToString();
            ObjectPool<StringBuilder>.ReturnObject(sb);
        }

        // 接続開始
        state     = State.Connecting;
        ws        = new WebSocket(new Uri(url));
        connector = ws.Connect();
        enabled   = true; // NOTE Update 有効化
    }

    //-------------------------------------------------------------------------- 送信とキャンセル
    // 送信
    public bool Send<TSend>(int type, TSend data) {
        var error = Transmit<TSend>(type, data);
        if (error != null) {
            return false;
        }
        return true;
    }

    // 送信 (リクエスト)
    public int Send<TSend,TRecv>(int type, TSend data, Action<string,TRecv> callback, float timeout = 10.0f) {
        var requestId = requestContext.NextRequestId();
        var requester = UUID;

        // リクエスト作成
        var request = Request<TRecv>.GetRequest(requestId, callback, timeout);
        requestContext.SetRequest(request);

        // 送信
        var error = Transmit<TSend>(type, data, requestId, requester);
        if (error != null) {
            requestContext.CancelRequest(requestId);
            return 0;
        }
        return requestId;
    }

    // リクエストのキャンセル
    public void CancelRequest(int requestId) {
        requestContext.CancelRequest(requestId);
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

    public void SetDataEventListener<TRecv>(int type, Action<TRecv> eventListener) { // NOTE レスポンスしないデータイベントリスナ
        if (eventListener == null) {
            dataEventListener.Remove(type);
            return;
        }
        dataEventListener[type] = (message) => {
            try {
                var data = JsonUtility.FromJson<TRecv>(message);
                eventListener(data);
            } catch (Exception e) {
                Debug.LogError(e.ToString());
                return;
            }
        };
    }

    public void SetDataEventListener<TRecv,TResponse>(int type, Action<TRecv,Response<TResponse>> eventListener) { // NOTE レスポンスするデータイベントリスナ
        if (eventListener == null) {
            dataEventListener.Remove(type);
            return;
        }
        dataEventListener[type] = (message) => {
            try {
                var data = JsonUtility.FromJson<TRecv>(message);
                var res  = new Response<TResponse>();
                res.connector = this;
                res.type      = type;
                RequestPropertyParser.TryParse(message, out res.requestId, out res.requester);
                eventListener(data, res);
            } catch (Exception e) {
                Debug.LogError(e.ToString());
                return;
            }
        };
    }

    protected void SetDataMessageEventListener(int type, Action<string> eventListener) { // NOTE メッセージそのままを受け取るデータイベントリスナ (継承用)
        if (eventListener == null) {
            dataEventListener.Remove(type);
            return;
        }
        dataEventListener[type] = eventListener;
    }

    //-------------------------------------------------------------------------- 実装 (MonoBehaviour)
    void Awake() {
        Init();
    }

    void Update() {
        var deltaTime = Time.deltaTime;

        // リクエストのタイムアウトチェック
        if (requestContext != null) {
            requestContext.CheckTimeout(deltaTime);
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
            if (connectEventListener != null) {
                connectEventListener();
            }
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
            if (RequestPropertyParser.TryParse(message, out requestId, out requester)) {
                if (requester == UUID) {
                    if (requestContext != null) {
                        requestContext.SetResponse(requestId, null, message);
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
            if (dataEventListener.ContainsKey(type) && dataEventListener[type] != null) {
                dataEventListener[type](message);
            }
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

// リクエストコンテキスト
// 送信済リクエスト情報を保持し、管理します。
public partial class WebSocketConnector {
    public class RequestContext {
        //-------------------------------------------------------------------------- 変数
        List<Request> requestList      = new List<Request>(); // 送信中のリクエスト一覧
        int           requestIdCounter = 0;                   // リクエストIDカウンタ

        //-------------------------------------------------------------------------- 初期化とクリア
        // コンストラクタ
        public RequestContext() {
            this.Init();
        }

        // 初期化
        public virtual void Init() {
            this.Clear();
        }

        // クリア
        public virtual void Clear() {
            for (int i = requestList.Count - 1; i >= 0; i--) {
                var request = requestList[i];
                requestList.RemoveAt(i);
                request.SetResponse("abort.", null);
                request.ReturnToPool();
            }
            requestList.Clear();
            requestIdCounter = 0;
        }

        //-------------------------------------------------------------------------- 操作
        // 次のリクエストID の取得
        public int NextRequestId() {
            return ((++requestIdCounter == 0)? ++requestIdCounter : requestIdCounter);
        }

        // リクエストを設定
        public virtual void SetRequest(Request request) {
            requestList.Add(request);
        }

        // レスポンスを設定
        public void SetResponse(int requestId, string error, string message) {
            for (int i = requestList.Count - 1; i >= 0; i--) {
                var request = requestList[i];
                if (request.requestId == requestId) {
                    requestList.RemoveAt(i);
                    request.SetResponse(error, message);
                    request.ReturnToPool();
                }
            }
        }

        // リクエストをキャンセル
        public void CancelRequest(int requestId) {
            this.SetResponse(requestId, "cancelled.", null);
        }

        //-------------------------------------------------------------------------- 操作 (その他)
        // リクエストのタイムアウトチェック
        public void CheckTimeout(float deltaTime) {
            for (int i = requestList.Count - 1; i >= 0; i--) {
                var request = requestList[i];
                request.timeoutRemainingTime -= deltaTime;
                if (request.timeoutRemainingTime <= 0.0f) {
                    requestList.RemoveAt(i);
                    request.SetResponse("timeout", null);
                    request.ReturnToPool();
                }
            }
        }
    }
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// リクエスト
// リクエスト中の情報を保持するためのクラス。
public partial class WebSocketConnector {
    public class Request {
        //---------------------------------------------------------------------- 変数
        public int    requestId            = 0;    // リクエスト番号
        public float  timeoutRemainingTime = 0.0f; // タイムアウト残り時間

        // 転送および解放コールバック
        // サブクラスが設定
        protected Action<Request,string,string> setResponse  = null;
        protected Action<Request>               returnToPool = null;

        //---------------------------------------------------------------------- 操作
        // レスポンスデータを設定
        public void SetResponse(string error, string message) {
            Debug.Assert(setResponse != null);
            setResponse(this, error, message);
        }

        // 解放
        public void ReturnToPool() {
            Debug.Assert(returnToPool != null);
            returnToPool(this);
        }
    }
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// リクエスト (型別)
// リクエスト中の情報を保持するためのクラス。
public partial class WebSocketConnector {
    public class Request<TRecv> : Request {
        //---------------------------------------------------------------------- 変数
        // 受信コールバック
        protected Action<string,TRecv> callback = null;

        //---------------------------------------------------------------------- 確保
        // 確保
        public static Request<TRecv> GetRequest(int requestId, Action<string,TRecv> callback, float timeout = 10.0f) {
            var req = ObjectPool<Request<TRecv>>.GetObject();
            req.requestId            = requestId;
            req.timeoutRemainingTime = timeout;
            req.setResponse          = Request<TRecv>.SetResponse;
            req.returnToPool         = Request<TRecv>.ReturnToPool;
            req.callback             = callback;
            return req;
        }

        //---------------------------------------------------------------------- 内部コールバック用
        // レスポンスデータを設定
        static void SetResponse(Request request, string err, string message) {
            var req = request as Request<TRecv>;
            Debug.Assert(req != null);
            Debug.Assert(req.callback != null);
            try {
                if (err != null) {
                    throw new Exception(err);
                }
                var data = JsonUtility.FromJson<TRecv>(message);
                req.callback(null, data);
            } catch (Exception e) {
                req.callback(e.ToString(), default(TRecv));
            }
        }

        // 解放
        static void ReturnToPool(Request request) {
            var req = request as Request<TRecv>;
            Debug.Assert(req != null);
            req.requestId            = 0;
            req.timeoutRemainingTime = 0.0f;
            req.setResponse          = null;
            req.returnToPool         = null;
            req.callback             = null;
            ObjectPool<Request<TRecv>>.ReturnObject(req);
        }
    }
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// レスポンス (型別)
// Send のコールバックで res.Send(data) を実現するためのクラス。
public partial class WebSocketConnector {
    public struct Response<TResponse> {
        //---------------------------------------------------------------------- 変数
        public WebSocketConnector connector;
        public int                type;
        public int                requestId;
        public string             requester;

        //---------------------------------------------------------------------- 操作
        // 送信元に向けて送り返す
        public void Send(TResponse data) {
            connector.Transmit<TResponse>(type, data, requestId, requester);
        }
    }
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// タイププロパティパーサー
// メッセージ先読み用
public partial class WebSocketConnector {
    public class TypePropertyParser {
        //---------------------------------------------------------------------- 定義
        class Container { public int type; }

        //---------------------------------------------------------------------- 操作
        public static bool TryParse(string message, out int type) {
            var hasType = false;
            type = -1;
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
            return hasType;
        }

        //---------------------------------------------------------------------- 操作 (おまけエイリアス)
        // 型識別の実行
        public static int IdentifyType(string message) {
            var type = -1;
            TryParse(message, out type);
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
    public class RequestPropertyParser {
        //---------------------------------------------------------------------- 定義
        class Container { public int requestId; public string requester; }

        //---------------------------------------------------------------------- 操作
        public static bool TryParse(string message, out int requestId, out string requester) {
            var hasRequest = false;
            requestId  = 0;
            requester  = default(string);
            var container = ObjectPool<Container>.GetObject();
            container.requestId = 0;
            container.requester = default(string);
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
            return hasRequest;
        }
    }
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// 送出
// 送出ロジックは繰り返し使うのでユーティリティとして外に出しておく。
public partial class WebSocketConnector {
    //-------------------------------------------------------------------------- 操作
    // 送出
    protected string Transmit<TSend>(int type, TSend data, int requestId = -1, string requester = null) {
        try {
            // 接続チェック
            if (state != State.Connected) {
                throw new Exception("WebSocket is not connected.");
            }

            // 文字列変換
            var message = JsonUtility.ToJson(data);

            // NOTE
            // メッセージに "type", "requestId", "requester" プロパティをねじ込む。
            var sb = ObjectPool<StringBuilder>.GetObject();
			sb.Length = 0;
            sb.Append(message);
            sb.Remove(message.Length - 1, 1); // "}" 消し
            if (requestId >= 0 && requester != null) {
				sb.AppendFormat(",\"type\":{0},\"requestId\":{1},\"requester\":\"{2}\"}}", type, requestId, requester);
            } else {
				sb.AppendFormat(",\"type\":{0}}}", type);
            }
            message = sb.ToString();
            ObjectPool<StringBuilder>.ReturnObject(sb);

            // 送信
            ws.SendString(message);

        } catch (Exception e) {
            return e.ToString();
        }
        return null;
    }
}
