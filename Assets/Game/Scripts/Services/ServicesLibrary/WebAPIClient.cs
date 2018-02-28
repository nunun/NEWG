using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// WebAPI クライアント
public partial class WebAPIClient : MonoBehaviour {
    //-------------------------------------------------------------------------- 変数
    public enum HttpMethod { Get, Post };

    //-------------------------------------------------------------------------- 変数
    public string   url             = false; // URL
    public string[] queries         = null;  // デフォルトのクエリ
    public string[] forms           = null;  // デフォルトのフォーム
    public string[] headers         = null;  // デフォルトのヘッダ
    public bool     isDefaultClient = false; // デフォルトクライアントかどうか

    // クライアント一覧とデフォルトクライアント
    static List<WebAPIClient> clients       = new List<string,WebAPIClient>();
    static WebAPIClient       defaultClient = null;

    //-------------------------------------------------------------------------- 実装ポイント
    // 初期化
    protected virtual void Init() {
        Clear();
    }

    // クリア
    protected virtual void Clear() {
        // NOTE
        // 今のところ処理なし
    }

    //-------------------------------------------------------------------------- リクエスト関連
    // Get メソッド
    public void Get<TRes>(string url, Callback<string,TRes> callback, string[] queries = null, string[] forms = null, string[] headers = null) {
        Request<object,TRes>(HttpMethod.Get, url, default(object), callback, queries, forms, headers);
    }

    // Post メソッド
    public void Post<TReq,TRes>(string url, TReq data, Callback<string,TRes> callback, string[] queries = null, string[] forms = null, string[] headers = null) {
        Request<TReq,TRes>(HttpMethod.Post, url, data, callback, queries, forms, headers);
    }

    // リクエスト
    public void Request<TReq,TRes>(HttpMethod method, string url, TReq data, Callback<string,TRes> callback, string[] queries = null, string[] forms = null, string[] headers = null) {
        // TODO
        // this.url + url にリクエスト
        // UnityWebRequest を生成して、Update() で isFinished まで待つ。
    }

    //-------------------------------------------------------------------------- インスタンス取得
    public static GetClient(string name = null) {
        if (name == null) {
            return defaultClient;
        }
        for (int i = 0; i < clients.Count; i++) {
            var client = clients[i];
            if (client.name == name) {
                return client;
            }
        }
        return null;
    }

    //-------------------------------------------------------------------------- 実装 (MonoBehaviour)
    void Awake() {
        // インスタンス設定
        if (clients.IndexOf(this) < 0) {
            clients.Add(this);
        }
        if (this.isDefaultClient || defaultClient == null) {
            Debug.Assert(((defaultClient != null) && defaultClient.isDefaultClient), "デフォルトクライアントが 2 つある？");
            defaultClient = this;
        }

        // 初期化
        Init();
    }

    void OnDestroy() {
        // クリア
        Clear();

        // インスタンス解除
        clients.Remove(this);
        if (defaultClient == this) {
            defaultClient =null;
        }
    }

    void Update() {
        // TODO
        // UnityWebRequest を待つ
    }
}
