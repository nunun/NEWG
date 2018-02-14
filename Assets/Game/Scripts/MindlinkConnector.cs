using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

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
    Dictionary<int,Action<string>> dispatchRequestEventListener = new Dictionary<int,Action<string>>(); // 転送リクエスト受信時イベントリスナ (コマンド別)
    Dictionary<int,Action<string>> dispatchDataEventListener    = new Dictionary<int,Action<string>>(); // 転送データ受信時イベントリスナ     (コマンド別)

    // 上位イベントリスナ初期化フラグ
    bool isRequestEventListenerInitialized = false;
    bool isDataEventListenerInitialized    = false;

    //-------------------------------------------------------------------------- イベントリスナ設定
    public void SetDispatchRequestEventListener<TRecv>(int cmd, Action<TRecv> eventListener) {
        if (!isRequestEventListenerInitialized) { // 上位イベントリスナ初期化
            isRequestEventListenerInitialized = true;
            SetRequestEventListener((int)DataType.M, (message) => {
                var cmd    = -1;
                var hasCmd = false;
                CommandPropertyParser.Parse(message, out cmd, out hasCmd);
                if (cmd < 0 || !dispatchRequestEventListener.ContainsKey(cmd)) {
                    return; // NOTE サイレントディスカード
                }
                dispatchRequestEventListener[cmd](message);
            });
        }
        if (eventListener == null) {
            dispatchRequestEventListener.Remove(cmd);
            return;
        }
        dispatchRequestEventListener[cmd] = (string message) => {
            try {
                var data = JsonUtility.FromJson<TRecv>(message);
                eventListener(data);
            } catch (Exception e) {
                Debug.LogError(e.ToString());
                return;
            }
        };
    }

    public void SetDispatchDataEventListener<TRecv>(int cmd, Action<TRecv> eventListener) {
        if (isDataEventListenerInitialized) { // 上位イベントリスナ初期化
            isDataEventListenerInitialized = true;
            SetDataEventListener((int)DataType.M, (message) => {
                var cmd    = -1;
                var hasCmd = false;
                CommandPropertyParser.Parse(message, out cmd, out hasCmd);
                if (cmd < 0 || !dispatchDataEventListener.ContainsKey(cmd)) {
                    return; // NOTE サイレントディスカード
                }
                dispatchDataEventListener[cmd](message);
            });
        }
        if (eventListener == null) {
            dispatchDataEventListener.Remove(cmd);
            return;
        }
        dispatchDataEventListener[cmd] = (string message) => {
            try {
                var data = JsonUtility.FromJson<TRecv>(message);
                eventListener(data);
            } catch (Exception e) {
                Debug.LogError(e.ToString());
                return;
            }
        };
    }
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// コマンドプロパティパーサー
// コマンド先読み用
public partial class WebSocketConnector {
    class CommandPropertyParser {
        //---------------------------------------------------------------------- 定義
        // リクエストプロパティコンテナ
        class Container { public int cmd; }

        //---------------------------------------------------------------------- 操作
        public static void Parse(string message, out int cmd, out bool hasCmd) {
            cmd    = -1;
            hasCmd = false;
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
