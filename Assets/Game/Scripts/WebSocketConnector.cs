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
    State       state             = State.Init; // 状態
    WebSocket   ws                = null;       // WebSocket
    IEnumerator connector         = null;       // 接続制御用列挙子
    int         currentRetryCount = 0;          // 現在のリトライ回数
    string      uuid              = null;       // UUID
    string[]    options           = null;       // 接続時オプション

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

    // リクエスト番号カウンター
    protected static int requestIdCounter = 0;

    // リクエスト番号取得用 (コール毎にリクエスト番号カウンターを +1)
    protected static int NextRequestId { get { return (++requestIdCounter == 0)? ++requestIdCounter : requestIdCounter; }}

    //-------------------------------------------------------------------------- 実装ポイント
    // 初期化
    protected virtual void Init() {
        this.uuid    = Guid.NewGuid().ToString(); // UUID
        this.options = null;
        Clear();
    }

    // クリア
    protected virtual void Clear() {
        if (ws != null) {
            ws.Close();
        }
        state             = State.Init;
        ws                = null;
        connector         = null;
        currentRetryCount = 0;
        enabled           = false; // NOTE Update 無効化
    }

    // 実装のカスタマイズ
    protected virtual void CustomizeBehaviour() {
        // 継承して実装
    }

    //-------------------------------------------------------------------------- 接続と切断
    // 接続
    public void Connect(params string[] options) {
        this.options           = options;
        this.currentRetryCount = 0;
        Reconnect();
    }

    // 再接続
    public void Reconnect() {
        // 初期状態でないなら一旦切断
        if (state != State.Init) {
            Disconnect();
        }

        // セットアップ
        Clear();
        CustomizeBehaviour();

        // URL 作成
        if (options.Length > 0 || queries.Length > 0) {
            var sb = ObjectPool<StringBuilder>.GetObject();
            sb.Append(url);
            if (options.Length > 0) {
                for (int i = 0; i < (options.Length - 2); i += 2) {
                    sb.Append((i == 0)? "?" : "&");
                    sb.Append(options[i + 0]); sb.Append("="); sb.Append(options[i + 1]);
                }
            }
            if (queries.Length > 0) {
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

    //-------------------------------------------------------------------------- 送信とリクエスト
    // TODO
    //// 送信
    //public bool Send<TSend>(int type, TSend data) {
    //    try {
    //        // 接続チェック
    //        if (state != State.Connected) {
    //            throw new Exception("WebSocket is not connected.");
    //        }
    //
    //        // 文字列変換
    //        var message = JsonUtility.ToJson(data);
    //
    //        // NOTE
    //        // メッセージに "type" プロパティをねじ込む。
    //        var sb = ObjectPool<StringBuilder>.GetObject();
    //        sb.Append(message);
    //        sb.Remove(message.Length - 1, 1); // "}" 消し
    //        sb.AppendFormat(",\"type\":{0}}", type);
    //        message = sb.ToString();
    //        sb.Length = 0;
    //        ObjectPool<StringBuilder>.ReturnObject(sb);
    //
    //        // 送信
    //        ws.SendString(message);
    //
    //    } catch (Exception e) {
    //        Debug.LogError(e.ToString());
    //        return false;
    //    }
    //    return true;
    //}
    //
    //// リクエスト送信
    //public bool Request<TSend,TRecv>(int type, TSend data, Action<string,TRecv> callback, int timeout = 0) {
    //    var requestInfo = default(RequestInfo<TRecv>);
    //    try {
    //        // 接続チェック
    //        if (state != State.Connected) {
    //            throw new Exception("WebSocket is not connected.");
    //        }
    //
    //        // 文字列変換
    //        var message = JsonUtility.ToJson(data);
    //
    //        // リクエスト番号、送信 UUID を設定
    //        var requestId = NextRequestId;
    //        var requester = UUID;
    //
    //        // NOTE
    //        // メッセージに "requestId", "requester" プロパティをねじ込む。
    //        var sb = ObjectPool<StringBuilder>.GetObject();
    //        sb.Append(message);
    //        sb.Remove(message.Length - 1, 1); // "}" 消し
    //        sb.AppendFormat(",\"type\":{0},\"requestId\":{1},\"requester\":\"{2}\"}", type, requestId, requester);
    //        message = sb.ToString();
    //        ObjectPool<StringBuilder>.ReturnObject(sb);
    //
    //        // リクエストを作成して追加
    //        // レスポンスを待つ。
    //        requestInfo = RequestInfo<TRecv>.GetRequest(callback);
    //        requestInfo.requestId            = requestId;
    //        requestInfo.requester            = requester;
    //        requestInfo.timeoutRemainingTime = (float)((timeout > 0)? timeout : REQUEST_DEFAULT_TIMEOUT) / 1000.0f;
    //        requestList.Add(requestInfo);
    //
    //        // 送信
    //        ws.SendString(message);
    //
    //    } catch (Exception e) {
    //        Debug.LogError(e.ToString());
    //        if (requestInfo != null) {
    //            requestList.Remove(requestInfo);
    //            requestInfo.ReturnToPool();
    //        }
    //        callback(e.ToString(), default(TRecv));
    //        return false;
    //    }
    //    return true;
    //}
    //
    //// レスポンス送信
    //public bool Response<TSend>(ResponseInfo responseInfo, TSend data) {
    //    try {
    //        // 接続チェック
    //        if (state != State.Connected) {
    //            throw new Exception("WebSocket is not connected.");
    //        }
    //
    //        // 文字列変換 (レスポンス)
    //        var message = JsonUtility.ToJson(data);
    //
    //        // リクエスト番号、送信 UUID を設定
    //        var requestId  = 0;
    //        var requester  = default(string);
    //        RequestPropertyParser.TryParse(responseInfo.message, out requestId, out requester);
    //
    //        // メッセージに "requestId", "requester" プロパティをねじ込む。
    //        var sb = ObjectPool<StringBuilder>.GetObject();
    //        sb.Append(message);
    //        sb.Remove(message.Length - 1, 1); // "}" 消し
    //        sb.AppendFormat(",\"requestId\":{0},\"requester\":\"{1}\"}", requestId, requester);
    //        message = sb.ToString();
    //        ObjectPool<StringBuilder>.ReturnObject(sb);
    //
    //        // 送信
    //        ws.SendString(message);
    //
    //    } catch (Exception e) {
    //        Debug.LogError(e.ToString());
    //        return false;
    //    }
    //    return true;
    //}
    //
    //// リクエストのキャンセル
    //public void CancelRequest(int requestId) {
    //    for (int i = requestList.Count - 1; i >= 0; i--) {
    //        var requestInfo = requestList[i];
    //        if (requestInfo.requestId == requestId) {
    //            requestList.RemoveAt(i);
    //            requestInfo.Response("cancelled.", null);
    //            requestInfo.ReturnToPool();
    //        }
    //    }
    //}

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

    // TODO
    //public void SetDataEventListener<TRecv>(int type, Action<TRecv,ResponseInfo> eventListener) {
    //    if (eventListener == null) {
    //        dataEventListener.Remove(type);
    //        return;
    //    }
    //    if (typeof(TRecv) == typeof(string)) {
    //        dataEventListener[type] = (message) => {
    //            var data         = (TRecv)(object)message;
    //            var responseInfo = new ResponseInfo() { message = message };
    //            eventListener(data, responseInfo);
    //        };
    //        return;
    //    }
    //    dataEventListener[type] = (message) => {
    //        try {
    //            var data         = JsonUtility.FromJson<TRecv>(message);
    //            var responseInfo = new ResponseInfo() { message = message };
    //            responseInfo.message = message;
    //            eventListener(data, responseInfo);
    //        } catch (Exception e) {
    //            Debug.LogError(e.ToString());
    //            return;
    //        }
    //    };
    //}

    //-------------------------------------------------------------------------- 実装 (MonoBehaviour)
    void Awake() {
        Init();
    }

    void Update() {
        var deltaTime = Time.deltaTime;

        // TODO
        // リクエストのタイムアウトチェック
        //if (requestList != null) {
        //    for (int i = requestList.Count - 1; i >= 0; i--) {
        //        var requestInfo = requestList[i];
        //        requestInfo.timeoutRemainingTime -= deltaTime;
        //        if (requestInfo.timeoutRemainingTime <= 0.0f) {
        //            requestList.RemoveAt(i);
        //            requestInfo.Response("timeout.", null);
        //            requestInfo.ReturnToPool();
        //        }
        //    }
        //}

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

            // TODO
            // リクエストのレスポンスであれば処理
            //var requestId  = 0;
            //var requester  = default(string);
            //var hasRequest = RequestPropertyParser.TryParse(message, out requestId, out requester);
            //if (hasRequest) {
            //    if (requester == UUID) {
            //        var found = false;
            //        for (int i = requestList.Count - 1; i >= 0; i--) {
            //            var requestInfo = requestList[i];
            //            if (requestInfo.requestId == requestId && requester != UUID) {
            //                requestList.RemoveAt(i);
            //                requestInfo.Response(null, message);
            //                requestInfo.ReturnToPool();
            //                found = true;
            //            }
            //        }
            //        if (!found) {
            //            Debug.LogError(string.Format("リクエスト番号不明 ({0}, {1})", requestId, message));
            //        }
            //        return;
            //    }
            //    //FALLTHROUGH
            //    // リクエストだが、自分が送ったものではない。
            //    // = 他のクライアントからのリクエストがあった。
            //}

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
//
//// リクエスト情報
//public partial class WebSocketConnector {
//    public class RequestInfo {
//        //---------------------------------------------------------------------- 変数
//        public int    requestId            = 0;    // リクエスト番号
//        public string requester            = null; // リクエスター
//        public float  timeoutRemainingTime = 0.0f; // タイムアウト残り時間
//
//        // 転送および解放コールバック
//        protected Action<RequestInfo,string,string> response      = null;
//        protected Action<RequestInfo>               returnRequest = null;
//
//        //---------------------------------------------------------------------- リクエスト操作
//        // レスポンス
//        public void Response(string error, string message) {
//            Debug.Assert(response != null);
//            response(this, error, message);
//        }
//
//        // 解放
//        public void ReturnToPool() {
//            Debug.Assert(returnRequest != null);
//            returnRequest(this);
//        }
//    }
//
//
//}
//
//// ウェブソケットコネクタ
//// WebSocket クラスを使って実際に通信を行うコンポーネント。
//public partial class WebSocketConnector : MonoBehaviour {
//    //-------------------------------------------------------------------------- 定義
//    // ウェブソケット状態
//    public enum State { Init, Connecting, Connected }
//
//    // リクエストのデフォルトタイムアウト時間 [ms]
//    protected static readonly int REQUEST_DEFAULT_TIMEOUT = 5000;
//
//    //-------------------------------------------------------------------------- 変数
//    public string   url           = "ws://localhost:7766"; // 接続先URL
//    public string[] queries       = null;                  // クエリ一覧
//    public int      retryCount    = 10;                    // 接続リトライ回数
//    public float    retryInterval = 3.0f;                  // 接続リトライ間隔
//
//    protected State             state       = State.Init; // 状態
//    protected WebSocket         ws          = null;       // ウェブソケット
//    protected IEnumerator       connector   = null;       // 接続制御用列挙子
//    protected List<RequestInfo> requestList = null;       // リクエストリスト
//
//    // イベントリスナ
//    Action                         connectEventListener     = null;                                 // 接続時イベントリスナ
//    Action<string>                 disconnectEventListener  = null;                                 // 切断時イベントリスナ
//    Dictionary<int,Action<string>> requestEventListener     = new Dictionary<int,Action<string>>(); // リクエスト受信時イベントリスナ (型別)
//    Dictionary<int,Action<string>> dataEventListener        = new Dictionary<int,Action<string>>(); // データ受信時イベントリスナ     (型別)
//
//    // 現在のリトライ回数
//    int currentRetryCount = 0;
//
//    // UUID
//    string uuid = null;
//
//    // UUID を取得する
//    public string UUID { get { return uuid ?? (uuid = Guid.NewGuid().ToString()); }}
//
//    // 接続済かどうか
//    public bool IsConnected { get { return (state == State.Connected); }}
//
//    // リクエスト番号カウンター
//    protected static int requestIdCounter = 0;
//
//    // リクエスト番号取得用 (コール毎にリクエスト番号カウンターを +1)
//    protected static int NextRequestId { get { return (++requestIdCounter == 0)? ++requestIdCounter : requestIdCounter; }}
//
//    //-------------------------------------------------------------------------- 接続と切断
//    public void Connect(params string[] queries) {
//        this.queries           = queries;
//        this.currentRetryCount = 0;
//        Reconnect();
//    }
//
//    public void Reconnect() {
//        // 初期状態でないなら一旦切断
//        if (state != State.Init) {
//            Disconnect();
//        }
//
//        // クエリパラメータ付与
//        if (queries.Length > 0) {
//            var sb = ObjectPool<StringBuilder>.GetObject();
//            sb.Append(url);
//            for (int i = 0; i < (queries.Length - 1); i += 2) {
//                var key = queries[i + 0];
//                var val = queries[i + 1];
//                sb.Append((i == 0)? "?" : "&");
//                sb.Append(key); sb.Append("="); sb.Append(val);
//            }
//            url = sb.ToString();
//            ObjectPool<StringBuilder>.ReturnObject(sb);
//        }
//
//        // 接続
//        state       = State.Connecting;
//        ws          = new WebSocket(new Uri(url));
//        connector   = ws.Connect();
//        requestList = new List<RequestInfo>();
//        enabled     = true;
//    }
//
//    public void Disconnect(string error = null) {
//        if (state == State.Connecting && !string.IsNullOrEmpty(error)) {
//            Debug.LogError(error);
//            if (currentRetryCount++ < retryCount) {
//                Invoke("Reconnect", retryInterval); // NOTE 再接続
//                return;
//            }
//        }
//        InvokeDisconnectEvent(error);
//        if (ws != null) {
//            ws.Close();
//        }
//        if (requestList != null) {
//            for (int i = requestList.Count - 1; i >= 0; i--) {
//                var request = requestList[i];
//                request.Response(error ?? "abort.", null);
//                request.ReturnToPool();
//                requestList.RemoveAt(i);
//            }
//            requestList.Clear();
//        }
//        state       = State.Init;
//        ws          = null;
//        connector   = null;
//        requestList = null;
//        enabled     = false;
//    }
//
//    //-------------------------------------------------------------------------- 送信とリクエスト
//    // 送信
//    public bool Send<TSend>(int type, TSend data) {
//        try {
//            // 接続チェック
//            if (state != State.Connected) {
//                throw new Exception("WebSocket is not connected.");
//            }
//
//            // 文字列変換
//            var message = JsonUtility.ToJson(data);
//
//            // NOTE
//            // メッセージに "type" プロパティをねじ込む。
//            var sb = ObjectPool<StringBuilder>.GetObject();
//            sb.Append(message);
//            sb.Remove(message.Length - 1, 1); // "}" 消し
//            sb.AppendFormat(",\"type\":{0}}", type);
//            message = sb.ToString();
//            sb.Length = 0;
//            ObjectPool<StringBuilder>.ReturnObject(sb);
//
//            // 送信
//            ws.SendString(message);
//
//        } catch (Exception e) {
//            Debug.LogError(e.ToString());
//            return false;
//        }
//        return true;
//    }
//
//    // リクエスト送信
//    public bool Request<TSend,TRecv>(int type, TSend data, Action<string,TRecv> callback, int timeout = 0) {
//        var requestInfo = default(RequestInfo<TRecv>);
//        try {
//            // 接続チェック
//            if (state != State.Connected) {
//                throw new Exception("WebSocket is not connected.");
//            }
//
//            // 文字列変換
//            var message = JsonUtility.ToJson(data);
//
//            // リクエスト番号、送信 UUID を設定
//            var requestId = NextRequestId;
//            var requester = UUID;
//
//            // NOTE
//            // メッセージに "requestId", "requester" プロパティをねじ込む。
//            var sb = ObjectPool<StringBuilder>.GetObject();
//            sb.Append(message);
//            sb.Remove(message.Length - 1, 1); // "}" 消し
//            sb.AppendFormat(",\"type\":{0},\"requestId\":{1},\"requester\":\"{2}\"}", type, requestId, requester);
//            message = sb.ToString();
//            ObjectPool<StringBuilder>.ReturnObject(sb);
//
//            // リクエストを作成して追加
//            // レスポンスを待つ。
//            requestInfo = RequestInfo<TRecv>.GetRequest(callback);
//            requestInfo.requestId            = requestId;
//            requestInfo.requester            = requester;
//            requestInfo.timeoutRemainingTime = (float)((timeout > 0)? timeout : REQUEST_DEFAULT_TIMEOUT) / 1000.0f;
//            requestList.Add(requestInfo);
//
//            // 送信
//            ws.SendString(message);
//
//        } catch (Exception e) {
//            Debug.LogError(e.ToString());
//            if (requestInfo != null) {
//                requestList.Remove(requestInfo);
//                requestInfo.ReturnToPool();
//            }
//            callback(e.ToString(), default(TRecv));
//            return false;
//        }
//        return true;
//    }
//
//    // レスポンス送信
//    public bool Response<TSend>(ResponseInfo responseInfo, TSend data) {
//        try {
//            // 接続チェック
//            if (state != State.Connected) {
//                throw new Exception("WebSocket is not connected.");
//            }
//
//            // 文字列変換 (レスポンス)
//            var message = JsonUtility.ToJson(data);
//
//            // リクエスト番号、送信 UUID を設定
//            var requestId  = 0;
//            var requester  = default(string);
//            RequestPropertyParser.TryParse(responseInfo.message, out requestId, out requester);
//
//            // メッセージに "requestId", "requester" プロパティをねじ込む。
//            var sb = ObjectPool<StringBuilder>.GetObject();
//            sb.Append(message);
//            sb.Remove(message.Length - 1, 1); // "}" 消し
//            sb.AppendFormat(",\"requestId\":{0},\"requester\":\"{1}\"}", requestId, requester);
//            message = sb.ToString();
//            ObjectPool<StringBuilder>.ReturnObject(sb);
//
//            // 送信
//            ws.SendString(message);
//
//        } catch (Exception e) {
//            Debug.LogError(e.ToString());
//            return false;
//        }
//        return true;
//    }
//
//    // リクエストのキャンセル
//    public void CancelRequest(int requestId) {
//        for (int i = requestList.Count - 1; i >= 0; i--) {
//            var requestInfo = requestList[i];
//            if (requestInfo.requestId == requestId) {
//                requestList.RemoveAt(i);
//                requestInfo.Response("cancelled.", null);
//                requestInfo.ReturnToPool();
//            }
//        }
//    }
//
//    //-------------------------------------------------------------------------- イベントリスナ設定
//    public void AddConnectEventListner(Action eventListener) {
//        connectEventListener += eventListener;
//    }
//
//    public void RemoveConnectEventListner(Action eventListener) {
//        connectEventListener -= eventListener;
//    }
//
//    public void ClearConnectEventListner() {
//        connectEventListener = null;
//    }
//
//    public void AddDisconnectEventListner(Action<string> eventListener) {
//        disconnectEventListener += eventListener;
//    }
//
//    public void RemoveDisconnectEventListner(Action<string> eventListener) {
//        disconnectEventListener -= eventListener;
//    }
//
//    public void ClearDisconnectEventListner() {
//        disconnectEventListener = null;
//    }
//
//    public void SetRequestEventListener<TRecv>(int type, Action<TRecv,ResponseInfo> eventListener) {
//        if (eventListener == null) {
//            requestEventListener.Remove(type);
//            return;
//        }
//        if (typeof(TRecv) == typeof(string)) {
//            requestEventListener[type] = (message) => {
//                var data         = (TRecv)(object)message;;
//                var responseInfo = new ResponseInfo() { message = message };
//                eventListener(data, responseInfo);
//            };
//            return;
//        }
//        requestEventListener[type] = (message) => {
//            try {
//                var data         = JsonUtility.FromJson<TRecv>(message);
//                var responseInfo = new ResponseInfo() { message = message };
//                responseInfo.message = message;
//                eventListener(data, responseInfo);
//            } catch (Exception e) {
//                Debug.LogError(e.ToString());
//                return;
//            }
//        };
//    }
//
//    public void SetDataEventListener<TRecv>(int type, Action<TRecv> eventListener) {
//        if (eventListener == null) {
//            dataEventListener.Remove(type);
//            return;
//        }
//        if (typeof(TRecv) == typeof(string)) {
//            dataEventListener[type] = (message) => {
//                var data = (TRecv)(object)message;;
//                eventListener(data);
//            };
//            return;
//        }
//        dataEventListener[type] = (message) => {
//            try {
//                var data = JsonUtility.FromJson<TRecv>(message);
//                eventListener(data);
//            } catch (Exception e) {
//                Debug.LogError(e.ToString());
//                return;
//            }
//        };
//    }
//
//    //-------------------------------------------------------------------------- イベント発行
//    void InvokeConnectEvent() {
//        if (connectEventListener != null) {
//            connectEventListener();
//        }
//    }
//
//    void InvokeDisconnectEvent(string error = null) {
//        if (disconnectEventListener != null) {
//            disconnectEventListener(error);
//        }
//    }
//
//    void InvokeRequestEvent(int type, string message) {
//        if (requestEventListener.ContainsKey(type) && requestEventListener[type] != null) {
//            requestEventListener[type](message);
//        }
//    }
//
//    void InvokeDataEvent(int type, string message) {
//        if (dataEventListener.ContainsKey(type) && dataEventListener[type] != null) {
//            dataEventListener[type](message);
//        }
//    }
//
//    //-------------------------------------------------------------------------- 実装 (MonoBehaviour)
//    void Update() {
//        var deltaTime = Time.deltaTime;
//
//        // リクエストのタイムアウトチェック
//        if (requestList != null) {
//            for (int i = requestList.Count - 1; i >= 0; i--) {
//                var requestInfo = requestList[i];
//                requestInfo.timeoutRemainingTime -= deltaTime;
//                if (requestInfo.timeoutRemainingTime <= 0.0f) {
//                    requestList.RemoveAt(i);
//                    requestInfo.Response("timeout.", null);
//                    requestInfo.ReturnToPool();
//                }
//            }
//        }
//
//        // 状態毎の処理
//        switch (state) {
//        case State.Init:
//            // NOTE
//            // 初期状態では停止させる。
//            // Connect, Disconnect したとき改めて設定される。
//            enabled = false;
//            break;
//
//        case State.Connecting:
//            if (connector.MoveNext()) {
//                return;
//            }
//            if (ws.error != null) {
//                Disconnect(ws.error);
//                return;
//            }
//            state = State.Connected;
//            InvokeConnectEvent();
//            break;
//
//        case State.Connected:
//            if (ws.error != null) {
//                Disconnect(ws.error);
//                return;
//            }
//            var message = ws.RecvString();
//            if (message == null) {
//                return;
//            }
//
//            // リクエストのレスポンスであれば処理
//            var requestId  = 0;
//            var requester  = default(string);
//            var hasRequest = RequestPropertyParser.TryParse(message, out requestId, out requester);
//            if (hasRequest) {
//                if (requester == UUID) {
//                    var found = false;
//                    for (int i = requestList.Count - 1; i >= 0; i--) {
//                        var requestInfo = requestList[i];
//                        if (requestInfo.requestId == requestId && requester != UUID) {
//                            requestList.RemoveAt(i);
//                            requestInfo.Response(null, message);
//                            requestInfo.ReturnToPool();
//                            found = true;
//                        }
//                    }
//                    if (!found) {
//                        Debug.LogError(string.Format("リクエスト番号不明 ({0}, {1})", requestId, message));
//                    }
//                    return;
//                }
//                //FALLTHROUGH
//                // リクエストだが、自分が送ったものではない。
//                // = 他のクライアントからのリクエストがあった。
//            }
//
//            // 型識別
//            var type = TypePropertyParser.IdentifyType(message);
//            if (type < 0) {
//                Debug.LogError(string.Format("型番号識別不可 ({0})", message));
//                return;
//            }
//
//            // ユーザコールバックに転送
//            if (hasRequest) {
//                InvokeRequestEvent(type, message);
//                return;
//            }
//            InvokeDataEvent(type, message);
//            break;
//
//        default:
//            Debug.LogError(string.Format("状態不明 ({0})", state));
//            break;
//        }
//    }
//}
//
//////////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////////
//
//// リクエスト情報
//public partial class WebSocketConnector {
//    public class RequestInfo {
//        //---------------------------------------------------------------------- 変数
//        public int    requestId            = 0;    // リクエスト番号
//        public string requester            = null; // リクエスター
//        public float  timeoutRemainingTime = 0.0f; // タイムアウト残り時間
//
//        // 転送および解放コールバック
//        protected Action<RequestInfo,string,string> response      = null;
//        protected Action<RequestInfo>               returnRequest = null;
//
//        //---------------------------------------------------------------------- リクエスト操作
//        // レスポンス
//        public void Response(string error, string message) {
//            Debug.Assert(response != null);
//            response(this, error, message);
//        }
//
//        // 解放
//        public void ReturnToPool() {
//            Debug.Assert(returnRequest != null);
//            returnRequest(this);
//        }
//    }
//}
//
//////////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////////
//
//// リクエスト情報 (型別)
//public partial class WebSocketConnector {
//    public class RequestInfo<TRecv> : RequestInfo {
//        //---------------------------------------------------------------------- 定義
//        static readonly int MAX_POOL_COUNT = 16;
//
//        //---------------------------------------------------------------------- 変数
//        // 受信コールバック
//        protected Action<string,TRecv> callback = null;
//
//        // リクエスト情報プール
//        static Queue<RequestInfo<TRecv>> pool = null;
//
//        //---------------------------------------------------------------------- 確保
//        // 確保
//        public static RequestInfo<TRecv> GetRequest(Action<string,TRecv> callback) {
//            var req = ((pool != null) && (pool.Count > 0))? pool.Dequeue() : new RequestInfo<TRecv>();
//            req.requestId            = 0;
//            req.requester            = null;
//            req.timeoutRemainingTime = 0.0f;
//            req.callback             = callback;
//            req.response             = RequestInfo<TRecv>.Response;
//            req.returnRequest        = RequestInfo<TRecv>.ReturnRequest;
//            return req;
//        }
//
//        //---------------------------------------------------------------------- 内部コールバック用
//        // 転送
//        static void Response(RequestInfo requestInfo, string error, string message) {
//            var req = requestInfo as RequestInfo<TRecv>;
//            Debug.Assert(req != null);
//            Debug.Assert(req.callback != null);
//            try {
//                if (error != null) {
//                    throw new Exception(error);
//                }
//                var data = JsonUtility.FromJson<TRecv>(message);
//                req.callback(null, data);
//            } catch (Exception e) {
//                req.callback(e.ToString(), default(TRecv));
//            }
//        }
//
//        // 解放
//        static void ReturnRequest(RequestInfo requestInfo) {
//            var req = requestInfo as RequestInfo<TRecv>;
//            Debug.Assert(req != null);
//            req.requestId            = 0;
//            req.timeoutRemainingTime = 0.0f;
//            req.callback             = null;
//            req.response             = null;
//            req.returnRequest        = null;
//            if (pool == null) {
//                pool = new Queue<RequestInfo<TRecv>>();
//            }
//            if (pool.Count < MAX_POOL_COUNT) {
//                pool.Enqueue(req);
//            }
//        }
//    }
//}
//
//////////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////////
//
//// レスポンス情報
//public partial class WebSocketConnector {
//    public struct ResponseInfo {
//        public string message;
//    }
//}
//
//////////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////////
//
//// タイププロパティパーサー
//// メッセージ先読み用
//public partial class WebSocketConnector {
//    public class TypePropertyParser {
//        //---------------------------------------------------------------------- 定義
//        class Container { public int type; }
//
//        //---------------------------------------------------------------------- 操作
//        public static bool TryParse(string message, out int type) {
//            var hasType = false;
//            type = -1;
//            var container = ObjectPool<Container>.GetObject();
//            container.type = -1;
//            try {
//                JsonUtility.FromJsonOverwrite(message, container);
//                type    = container.type;
//                hasType = true;
//            } catch (Exception e) {
//                Debug.LogError(e.ToString());
//            }
//            container.type = -1;
//            ObjectPool<Container>.ReturnObject(container);
//            return hasType;
//        }
//
//        //---------------------------------------------------------------------- 操作 (おまけエイリアス)
//        // 型識別の実行
//        public static int IdentifyType(string message) {
//            var type = -1;
//            TryParse(message, out type);
//            return type;
//        }
//    }
//}
//
//////////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////////
//
//// リクエストプロパティパーサー
//// リクエスト先読み用
//public partial class WebSocketConnector {
//    public class RequestPropertyParser {
//        //---------------------------------------------------------------------- 定義
//        class Container { public int requestId; public string requester; }
//
//        //---------------------------------------------------------------------- 操作
//        public static bool TryParse(string message, out int requestId, out string requester) {
//            var hasRequest = false;
//            requestId  = 0;
//            requester  = default(string);
//            var container = ObjectPool<Container>.GetObject();
//            container.requestId = 0;
//            try {
//                JsonUtility.FromJsonOverwrite(message, container);
//                requestId  = container.requestId;
//                requester  = container.requester;
//                hasRequest = (requestId != 0 && requester != default(string));
//            } catch (Exception e) {
//                Debug.LogError(e.ToString());
//            }
//            container.requestId = 0;
//            container.requester = default(string);
//            ObjectPool<Container>.ReturnObject(container);
//            return hasRequest;
//        }
//    }
//}
