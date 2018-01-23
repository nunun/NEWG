using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

// マインドリンクコネクタ
// TODO CONNECT_KEY_FILE 環境変数の値のファイルから接続キーを読み込む。
public partial class MindlinkConnector : WebSocketConnector {
    //-------------------------------------------------------------------------- 変数
    public string url           = "ws://localhost:7766"; // 接続先URL
    public string connectKey    = "";                    // 接続キー
    public int    retryCount    = 10;                    // 接続リトライ回数
    public float  retryInterval = 3.0f;                  // 接続リトライ間隔

    int currentRetryCount;

    //-------------------------------------------------------------------------- 実装 (MonoBehaviour)
    void Start() {
        currentRetryCount = 0;
        OnConnect(OnMindlinkConnectorConnect);
        OnDisconnect(OnMindlinkConnectorDisconnect);
        StartConnect();
    }

    //-------------------------------------------------------------------------- イベントハンドラその他
    void StartConnect() {
        var uriBuilder = new UriBuilder(url);
        if (!string.IsNullOrEmpty(connectKey)) {
            uriBuilder.Query += ((string.IsNullOrEmpty(uriBuilder.Query))? "?" : "&") + "ck=" + connectKey;
        }
        Connect(uriBuilder.ToString());
    }

    void OnMindlinkConnectorConnect() {
        currentRetryCount = 0; // NOTE 接続成功で接続リトライ回数リセット
    }

    void OnMindlinkConnectorDisconnect(string error) {
        Debug.LogError(error);
        if (currentRetryCount++ < retryCount) {
            Invoke("StartConnect", 3.0f);
            return;
        }
        Application.Quit();
    }
}
