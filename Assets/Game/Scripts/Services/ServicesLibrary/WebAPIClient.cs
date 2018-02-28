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
    public void Get<TRes>(string apiPath, Callback<string,TRes> callback, string[] queries = null, string[] forms = null, string[] headers = null) {
        Request<object,TRes>(HttpMethod.Get, url, default(object), callback, queries, forms, headers);
    }

    // Post メソッド
    public void Post<TReq,TRes>(string apiPath, TReq data, Callback<string,TRes> callback, string[] queries = null, string[] forms = null, string[] headers = null) {
        Request<TReq,TRes>(HttpMethod.Post, apiPath, data, callback, queries, forms, headers);
    }

    // リクエスト
    public void Request<TReq,TRes>(HttpMethod method, string apiPath, TReq data, Callback<string,TRes> callback, string[] queries = null, string[] forms = null, string[] headers = null) {
        var parameters = Parameters.RentFromPool();

        // パラメータを全部追加
        parameters.Import(this);
        parameters.Import(queries, forms, headers);

        // データを追加
        if (data != default(TReq)) {
            parameters.SetJsonTextContent(JsonUtility.ToJson<TReq>(data));
        }

        // リクエスト作成
        var request = parameters.CreateUnityWebRequest(method, url, apiPath);
        parameters.ReturnToPool(); // NOTE パラメータは即不要になるので返却しておく

        // TODO
        // request が isFinished になるまで待ってコールバック。
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

        // TODO
        // 待ちリストクリア
    }

    void OnDestroy() {
        // インスタンス解除
        clients.Remove(this);
        if (defaultClient == this) {
            defaultClient =null;
        }

        // TODO
        // 待ちリストクリア

        // クリア
        Clear();
    }

    void Update() {
        // TODO
        // UnityWebRequest を待つ
    }
}

////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////

// パラメータクラス
public partial class WebAPIClient {
    public class Parameters {
        //------------------------------------------------------------------ 変数
        Dictionary<string,string> queries  = new Dictionary<string,string>();
        Dictionary<string,string> forms    = new Dictionary<string,string>();
        Dictionary<string,string> headers  = new Dictionary<string,string>();
        string                    text     = null;

        //------------------------------------------------------------------ 生成と破棄
        // プールから借りる
        public static Parameters RentFromPool() {
            var parameters = ObjectPool<Parameters>.GetObject();
            parameters.queries.Clear();
            parameters.forms.Clear();
            parameters.headers.Clear();
            parameters.text = null;
            return parameters;
        }

        // プールに返却
        public void ReturnToPool() {
            queries.Clear();
            forms.Clear();
            headers.Clear();
            text = null;
            ObjectPool<Parameters>.ReturnObject(this);
        }

        //------------------------------------------------------------------ 操作 (パラメータの設定)
        // クエリを追加
        public void AddQuery(string key, string val) {
            queries.Add(key, val);
        }

        // POST フォームを追加
        public void AddForm(string key, string val) {
            forms.Add(key, val);
        }

        // ヘッダを追加
        public void AddHeader(string key, string val) {
            headers.Add(key, val);
        }

        // リクエストコンテントにテキストを設定
        public void SetTextContent(string text, string contentType = "text/plain") {
            this.headers["content-type"] = contentType;
            this.text = text;
        }

        // リクエストコンテントに JSON テキストを設定
        public void SetJsonTextContent(string jsonText, string contentType = "application/json") {
            SetTextContent(jsonText, contentType);
        }

        // リクエストコンテントにオブジェクトの JSON テキストを設定
        public void SetObjectJsonTextContent(object obj, string contentType = "application/json") {
            SetTextContent(JsonUtility.ToJson(obj), contentType);
        }

