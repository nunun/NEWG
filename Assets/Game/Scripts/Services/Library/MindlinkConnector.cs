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

    // リモート送信データ
    [Serializable]
    public class SendDataToRemote<TSend> {
        public int          type      = 0;
        public TSend        data      = default(TSend);
        public int          requestId = 0;
        public bool         response  = false;
        public string       error     = null;
        public RemoteHeader remote    = default(RemoteHeader);
    }

    // リモート受信データ
    [Serializable]
    public class RecvDataFromRemote<TRecv> {
        public TRecv        data   = default(TRecv);
        public RemoteHeader remote = default(RemoteHeader);
    }

    // リモートヘッダ
    [Serializable]
    public struct RemoteHeader {
        public string from;
        public string to;
        public int    type;
        public int    requestId;
        public bool   response;
    }

    //-------------------------------------------------------------------------- 変数
    Dictionary<int,Action<string>> dataFromRemoteEventListener = new Dictionary<int,Action<string>>(); // リモートデータ受信時イベントリスナ (型別)

    // インスタンスコンテナ
    static InstanceContainer<MindlinkConnector> instanceContainer = new InstanceContainer<MindlinkConnector>();

    //-------------------------------------------------------------------------- 実装 (WebSocketConnector)
    // 初期化
    protected override void Init() {
        base.Init();

        // データイベントリスナに接続
        SetDataThruEventListener((int)DataType.M, (message) => {
            var requestId = 0;
            var response  = false;
            var error     = default(string);
            if (RemoteHeaderRequestPropertyParser.TryParse(message, out requestId, out response, out error)) {
                requestContext.SetResponse(requestId, error, message);
                return;
            }
            int type = -1;
            if (RemoteHeaderTypePropertyParser.TryParse(message, out type)) {
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

    //-------------------------------------------------------------------------- サービスリクエスト
    // ステータス送信
    public int SendStatus<TStatus>(TStatus status, Action<string> callback) {
        return Send<TStatus,TStatus>((int)MindlinkConnector.DataType.S, status, (error,recvStatus) => {
            callback(error);
        });
    }

    //-------------------------------------------------------------------------- 送信とキャンセル
    // リモートに送信 (リクエスト)
    public int SendToRemote<TSend,TRecv>(string to, int type, TSend data, Action<string,TRecv> callback, float timeout = 10.0f) {
        var requestId = requestContext.NextRequestId();
        var requester = UUID;

        var request = RequestToRemote<TRecv>.RentFromPool(requestId, callback, timeout);
        requestContext.SetRequest(request);

        // 送信
        var error = TransmitToRemote<TSend>(to, type, data, requestId);
        if (error != null) {
            requestContext.CancelRequest(requestId, error);
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
                var recvData = JsonUtility.FromJson<RecvData<TRecv>>(message);
                eventListener(recvData.data);
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
                var recvData  = JsonUtility.FromJson<RecvDataFromRemote<TRecv>>(message);
                var res       = new ResponseToRemote<TResponse>();
                res.connector = this;
                res.to        = recvData.remote.from;
                res.type      = recvData.remote.type;
                res.requestId = recvData.remote.requestId;
                eventListener(recvData.data, res);
            } catch (Exception e) {
                Debug.LogError(e.ToString());
                return;
            }
        };
    }

    protected void SetDataMessageFromRemoteEventListener(int type, Action<string> eventListener) { // NOTE メッセージそのままを受け取るデータイベントリスナ (継承用)
        if (eventListener == null) {
            dataFromRemoteEventListener.Remove(type);
            return;
        }
        dataFromRemoteEventListener[type] = eventListener;
    }

    //-------------------------------------------------------------------------- インスタンス取得
    // コネクタの取得
    new public static MindlinkConnector GetConnector(string name = null) {
        return instanceContainer.Find(name);
    }

    //-------------------------------------------------------------------------- 実装 (MonoBehaviour)
    void Awake() {
        instanceContainer.Add(connectorName, this);
        Init();
    }

    void OnDestroy() {
        Clear();
        instanceContainer.Remove(this);
    }
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// リモートへのリクエスト (型別)
// リクエスト中の情報を保持するためのクラス。
public partial class WebSocketConnector {
    public class RequestToRemote<TRecv> : Request<TRecv> {
        //---------------------------------------------------------------------- 確保
        // 確保
        new public static RequestToRemote<TRecv> RentFromPool(int requestId, Action<string,TRecv> callback, float timeout = 10.0f) {
            var req = ObjectPool<RequestToRemote<TRecv>>.RentObject();
            req.requestId            = requestId;
            req.timeoutRemainingTime = timeout;
            req.setResponse          = Request<TRecv>.SetResponse;
            req.returnToPool         = Request<TRecv>.ReturnToPool;
            req.callback             = callback;
            return req;
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
        public MindlinkConnector connector;
        public string            to;
        public int               type;
        public int               requestId;

        //---------------------------------------------------------------------- 操作
        // 送信元に向けて送り返す
        public void Send(TResponse data) {
            connector.TransmitToRemote<TResponse>(to, type, data, requestId, true, null);
        }

        // 送信元に向けてエラーを送り返す
        public void Error(string error) {
            connector.TransmitToRemote<TResponse>(to, type, default(TResponse), requestId, true, error);
        }
    }
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// リモートデータタイププロパティパーサー
// リクエスト先読み用
public partial class WebSocketConnector {
    public class RemoteHeaderTypePropertyParser {
        //---------------------------------------------------------------------- 定義
        [Serializable]
        class Container { public RemoteHeaderContainer remote = new RemoteHeaderContainer(); }

        [Serializable]
        class RemoteHeaderContainer { public int type; }

        //---------------------------------------------------------------------- 操作
        public static bool TryParse(string message, out int type) {
            var hasType = false;
            type = -1;
            var container = ObjectPool<Container>.RentObject();
            container.remote.type = -1;
            try {
                JsonUtility.FromJsonOverwrite(message, container);
                type = container.remote.type;
                hasType = (type != -1);
            } catch (Exception e) {
                Debug.LogError(e.ToString());
            }
            container.remote.type = -1;
            ObjectPool<Container>.ReturnObject(container);
            return hasType;
        }
    }
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// リモートヘッダーリクエストプロパティパーサー
// リクエスト先読み用
public partial class WebSocketConnector {
    public class RemoteHeaderRequestPropertyParser {
        //---------------------------------------------------------------------- 定義
        [Serializable]
        class Container { public RemoteHeaderContainer remote = new RemoteHeaderContainer(); }

        [Serializable]
        class RemoteHeaderContainer { public int requestId; public bool response; public string error; }

        //---------------------------------------------------------------------- 操作
        public static bool TryParse(string message, out int requestId, out bool response, out string error) {
            var hasRequest = false;
            requestId = 0;
            response  = false;
            error     = null;
            var container = ObjectPool<Container>.RentObject();
            container.remote.requestId = 0;
            container.remote.response  = false;
            container.remote.error     = null;
            try {
                JsonUtility.FromJsonOverwrite(message, container);
                requestId  = container.remote.requestId;
                response   = container.remote.response;
                error      = container.remote.error;
                hasRequest = (requestId != 0 && response);
            } catch (Exception e) {
                Debug.LogError(e.ToString());
            }
            container.remote.requestId = 0;
            container.remote.response  = false;
            container.remote.error     = null;
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
public partial class MindlinkConnector {
    //-------------------------------------------------------------------------- 操作
    // リモートに送出
    protected string TransmitToRemote<TSend>(string to, int type, TSend data, int requestId = -1, bool response = false, string error = null) {
        try {
            // 接続チェック
            if (state != State.Connected) {
                throw new Exception("WebSocket is not connected.");
            }

            // 文字列変換
            var sendData = ObjectPool<SendDataToRemote<TSend>>.RentObject();
            sendData.type             = (int)DataType.M;
            sendData.data             = data;
            sendData.requestId        = 0;
            sendData.response         = false;
            sendData.error            = null;
            sendData.remote.from      = null;
            sendData.remote.to        = to;
            sendData.remote.type      = type;
            sendData.remote.requestId = requestId;
            sendData.remote.response  = response;
            var message = JsonUtility.ToJson(sendData);

            // プールに戻す
            sendData.type             = 0;
            sendData.data             = default(TSend);
            sendData.requestId        = 0;
            sendData.response         = false;
            sendData.error            = null;
            sendData.remote.from      = null;
            sendData.remote.to        = null;
            sendData.remote.type      = 0;
            sendData.remote.requestId = 0;
            sendData.remote.response  = false;
            ObjectPool<SendDataToRemote<TSend>>.ReturnObject(sendData);

            // 送信
            ws.SendString(encrypter.Encrypt(message));

        } catch (Exception e) {
            return e.ToString();
        }
        return null;
    }
}
