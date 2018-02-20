using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// マインドリンクコネクタ
// Mindlink と実際に通信を行うコンポーネント。
public partial class MindlinkConnector : WebSocketConnector {
    //-------------------------------------------------------------------------- 定義
    // マインドリンクデータタイプ
    public enum DataType {
        S = 1, // サービス更新
        Q = 2, // サービスクエリ
        M = 3, // メッセージ転送
    }

    // ペイロード付きデータ
    public struct DataWithPayload<TPayload> {
        TPayload payload;
    }

    //-------------------------------------------------------------------------- 変数
    Dictionary<int,Action<string>> dataFromRemoteEventListener = new Dictionary<int,Action<string>>(); // リモートデータ受信時イベントリスナ (型別)

    //-------------------------------------------------------------------------- 実装 (WebSocketConnector)
    // 初期化
    protected override void Init() {
        base.Init();

        // データイベントリスナに接続
        SetDataMessageEventListener((int)DataType.M, (message) => {
            if (PayloadTypePropertyParser.TryParse(message, out type)) {
                if (dataFromRemoteEventListener.ContainsKey(type)) {
                    dataFromRemoteEventListener[type](message);
                }
            }
        });
    }

    // クリア
    protected override void Clear() {
        base.Clear();
    }

    //-------------------------------------------------------------------------- 送信とキャンセル
    // リモートに送信 (リクエスト)
    public int SendToRemote<TSend,TRecv>(int type, TSend data, Action<string,TRecv> callback, int timeout = 0) {
        var requestId = requestContext.NextRequestId();
        var requester = UUID;

        var request = RequestToRemote<TRecv>.GetRequest(requestId, callback, timeout);
        requestContext.SetRequest(request);

        // 送信
        var error = Transmit<TSend>(type, data, requestId, requester);
        if (error != null) {
            requestContext.CancelRequest(requestId);
            return 0;
        }
        return requestId;
    }

    //-------------------------------------------------------------------------- イベントリスナ
    public void SetDataFromRemoteEventListener<TRecv>(int type, Action<TRecv> eventListener) { // NOTE レスポンスしないデータイベントリスナ
        if (eventListener == null) {
            dataFromRemoteEventListener.Remove(type);
            return;
        }
        dataFromRemoteEventListener[type] = (message) => {
            try {
                var data = JsonUtility.FromJson<DataWithPayload<TRecv>>(message);
                eventListener(data.payload);
            } catch (Exception e) {
                Debug.LogError(e.ToString());
                return;
            }
        };
    }

    public void SetDataFromRemoteEventListener<TRecv,TResponse>(int type, Action<TRecv,ResponseToRemote<TResponse>> eventListener) { // NOTE レスポンスするデータイベントリスナ
        if (eventListener == null) {
            dataFromRemoteEventListener.Remove(type);
            return;
        }
        dataFromRemoteEventListener[type] = (message) => {
            try {
                var data = JsonUtility.FromJson<DataWithPayload<TRecv>>(message);
                var res  = new ResponseToRemote<TResponse>();
                res.connector = this;
                FromPropertyParser.TryParse(message, out res.to);
                res.type = type;
                RequestPropertyParser.TryParse(message, out res.requestId, out res.requester);
                eventListener(data.payload, res);
            } catch (Exception e) {
                Debug.LogError(e.ToString());
                return;
            }
        };
    }

    protected void SetDataMessageFromRemoteEventListener(int type, Actoin<string> eventListener) { // NOTE メッセージそのままを受け取るデータイベントリスナ (継承用)
        if (eventListener == null) {
            dataFromRemoteEventListener.Remove(type);
            return;
        }
        dataFromRemoteEventListener[type] = eventListener;
    }
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// リモートへのリクエスト (型別)
// リクエスト中の情報を保持するためのクラス。
public partial class WebSocketConnector {
    public class RequestToRemote<TRecv> : Request {
        //---------------------------------------------------------------------- 変数
        // 受信コールバック
        protected Action<string,TRecv> callback = null;

