using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

// WebAPI クライアント
public partial class WebAPIClient : MonoBehaviour {
    //-------------------------------------------------------------------------- 定義
    public enum HttpMethod { Get, Post };

    //-------------------------------------------------------------------------- 変数
    public string   url           = null; // URL
    public string[] queries       = null; // デフォルトのクエリ
    public string[] forms         = null; // デフォルトのフォーム
    public string[] headers       = null; // デフォルトのヘッダ
    public int      retryCount    = 10;   // リトライ回数
    public float    retryInterval = 3.0f; // リトライ感覚
    public string[] clientName    = null; // クライアント名
    public string   cryptSetting  = null; // 暗号化装置設定

    // 暗号化装置
    protected Crypter crypter = null;

    // インスタンスコンテナ
    static InstanceContainer<WebAPIClient> instanceContainer = new InstanceContainer<WebAPIClient>();

    //-------------------------------------------------------------------------- 実装ポイント
    // 初期化
    protected virtual void Init() {
        Clear();
    }

    // クリア
    protected virtual void Clear() {
        ClearRequestList();

        // 暗号化装置
        crypter = new Crypter(cryptSetting);
    }

    //-------------------------------------------------------------------------- リクエスト関連
    // Get メソッド
    public Request Get<TRes>(string apiPath, Action<string,TRes> callback, string[] queries = null, string[] forms = null, string[] headers = null) {
        return StartRequest<object,TRes>(HttpMethod.Get, url, default(object), callback, queries, forms, headers);
    }

    // Post メソッド
    public Request Post<TReq,TRes>(string apiPath, TReq data, Action<string,TRes> callback, string[] queries = null, string[] forms = null, string[] headers = null) {
        return StartRequest<TReq,TRes>(HttpMethod.Post, apiPath, data, callback, queries, forms, headers);
    }

    // リクエスト
    public Request StartRequest<TReq,TRes>(HttpMethod method, string apiPath, TReq data, Action<string,TRes> callback, string[] queries = null, string[] forms = null, string[] headers = null) {
        var parameters = Parameters.RentFromPool();

        // パラメータを全部追加
        parameters.Import(this);
        parameters.Import(queries, forms, headers);

        // データを追加 (POST 時)
        if (method == HttpMethod.Post) {
            parameters.SetJsonTextContent(crypter.Encrypt(JsonUtility.ToJson(data)));
        }

        // リクエストキャンセラーを返却。
        // req.Abort() でリクエストをキャンセルできる。
        return AddRequest<TRes>(method, url, apiPath, parameters, callback);
    }

    //-------------------------------------------------------------------------- インスタンス取得
    // クライアントの取得
    public static WebAPIClient GetClient(string name = null) {
        return instanceContainer.Find(name);
    }

    //-------------------------------------------------------------------------- 実装 (MonoBehaviour)
    void Awake() {
        instanceContainer.Add(clientName, this);
        Init();
    }

    void OnDestroy() {
        Clear();
        instanceContainer.Remove(this);
    }

    void Update() {
        CheckRequestList();
    }
}

////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////

// リクエストまわり
public partial class WebAPIClient {
    //---------------------------------------------------------------------- 変数
    List<Request> requestList = new List<Request>();

    //---------------------------------------------------------------------- 操作
    // リクエストの追加
    Request AddRequest<TRes>(HttpMethod method, string url, string apiPath, Parameters parameters, Action<string,TRes> callback) {
        var request = Request<TRes>.RentFromPool(this, method, url, apiPath, parameters, callback);
        requestList.Add(request);
        request.Send(); // 送信！
        enabled = true;
        return request;
    }