        //------------------------------------------------------------------ 操作 (リクエスト作成)
        // UnityWebRequest を取得
        public UnityWebRequest CreateUnityWebRequest(WebAPIClient client, HttpMethod method, string url, string apiPath) {
            // リクエスト作成
            var request = default(UnityWebRequest);
            switch (method) {
            case Method.POST:
                if (HasText()) {
                    request = UnityWebRequest.Post(url, GetText());
                } else if (HasWWWForm()) {
                    request = UnityWebRequest.Post(url, GetWWWForm());
                } else {
                    request = UnityWebRequest.Get(url);
                    request.method = UnityWebRequest.kHttpVerbPOST;
                }
                break;
            case Method.GET:
            default:
                request = UnityWebRequest.Get(url);
                break;
            }

            // ヘッダ追加
            SetHeadersToRequest(request);

            // リクエスト作成完了
            return request;
        }

        //------------------------------------------------------------------ 操作 (パラメータの一括取り込み)
        // インポート (string[])
        public void Import(string[] queries, string[] forms, string[] headers) {
            if (queries != null) {
                for (int i = 0; (i + 1) < queries.Length; i += 2) {
                    AddQuery(queries[i], queries[i + 1]);
                }
            }
            if (forms != null) {
                for (int i = 0; (i + 1) < forms.Length; i += 2) {
                    AddForm(forms[i], forms[i + 1]);
                }
            }
            if (headers != null) {
                for (int i = 0; (i + 1) < headers.Length; i += 2) {
                    AddHeader(headers[i], headers[i + 1]);
                }
            }
        }

        // インポート (WebAPIClient)
        public void Import(WebAPIClient client) {
            Import(client.queries, client.forms, client.headers);
        }

        // インポート (Parameters)
        public void Import(Parameters other) {
            var enumerator = other.queries.GetEnumerator();
            while (enumerator.MoveNext()) {
                var element = enumerator.Current;
                queries.Add(element.Key, element.Value);
            }
            enumerator = other.forms.GetEnumerator();
            while (enumerator.MoveNext()) {
                var element = enumerator.Current;
                forms.Add(element.Key, element.Value);
            }
            enumerator = other.headers.GetEnumerator();
            while (enumerator.MoveNext()) {
                var element = enumerator.Current;
                headers.Add(element.Key, element.Value);
            }
            other.text = text;
        }

        //------------------------------------------------------------------ 各種情報の作成、取得、設定
        // クエリ URL を作成
        string CreateRequestUrl(string url, string apiPath) {
            var sb = ObjectPool.GetObject<StringBuilder>();
            sb.Length = 0;
            sb.Append(url);
            if (!url.EndsWith("/")) {
                sb.Append("/");
            }
            sb.Append(apiPath);
            if (queries.Count > 0) {
                sb.Append("?");
                var enumerator = queries.GetEnumerator();
                while (enumerator.MoveNext()) {
                    var element = enumerator.Current;
                    sb.Append(element.Key);
                    sb.Append("&");
                    sb.Append(element.Value);
                }
            }
            var message = sb.ToString();
            sb.Length = 0;
            ObjectPool.ReturnObject(sb);
            return message;
        }

        // テキストデータを持っているか
        bool HasText() {
            return (text != null);
        }

        // テキストを取得
        string GetText() {
            return text;
        }

        // テキストを取得
        byte[] GetTextData() {
            return Encoding.UTF8.GetBytes(text);
        }

        // WWWForm を持っているか
        bool HasWWWForm() {
            return (forms.Count > 0);
        }

        // WWWForm を作成して取得
        WWWForm GetWWWForm() {
            var wwwform = new WWWForm();
            if (headers.Count > 0) {
                var enumerator = headers.GetEnumerator();
                while (enumerator.MoveNext()) {
                    var element = enumerator.Current;
                    wwwform.AddField(element.Key, element.Value);
                }
            }
            return wwwform;
        }

        // WWWForm の作成してバイト配列を取得
        byte[] GetWWWFormData() {
            return GetWWWForm().data;
        }

        // 現在のヘッダ情報を UnityWebRequest にセット
        void SetHeadersToRequest(UnityWebRequest request) {
            if (headers.Count > 0) {
                var enumerator = headers.GetEnumerator();
                while (enumerator.MoveNext()) {
                    var element = enumerator.Current;
                    request.SetRequestHeader(element.Key, element.Value);
                }
            }
        }
    }
}