        //---------------------------------------------------------------------- 確保
        // 確保
        public static RequestToRemote<TRecv> GetRequest(int requestId, Action<string,TRecv> callback, float timeout = 10.0f) {
            var req = ObjectPool<RequestToRemote<TRecv>>.GetObject();
            req.requestId            = requestId;
            req.timeoutRemainingTime = timeout;
            req.setResponse          = RequestToRemote<TRecv>.SetResponse;
            req.returnToPool         = RequestToRemote<TRecv>.ReturnToPool;
            req.callback             = callback;
            return req;
        }

        //---------------------------------------------------------------------- 内部コールバック用
        // レスポンスデータを設定
        static void SetResponse(Request request, string err, string message) {
            var req = request as RequestToRemote<TRecv>;
            Debug.Assert(req != null);
            Debug.Assert(req.callback != null);
            try {
                if (err != null) {
                    throw new Exception(err);
                }
                var data = JsonUtility.FromJson<DataWithPayload<TRecv>>(message);
                req.callback(null, data.payload);
            } catch (Exception e) {
                req.callback(e.ToString(), default(TRecv));
            }
        }

        // 解放
        static void ReturnToPool(Request request) {
            var req = request as RequestToRemote<TRecv>;
            Debug.Assert(req != null);
            req.requestId            = 0;
            req.timeoutRemainingTime = 0.0f;
            req.setResponse          = null;
            req.returnToPool         = null;
            req.callback             = null;
            ObjectPool<RequestToRemote<TRecv>>.ReturnObject(req);
        }
    }
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// リモートへのレスポンス (型別)
// Send のコールバックで res.Send(data) を実現するためのクラス。
public partial class MindlinkConnector {
    public struct ResponseToRemote<TResponse> {
        //---------------------------------------------------------------------- 変数
        WebSocketConnector connector;
        string             to;
        int                type;
        int                requestId;
        string             requester;

        //---------------------------------------------------------------------- 操作
        // 送信元に向けて送り返す
        public void Send(TResponse data) {
            connector.TransmitToRemote<TResponse>(to, type, data, requestId, requester);
        }
    }
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// 送信元プロパティパーサー
// リクエスト先読み用
public partial class WebSocketConnector {
    public class FromPropertyParser {
        //---------------------------------------------------------------------- 定義
        class Container { public string from; }

        //---------------------------------------------------------------------- 操作
        public static bool TryParse(string message, out string from) {
            var hasFrom = false;
            from = default(string);
            var container = ObjectPool<Container>.GetObject();
            container.from = null;
            try {
                JsonUtility.FromJsonOverwrite(message, container);
                from = container.from;
                hasFrom = (from != default(string));
            } catch (Exception e) {
                Debug.LogError(e.ToString());
            }
            container.from = default(string);
            ObjectPool<Container>.ReturnObject(container);
            return hasFrom;
        }
    }
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// ペイロードタイププロパティパーサー
// リクエスト先読み用
public partial class WebSocketConnector {
    public class PayloadTypePropertyParser {
        //---------------------------------------------------------------------- 定義
        class Container { public PayloadContainer payload = new PayloadContainer(); }

        class PayloadContainer { public int type; }

