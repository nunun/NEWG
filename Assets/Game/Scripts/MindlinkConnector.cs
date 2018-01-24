using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

// マインドリンクコネクタ
public partial class MindlinkConnector : WebSocketConnector {
    //-------------------------------------------------------------------------- 変数
    public string url           = "ws://localhost:7766"; // 接続先URL
    public string connectKey    = "";                    // 接続キー
    public int    retryCount    = 10;                    // 接続リトライ回数
    public float  retryInterval = 3.0f;                  // 接続リトライ間隔

    // 現在のリトライ回数
    int currentRetryCount = 0;

    // インスタンス
    static MindlinkConnector instance = null;

    // インスタンスの取得
    public static MindlinkConnector Instance { get { return instance; }}

    //-------------------------------------------------------------------------- 実装 (MonoBehaviour)
    void Awake() {
        if (instance != null) {
            GameObject.Destroy(this.gameObject);
            return;
        }
        instance = this;
    }

    void OnDestroy() {
        if (instance == this) {
            instance = null;
        }
        Disconnect();
    }

    void Start() {
        OnConnect(OnMindlinkConnectorConnect);
        OnDisconnect(OnMindlinkConnectorDisconnect);
        // TODO
        // CONNECT_KEY または CONNECT_KEY_FILE から接続キーを読み込んで
        // connectKey にセット
        StartConnect(); // NOTE 自動接続
    }

    //-------------------------------------------------------------------------- 接続ハンドリング
    public void StartConnect() {
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
        if (error != null) {
            Debug.LogError(error);
            if (currentRetryCount++ < retryCount) {
                Invoke("StartConnect", retryInterval); // NOTE しばらく待って再接続
                return;
            }
        }
        Application.Quit(); // NOTE 接続終了時 (再接続失敗時も含む) はアプリ毎落とす
    }
}