    // リクエストのチェック
    void CheckRequestList() {
        var deltaTime = Time.deltaTime;
        for (int i = requestList.Count - 1; i >= 0; i--) {
            var request = requestList[i];

            // リトライタイマをチェック
            if (request.retryTime > 0.0f) {
                request.retryTime -= deltaTime;
                if (request.retryTime <= 0.0f) {
                    request.retryTime = 0.0f;
                    request.Send(); // 再送！
                }
                continue;
            }

            // レスポンスチェック
            var unityWebRequest = request.UnityWebRequest;
            if (unityWebRequest.isError) {
                var error = unityWebRequest.error;
                if (error.IndexOf("Cannot connect to destination host") >= 0) {
                    if (--request.retryCount >= 0) {
                        Debug.LogWarningFormat("WebAPIClient: CheckRequestList: network error ({0}, {1})",     unityWebRequest.url, error);
                        Debug.LogWarningFormat("WebAPIClient: CheckRequestList: retry: {0} times remaining.", request.retryCount);
                        request.retryTime = request.retryInterval;
                        continue;
                    }
                }
                requestList.RemoveAt(i);
                request.SetResponse(error, null);
                request.ReturnToPool();
            } else if (unityWebRequest.isDone) {
                var message = crypter.Decrypt(unityWebRequest.downloadHandler.text);
                requestList.RemoveAt(i);
                request.SetResponse(null, message);
                request.ReturnToPool();
            }
        }
    }

    // リクエストのクリア
    void ClearRequestList() {
        for (int i = requestList.Count - 1; i >= 0; i--) {
            var request = requestList[i];
            requestList.RemoveAt(i);
            request.SetResponse("cancelled.", null);
            request.ReturnToPool();
        }
        requestList.Clear();
        enabled = false;
    }
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// リクエスト
// リクエスト中の情報を保持するためのクラス。
public partial class WebAPIClient {
    public class Request {
        //---------------------------------------------------------------------- 変数
        protected WebAPIClient    client          = default(WebAPIClient);    // WebAPI クライアント
        protected HttpMethod      method          = HttpMethod.Get;           // HTTP メソッド
        protected string          url             = default(string);          // 送信先 URL
        protected string          apiPath         = default(string);          // 送信先 API Path
        protected Parameters      parameters      = default(Parameters);      // 送信パラメータ
        public    int             retryCount      = 0;                        // リトライ回数
        public    float           retryInterval   = 0.0f;                     // リトライ間隔
        public    float           retryTime       = 0.0f;                     // リトライタイマ
        protected UnityWebRequest unityWebRequest = default(UnityWebRequest); // リクエスト実態

        // 転送および解放コールバック
        // サブクラスが設定
        protected Action<Request,string,string> setResponse  = null;
        protected Action<Request>               returnToPool = null;

        // UnityWebRequest の取得
        public UnityWebRequest UnityWebRequest { get { return unityWebRequest; }}

        //---------------------------------------------------------------------- 操作
        // 送信
        public void Send() {
            Debug.LogFormat("WebAPIClient.Request.Send: method[{0}] url[{1}] apiPath[{2}] parameters[{3}]", method, url, apiPath, parameters);
            this.unityWebRequest = parameters.CreateUnityWebRequest(client, method, url, apiPath);
            this.unityWebRequest.Send();
        }

        // レスポンスデータを設定
        public void SetResponse(string error, string message) {
            Debug.Assert(setResponse != null);
            setResponse(this, error, message);
        }

        // 解放
        public void ReturnToPool() {
            Debug.Assert(returnToPool != null);
            returnToPool(this);
        }

        //---------------------------------------------------------------------- 操作 (中断)
        // リクエスト中断
        public void Abort() {
            // NOTE
            // Unity 側を中断するだけ。
            // WebAPIClient の Update ポーリングで
            // リクエストの終了チェックが行われる。
            unityWebRequest.Abort();
        }
    }
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// リクエスト (型別)
// リクエスト中の情報を保持するためのクラス。
public partial class WebAPIClient {
    public class Request<TRes> : Request {
        //---------------------------------------------------------------------- 変数
        // 受信コールバック
        protected Action<string,TRes> callback = null;

        //---------------------------------------------------------------------- 確保
        // プールから借りる
        public static Request<TRes> RentFromPool(WebAPIClient client, HttpMethod method, string url, string apiPath, Parameters parameters, Action<string,TRes> callback) {
            var req = ObjectPool<Request<TRes>>.RentObject();
            req.client          = client;
            req.method          = method;
            req.url             = url;
            req.apiPath         = apiPath;
            req.parameters      = parameters;
            req.unityWebRequest = default(UnityWebRequest);
            req.retryCount      = client.retryCount;
            req.retryInterval   = client.retryInterval;
            req.retryTime       = 0.0f;
            req.setResponse     = Request<TRes>.SetResponse;
            req.returnToPool    = Request<TRes>.ReturnToPool;
            req.callback        = callback;
            return req;
        }