        //---------------------------------------------------------------------- 操作
        public static bool TryParse(string message, out string type) {
            var hasType = false;
            type = -1;
            var container = ObjectPool<Container>.GetObject();
            container.payload.type = -1;
            try {
                JsonUtility.FromJsonOverwrite(message, container);
                type = container.payload.type;
                hasType = (type != -1);
            } catch (Exception e) {
                Debug.LogError(e.ToString());
            }
            container.payload.type = -1;
            ObjectPool<Container>.ReturnObject(container);
            return hasType;
        }
    }
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// 送出
// 送出ロジックは繰り返し使うのでユーティリティとして外に出しておく。
public partial class MindlinkConnector {
    //-------------------------------------------------------------------------- 操作
    // リモートに送出
    protected string TransmitToRemote<TSend>(string to, int type, TSend data, int requestId = -1, string requester = null) {
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
            sb.AppendFormat(",\"type\":{1}}", type);
            message = sb.ToString();
            ObjectPool<StringBuilder>.ReturnObject(sb);

            // NOTE
            // ラッパーメッセージに "type", "to", "payload", "requestId", "requester" プロパティをねじ込む。
            var ssb = ObjectPool<StringBuilder>.GetObject();
            if (requestId >= 0 && requester != null) {
                ssb.AppendFormat("{\"type\":{0},\"to\":{1},\"payload\":{2},\"requestId\":{3},\"requester\":\"{4}\"}", (int)DataType.M, to, message, requestId, requester);
            } else {
                ssb.AppendFormat("{\"type\":{0},\"to\":{1},\"payload\":{2}}", (int)DataType.M, to, message);
            }
            sendMessage = ssb.ToString();
            ObjectPool<StringBuilder>.ReturnObject(ssb);

            // 送信
            ws.SendString(sendMessage);

        } catch (Exception e) {
            return e.ToString();
        }
        return null;
    }
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
//// マインドリンクコネクタ
//public partial class MindlinkConnector : WebSocketConnector {
//    //-------------------------------------------------------------------------- 定義
//    // マインドリンクデータタイプ
//    public enum DataType {
//        S = 1, // サービス更新
//        Q = 2, // サービスクエリ
//        M = 3, // メッセージ転送
//    }
//
//    //-------------------------------------------------------------------------- 変数
//    // イベントリスナ
//    Dictionary<int,Action<string>> requestFromRemoteEventListener = new Dictionary<int,Action<string>>(); // リモートからのリクエスト受信時イベントリスナ (コマンド別)
//    Dictionary<int,Action<string>> dataFromRemoteEventListener    = new Dictionary<int,Action<string>>(); // リモートからのデータ受信時イベントリスナ     (コマンド別)
//
//    //-------------------------------------------------------------------------- 送信とリクエスト
//    // リモートに送信
//    public bool SendToRemote<TSend>(string to, int cmd, TSend data) {
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
//            // メッセージに "type", "to", "cmd" プロパティをねじ込む。
//            var sb = ObjectPool<StringBuilder>.GetObject();
//            sb.Append(message);
//            sb.Remove(message.Length - 1, 1); // "}" 消し
//            sb.AppendFormat(",\"type\":{0},\"to\":\"{1}\",\"cmd\":{2}}", (int)DataType.M, to, cmd);
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
//    // リモートにリクエスト送信
//    public bool RequestToRemote<TSend,TRecv>(string to, int cmd, TSend data, Action<string,TRecv> callback, int timeout = 0) {
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
//            // メッセージに "type", "to", "cmd", "requestId", "requester" プロパティをねじ込む。
//            var sb = ObjectPool<StringBuilder>.GetObject();
//            sb.Append(message);
//            sb.Remove(message.Length - 1, 1); // "}" 消し
//            sb.AppendFormat(",\"type\":{0},\"to\":\"{1}\",\"cmd\":{2},\"requestId\":{1},\"requester\":\"{2}\"}", (int)DataType.M, to, cmd, requestId, requester);
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
//    // リモートにレスポンス送信
//    public bool ResponseToRemote<TSend>(ResponseInfo responseInfo, TSend data) {
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
//            var from       = default(string);
//            var requestId  = 0;
//            var requester  = default(string);
//            FromPropertyParser.TryParse(responseInfo.message, out from);
//            RequestPropertyParser.TryParse(responseInfo.message, out requestId, out requester);
//
//            // メッセージに "requestId", "requester" プロパティをねじ込む。
//            var sb = ObjectPool<StringBuilder>.GetObject();
//            sb.Append(message);
//            sb.Remove(message.Length - 1, 1); // "}" 消し
//            sb.AppendFormat(",\"type\":{0},\"to\":{1},\"requestId\":{2},\"requester\":\"{3}\"}", (int)DataType.M, from, requestId, requester);
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
//    //-------------------------------------------------------------------------- イベントリスナ設定
//    public void SetRequestFromRemoteEventListener<TRecv>(int cmd, Action<TRecv,ResponseInfo> eventListener) {
//        if (eventListener == null) {
//            requestFromRemoteEventListener.Remove(cmd);
//            return;
//        }
//        if (typeof(TRecv) == typeof(string)) {
//            requestFromRemoteEventListener[cmd] = (message) => {
//                var data         = (TRecv)(object)message;
//                var responseInfo = new ResponseInfo() { message = message };
//                eventListener(data, responseInfo);
//            };
//            return;
//        }
//        requestFromRemoteEventListener[cmd] = (string message) => {
//            try {
//                var data         = JsonUtility.FromJson<TRecv>(message);
//                var responseInfo = new ResponseInfo() { message = message };
//                eventListener(data, responseInfo);
//            } catch (Exception e) {
//                Debug.LogError(e.ToString());
//                return;
//            }
//        };
//    }
//
//    public void SetDataFromRemoteEventListener<TRecv>(int cmd, Action<TRecv> eventListener) {
//        if (eventListener == null) {
//            dataFromRemoteEventListener.Remove(cmd);
//            return;
//        }
//        if (typeof(TRecv) == typeof(string)) {
//            dataFromRemoteEventListener[cmd] = (message) => {
//                var data = (TRecv)(object)message;;
//                eventListener(data);
//            };
//            return;
//        }
//        dataFromRemoteEventListener[cmd] = (string message) => {
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
//    //-------------------------------------------------------------------------- 実装 (MonoBehaviour)
//    void Awake() {
//        // リモートメッセージングを接続
//        SetRequestEventListener<string>((int)DataType.M, (message, responseInfo) => {
//            var cmd = -1;
//            if (   CmdPropertyParser.TryParse(message, out cmd)
//                && requestFromRemoteEventListener.ContainsKey(cmd)
//                && requestFromRemoteEventListener[cmd] != null) {
//                requestFromRemoteEventListener[cmd](message);
//            }
//        });
//        SetDataEventListener<string>((int)DataType.M, (message) => {
//            var cmd = -1;
//            if (   CmdPropertyParser.TryParse(message, out cmd)
//                && requestFromRemoteEventListener.ContainsKey(cmd)
//                && requestFromRemoteEventListener[cmd] != null) {
//                dataFromRemoteEventListener[cmd](message);
//            }
//        });
//    }
//}
//
//////////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////////
//
//// フロムプロパティパーサー
//// コマンド先読み用
//public partial class MindlinkConnector {
//    class FromPropertyParser {
//        //---------------------------------------------------------------------- 定義
//        // リクエストプロパティコンテナ
//        public class Container { public string from; }
//
//        //---------------------------------------------------------------------- 操作
//        public static bool TryParse(string message, out string from) {
//            var hasFrom = false;
//            from = null;
//            var container = ObjectPool<Container>.GetObject();
//            container.from = null;
//            try {
//                JsonUtility.FromJsonOverwrite(message, container);
//                from    = container.from;
//                hasFrom = true;
//            } catch (Exception e) {
//                Debug.LogError(e.ToString());
//            }
//            container.from = null;
//            ObjectPool<Container>.ReturnObject(container);
//            return hasFrom;
//        }
//    }
//}
//
//////////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////////
//
//// コマンドプロパティパーサー
//// コマンド先読み用
//public partial class MindlinkConnector {
//    class CmdPropertyParser {
//        //---------------------------------------------------------------------- 定義
//        // リクエストプロパティコンテナ
//        public class Container { public int cmd; }
//
//        //---------------------------------------------------------------------- 操作
//        public static bool TryParse(string message, out int cmd) {
//            var hasCmd = false;
//            cmd = -1;
//            var container = ObjectPool<Container>.GetObject();
//            container.cmd = -1;
//            try {
//                JsonUtility.FromJsonOverwrite(message, container);
//                cmd    = container.cmd;
//                hasCmd = true;
//            } catch (Exception e) {
//                Debug.LogError(e.ToString());
//            }
//            container.cmd = -1;
//            ObjectPool<Container>.ReturnObject(container);
//            return hasCmd;
//        }
//    }
//}
//
//// connectKey 調整
//// 環境変数からシークレットを取得して付与。
////var connectKeyValue = Environment.GetEnvironmentVariable("CONNECT_KEY");
////if (!string.IsNullOrEmpty(connectKeyValue)) {
////    connectKey = connectKeyValue;
////}
////var connectKeyFileValue = Environment.GetEnvironmentVariable("CONNECT_KEY_FILE");
////if (!string.IsNullOrEmpty(connectKeyFileValue)) {
////    try {
////        connectKey = File.ReadAllText(connectKeyFileValue).Trim();
////    } catch (Exception e) {
////        Debug.LogError(e.ToString());
////    }
////}
