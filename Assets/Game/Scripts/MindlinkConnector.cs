using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

// マインドリンクコネクタ
public partial class MindlinkConnector : WebSocketConnector {
    //-------------------------------------------------------------------------- 変数
    public string url              = "ws://localhost:7766"; // 接続先URL
    public string connectKey       = "";                    // 接続キー
    public int    retryCount       = 10;                    // 接続リトライ回数
    public float  retryInterval    = 3.0f;                  // 接続リトライ間隔
    public bool   connectOnStart   = true;                  // 自動接続
    public bool   quitOnDisconnect = true;                  // 切断終了

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
        // イベントハンドラ設定
        SetConnectEventHandler(OnConnect);
        SetDisconnectEventHandler(OnDisconnect);

        // connectKey 調整
        // 環境変数からシークレットを取得して付与。
        var connectKeyValue = Environment.GetEnvironmentVariable("CONNECT_KEY");
        if (!string.IsNullOrEmpty(connectKeyValue)) {
            connectKey = connectKeyValue;
        }
        var connectKeyFileValue = Environment.GetEnvironmentVariable("CONNECT_KEY_FILE");
        if (!string.IsNullOrEmpty(connectKeyFileValue)) {
            try {
                connectKey = string.Join("", File.ReadAllText(connectKeyFileValue));
            } catch (Exception e) {
                Debug.LogError(e.ToString());
            }
        }

        // 接続開始
        if (connectOnStart) {
            StartConnect();
        }
    }

    //-------------------------------------------------------------------------- 接続ハンドリング
    public void StartConnect() {
        var uriBuilder = new UriBuilder(url);
        if (!string.IsNullOrEmpty(connectKey)) {
            uriBuilder.Query += ((string.IsNullOrEmpty(uriBuilder.Query))? "?" : "&") + "ck=" + connectKey;
        }
        Connect(uriBuilder.ToString());
    }

    void OnConnect() {
        currentRetryCount = 0; // NOTE 接続成功で接続リトライ回数リセット
    }

    void OnDisconnect(string error) {
        if (error != null) {
            Debug.LogError(error);
            if (currentRetryCount++ < retryCount) {
                Invoke("StartConnect", retryInterval); // NOTE 再接続
                return;
            }
        }
        if (quitOnDisconnect) {
            Application.Quit(); // NOTE 切断時 (再接続失敗時含む) はアプリ終了
        }
    }
}
