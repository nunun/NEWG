using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// マインドリンクコネクタ
public partial class MindlinkConnector : WebSocketConnector {
    //-------------------------------------------------------------------------- 定義
    // マインドリンクデータタイプ
    public enum DataType {
        S = 1, // サービス更新
        Q = 2, // サービスクエリ
        M = 3, // メッセージ転送
    }

    //-------------------------------------------------------------------------- 変数
    // イベントリスナ
    Dictionary<int,Action<string>> requestFromRemoteEventListener = new Dictionary<int,Action<string>>(); // リモートからのリクエスト受信時イベントリスナ (コマンド別)
    Dictionary<int,Action<string>> dataFromRemoteEventListener    = new Dictionary<int,Action<string>>(); // リモートからのデータ受信時イベントリスナ     (コマンド別)

    //-------------------------------------------------------------------------- 送信とリクエスト
    // リモートに送信
    public bool SendToRemote<TSend>(string to, int cmd, TSend data) {
        try {
            // 接続チェック
            if (state != State.Connected) {
                throw new Exception("WebSocket is not connected.");
            }

            // 文字列変換
            var message = JsonUtility.ToJson(data);

            // NOTE
            // メッセージに "type", "to", "cmd" プロパティをねじ込む。
            var sb = ObjectPool<StringBuilder>.GetObject();
            sb.Append(message);
            sb.Remove(message.Length - 1, 1); // "}" 消し
            sb.AppendFormat(",\"type\":{0},\"to\":\"{1}\",\"cmd\":{2}}", (int)DataType.M, to, cmd);
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

    // リモートにリクエスト送信
    public bool RequestToRemote<TSend,TRecv>(string to, int cmd, TSend data, Action<string,TRecv> callback, int timeout = 0) {
        var requestInfo = default(RequestInfo<TRecv>);
        try {
            // 接続チェック
            if (state != State.Connected) {
                throw new Exception("WebSocket is not connected.");
            }

            // 文字列変換
            var message = JsonUtility.ToJson(data);

            // リクエスト番号、送信 UUID を設定
            var requestId = NextRequestId;
            var requester = UUID;

            // NOTE
            // メッセージに "type", "to", "cmd", "requestId", "requester" プロパティをねじ込む。
            var sb = ObjectPool<StringBuilder>.GetObject();
            sb.Append(message);
            sb.Remove(message.Length - 1, 1); // "}" 消し
            sb.AppendFormat(",\"type\":{0},\"to\":\"{1}\",\"cmd\":{2},\"requestId\":{1},\"requester\":\"{2}\"}", (int)DataType.M, to, cmd, requestId, requester);
            message = sb.ToString();
            ObjectPool<StringBuilder>.ReturnObject(sb);

            // リクエストを作成して追加
            // レスポンスを待つ。
            requestInfo = RequestInfo<TRecv>.GetRequest(callback);
            requestInfo.requestId            = requestId;
            requestInfo.requester            = requester;
            requestInfo.timeoutRemainingTime = (float)((timeout > 0)? timeout : REQUEST_DEFAULT_TIMEOUT) / 1000.0f;
            requestList.Add(requestInfo);

            // 送信
            ws.SendString(message);

        } catch (Exception e) {
            Debug.LogError(e.ToString());
            if (requestInfo != null) {
                requestList.Remove(requestInfo);
                requestInfo.ReturnToPool();
            }
            callback(e.ToString(), default(TRecv));
            return false;
        }
        return true;
    }

    // リモートにレスポンス送信
    public bool ResponseToRemote<TSend>(ResponseInfo responseInfo, TSend data) {
        try {
            // 接続チェック
            if (state != State.Connected) {
                throw new Exception("WebSocket is not connected.");
            }

            // 文字列変換 (レスポンス)
            var message = JsonUtility.ToJson(data);

            // リクエスト番号、送信 UUID を設定
            var from       = default(string);
            var requestId  = 0;
            var requester  = default(string);
            FromPropertyParser.TryParse(responseInfo.message, out from);
            RequestPropertyParser.TryParse(responseInfo.message, out requestId, out requester);

            // メッセージに "requestId", "requester" プロパティをねじ込む。
            var sb = ObjectPool<StringBuilder>.GetObject();
            sb.Append(message);
            sb.Remove(message.Length - 1, 1); // "}" 消し
            sb.AppendFormat(",\"type\":{0},\"to\":{1},\"requestId\":{2},\"requester\":\"{3}\"}", (int)DataType.M, from, requestId, requester);
            message = sb.ToString();
            ObjectPool<StringBuilder>.ReturnObject(sb);

            // 送信
            ws.SendString(message);

        } catch (Exception e) {
            Debug.LogError(e.ToString());
            return false;
        }
        return true;
    }

    //-------------------------------------------------------------------------- イベントリスナ設定
    public void SetRequestFromRemoteEventListener<TRecv>(int cmd, Action<TRecv,ResponseInfo> eventListener) {
        if (eventListener == null) {
            requestFromRemoteEventListener.Remove(cmd);
            return;
        }
        if (typeof(TRecv) == typeof(string)) {
            requestFromRemoteEventListener[cmd] = (message) => {
                var data         = (TRecv)(object)message;
                var responseInfo = new ResponseInfo() { message = message };
                eventListener(data, responseInfo);
            };
            return;
        }
        requestFromRemoteEventListener[cmd] = (string message) => {
            try {
                var data         = JsonUtility.FromJson<TRecv>(message);
                var responseInfo = new ResponseInfo() { message = message };
                eventListener(data, responseInfo);
            } catch (Exception e) {
                Debug.LogError(e.ToString());
                return;
            }
        };
    }

    public void SetDataFromRemoteEventListener<TRecv>(int cmd, Action<TRecv> eventListener) {
        if (eventListener == null) {
            dataFromRemoteEventListener.Remove(cmd);
            return;
        }
        if (typeof(TRecv) == typeof(string)) {
            dataFromRemoteEventListener[cmd] = (message) => {
                var data = (TRecv)(object)message;;
                eventListener(data);
            };
            return;
        }
        dataFromRemoteEventListener[cmd] = (string message) => {
            try {
                var data = JsonUtility.FromJson<TRecv>(message);
                eventListener(data);
            } catch (Exception e) {
                Debug.LogError(e.ToString());
                return;
            }
        };
    }

    //-------------------------------------------------------------------------- 実装 (MonoBehaviour)
    void Awake() {
        // リモートメッセージングを接続
        SetRequestEventListener<string>((int)DataType.M, (message, responseInfo) => {
            var cmd = -1;
            if (   !CmdPropertyParser.TryParse(message, out cmd)
                || !requestFromRemoteEventListener.ContainsKey(cmd)) {
                return; // NOTE サイレントディスカード
            }
            requestFromRemoteEventListener[cmd](message);
        });
        SetDataEventListener<string>((int)DataType.M, (message) => {
            var cmd    = -1;
            if (   !CmdPropertyParser.TryParse(message, out cmd)
                || !requestFromRemoteEventListener.ContainsKey(cmd)) {
                return; // NOTE サイレントディスカード
            }
            dataFromRemoteEventListener[cmd](message);
        });
    }
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// フロムプロパティパーサー
// コマンド先読み用
public partial class MindlinkConnector {
    class FromPropertyParser {
        //---------------------------------------------------------------------- 定義
        // リクエストプロパティコンテナ
        public class Container { public string from; }

        //---------------------------------------------------------------------- 操作
        public static bool TryParse(string message, out string from) {
            var hasFrom = false;
            from = null;
            var container = ObjectPool<Container>.GetObject();
            container.from = null;
            try {
                JsonUtility.FromJsonOverwrite(message, container);
                from    = container.from;
                hasFrom = true;
            } catch (Exception e) {
                Debug.LogError(e.ToString());
            }
            container.from = null;
            ObjectPool<Container>.ReturnObject(container);
            return hasFrom;
        }
    }
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// コマンドプロパティパーサー
// コマンド先読み用
public partial class MindlinkConnector {
    class CmdPropertyParser {
        //---------------------------------------------------------------------- 定義
        // リクエストプロパティコンテナ
        public class Container { public int cmd; }

        //---------------------------------------------------------------------- 操作
        public static bool TryParse(string message, out int cmd) {
            var hasCmd = false;
            cmd = -1;
            var container = ObjectPool<Container>.GetObject();
            container.cmd = -1;
            try {
                JsonUtility.FromJsonOverwrite(message, container);
                cmd    = container.cmd;
                hasCmd = true;
            } catch (Exception e) {
                Debug.LogError(e.ToString());
            }
            container.cmd = -1;
            ObjectPool<Container>.ReturnObject(container);
            return hasCmd;
        }
    }
}

// connectKey 調整
// 環境変数からシークレットを取得して付与。
//var connectKeyValue = Environment.GetEnvironmentVariable("CONNECT_KEY");
//if (!string.IsNullOrEmpty(connectKeyValue)) {
//    connectKey = connectKeyValue;
//}
//var connectKeyFileValue = Environment.GetEnvironmentVariable("CONNECT_KEY_FILE");
//if (!string.IsNullOrEmpty(connectKeyFileValue)) {
//    try {
//        connectKey = File.ReadAllText(connectKeyFileValue).Trim();
//    } catch (Exception e) {
//        Debug.LogError(e.ToString());
//    }
//}