        //---------------------------------------------------------------------- 内部コールバック用
        // レスポンスデータを設定
        protected static void SetResponse(Request request, string error, string message) {
            Debug.LogFormat("WebAPIClient.Request<{0}>.SetResponse: error[{1}] message[{2}]", typeof(TRes), error, message);
            var req = request as Request<TRes>;
            Debug.Assert(req != null);
            try {
                if (!string.IsNullOrEmpty(error)) {
                    throw new Exception(error);
                }
                var data = JsonUtility.FromJson<TRes>(message);
                if (req.callback != null) {
                    var callback = req.callback;
                    req.callback = null;
                    callback(null, data);
                }
            } catch (Exception e) {
                if (req.callback != null) {
                    var callback = req.callback;
                    req.callback = null;
                    callback(e.ToString(), default(TRes));
                }
            }
        }

        // 解放
        protected static void ReturnToPool(Request request) {
            var req = request as Request<TRes>;
            Debug.Assert(req != null);
            if (req.parameters != null) {
                req.parameters.ReturnToPool();
            }
            if (req.setResponse != null) {
                req.setResponse(req, "cancelled.", null);
            }
            req.client          = default(WebAPIClient);
            req.method          = HttpMethod.Get;
            req.url             = default(string);
            req.apiPath         = default(string);
            req.parameters      = default(Parameters);
            req.unityWebRequest = default(UnityWebRequest);
            req.retryCount      = 0;
            req.retryInterval   = 0.0f;
            req.retryTime       = 0.0f;
            req.setResponse     = null;
            req.returnToPool    = null;
            req.callback        = null;
            ObjectPool<Request<TRes>>.ReturnObject(req);
        }
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
            var parameters = ObjectPool<Parameters>.RentObject();
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
            // url
            var sb = ObjectPool<StringBuilder>.RentObject();
            sb.Length = 0;
            sb.Append(url);
            sb.Append(apiPath);
            url = sb.ToString();
            sb.Length = 0;
            ObjectPool<StringBuilder>.ReturnObject(sb);

            // リクエスト作成
            var request = default(UnityWebRequest);
            switch (method) {
            case HttpMethod.Post:
                if (HasText()) {
                    if (this.headers["content-type"] == "application/json") {
                        var bytes = Encoding.UTF8.GetBytes(GetText());
                        request = new UnityWebRequest(url);
                        request.uploadHandler   = new UploadHandlerRaw(bytes);
                        request.downloadHandler = new DownloadHandlerBuffer();
                        request.method          = UnityWebRequest.kHttpVerbPOST;
                    } else {
                        request = UnityWebRequest.Post(url, GetText());
                    }
                } else if (HasWWWForm()) {
                    request = UnityWebRequest.Post(url, GetWWWForm());
                } else {
                    request = UnityWebRequest.Get(url);
                    request.method = UnityWebRequest.kHttpVerbPOST;
                }
                break;
            case HttpMethod.Get:
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
            var sb = ObjectPool<StringBuilder>.RentObject();
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
            ObjectPool<StringBuilder>.ReturnObject(sb);
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

        //------------------------------------------------------------------ 実装 (ToString)
        public override string ToString() {
            var debugQueries = string.Join(", ", this.queries.Select((v) => v.Key + ":" + v.Value).ToArray());
            var debugForms   = string.Join(", ", this.forms.Select((v)   => v.Key + ":" + v.Value).ToArray());
            var debugHeaders = string.Join(", ", this.headers.Select((v) => v.Key + ":" + v.Value).ToArray());
            var debugText    = this.text;
            var debugFormat = "queries[{0}] forms[{1}] headers[{2}] text[{3}]";
            return string.Format(debugFormat, debugQueries, debugForms, debugHeaders, debugText);
        }
    }
}