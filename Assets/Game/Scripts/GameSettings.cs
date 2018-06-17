using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

#if UNITY_EDITOR
using UnityEditor;
#endif

#if STANDALONE_MODE
using Services.Protocols;
using Services.Protocols.Consts;
using Services.Protocols.Models;
#endif

// ゲーム設定
[Serializable]
public partial class GameSettings {
    // NOTE
    // パーシャルクラスを参照
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// ランタイムサービスモード
public partial class GameSettings {
    //-------------------------------------------------------------------------- 定義
    public enum ServiceMode { Client, Server, Host };

    //-------------------------------------------------------------------------- 変数
    public ServiceMode runtimeServiceMode = ServiceMode.Host;

    public static ServiceMode RuntimeServiceMode { get { return instance.runtimeServiceMode; }}
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// サーバ設定
public partial class GameSettings {
    //-------------------------------------------------------------------------- 変数
    public string serverAddress          = "localhost";
    public int    serverPort             = 7777;
    public string serverToken            = null;
    public string serverSceneName        = "MapProvingGround";
    public string serverDiscoveryAddress = "localhost";
    public int    serverDiscoveryPort    = 7777;
    public int    serverPortRandomRange  = 0;

    public static string ServerAddress          { get { return instance.serverAddress;          }}
    public static int    ServerPort             { get { return instance.serverPort;             }}
    public static string ServerToken            { get { return instance.serverToken;            }}
    public static string ServerSceneName        { get { return instance.serverSceneName;        }}
    public static string ServerDiscoveryAddress { get { return instance.serverDiscoveryAddress; }}
    public static int    ServerDiscoveryPort    { get { return instance.serverDiscoveryPort;    }}
    public static int    ServerPortRandomRange  { get { return instance.serverPortRandomRange;  }}

    //-------------------------------------------------------------------------- 設定
    public static void SetServerInformation(string address, int port, string token, string sceneName, int portRandomRange) {
        instance.serverAddress         = address;
        instance.serverPort            = port;
        instance.serverToken           = token;
        instance.serverSceneName       = sceneName;
        instance.serverPortRandomRange = portRandomRange;
        instance.InvokeUpdateEvent();
    }

    public static void SetServerDiscoveryInformation(string address, int port) {
        instance.serverDiscoveryAddress = address;
        instance.serverDiscoveryPort    = port;
        instance.InvokeUpdateEvent();
    }
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// NetworkManager 設定関連
public partial class GameSettings {
    //-------------------------------------------------------------------------- 変数
    public bool                  bindIp        = false;
    public bool                  useWebSockets = true;
    public LogFilter.FilterLevel logLevel      = LogFilter.FilterLevel.Debug;

    public static bool                  BindIP        { get { return instance.bindIp;        }}
    public static bool                  UseWebSockets { get { return instance.useWebSockets; }}
    public static LogFilter.FilterLevel LogLevel      { get { return instance.logLevel;      }}
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// マッチングサーバ設定
public partial class GameSettings {
    //-------------------------------------------------------------------------- 変数
    public string matchingServerUrl = "ws://localhost:7755";

    public static string MatchingServerUrl { get { return instance.matchingServerUrl; }}

    //-------------------------------------------------------------------------- 設定
    public static void SetMatchingServerInformation(string matchingServerUrl) {
        instance.matchingServerUrl = matchingServerUrl;
        instance.InvokeUpdateEvent();
    }
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// WebAPI 設定
public partial class GameSettings {
    //-------------------------------------------------------------------------- 変数
    public string webapiUrl = "http://localhost:7780";

    public static string WebAPIURL { get { return instance.webapiUrl; }}
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// マインドリンク設定
public partial class GameSettings {
    //-------------------------------------------------------------------------- 変数
    public string mindlinkUrl = "ws://localhost:7766";

    public static string MindlinkURL { get { return instance.mindlinkUrl; }}
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
#if STANDALONE_MODE

// スタンドアローンモード設定
public partial class GameSettings {
    //-------------------------------------------------------------------------- 定義
    static readonly float DEBUG_DELAY = 0.5f; // デバッグディレイ

    //-------------------------------------------------------------------------- 変数
    WebAPIClient.Request debugRequest = null; // デバッグ中のリクエスト
    float                debugDelay   = 0.0f; // デバッグディレイ

    // スタンドアローンモードかどうか (常に true)
    public static bool IsStandaloneMode { get { return true; }}

    //-------------------------------------------------------------------------- WebAPI エミュレーション
    public static bool SimulateWebAPI(WebAPIClient.Request request, float deltaTime) {
        if (instance.debugRequest == null) {
            Debug.LogFormat("StandaloneSimulatorSettings: WebAPI リクエストを処理 ({0})", request.ToString());
            instance.debugRequest = request;
            instance.debugDelay   = DEBUG_DELAY;
        }
        if (instance.debugDelay > 0.0f) {//WebAPIっぽい待ちディレイをつけておく
            instance.debugDelay -= deltaTime;
            return true;
        }
        instance.SimulateWebAPIRequest(request);
        instance.debugRequest = null;
        return false;
    }

    //-------------------------------------------------------------------------- WebAPI エミュレーションの処理
    void SimulateWebAPIRequest(WebAPIClient.Request request) {
        switch (request.APIPath) {
        case "/signup"://サインアップ
            {
                //var req = JsonUtility.FromJson<WebAPI.SignupRequest>(request.Parameters.GetText());

                var playerData = new PlayerData();
                playerData.playerId   = "(dummy playerId)";
                playerData.playerName = "(dummy player)";

                var sessionData = new SessionData();
                sessionData.sessionToken = "(dummy sessionToken)";

                var credentialData = new CredentialData();
                credentialData.signinToken = "(dummy signinToken)";

                var playerDataJson     = string.Format("\"playerData\":{{\"active\":true,\"data\":{0}}}",     JsonUtility.ToJson(playerData));
                var sessionDataJson    = string.Format("\"sessionData\":{{\"active\":true,\"data\":{0}}}",    JsonUtility.ToJson(sessionData));
                var credentialDataJson = string.Format("\"credentialData\":{{\"active\":true,\"data\":{0}}}", JsonUtility.ToJson(credentialData));
                var response = string.Format("{{\"activeData\":{{{0},{1},{2}}}}}", playerDataJson, sessionDataJson, credentialDataJson);
                request.SetResponse(null, response);
            }
            break;
        case "/signin"://サインイン
            {
                //var req = JsonUtility.FromJson<WebAPI.SignupRequest>(request.Parameters.GetText());

                var playerData = new PlayerData();
                playerData.playerId   = "(dummy playerId)";
                playerData.playerName = "(dummy player)";

                var sessionData = new SessionData();
                sessionData.sessionToken = "(dummy sessionToken)";

                var playerDataJson  = string.Format("\"playerData\":{{\"active\":true,\"data\":{0}}}",  JsonUtility.ToJson(playerData));
                var sessionDataJson = string.Format("\"sessionData\":{{\"active\":true,\"data\":{0}}}", JsonUtility.ToJson(sessionData));
                var response = string.Format("{{\"activeData\":{{{0},{1}}}}}", playerDataJson, sessionDataJson);
                request.SetResponse(null, response);
            }
            break;
        case "/matching"://マッチング
            {
                //var req = JsonUtility.FromJson<WebAPI.SignupRequest>(request.Parameters.GetText());

                var matchingResponse = new WebAPI.MatchingResponse();
                matchingResponse.matchingServerUrl = "ws//localhost:7755?matchingId=dummy_token";

                var matchingResponseJson = JsonUtility.ToJson(matchingResponse);
                var response = string.Format("{0}", matchingResponseJson);
                request.SetResponse(null, response);
            }
            break;
        default:
            Debug.LogErrorFormat("スタンドアローンデバッグで処理できない API パス ({0})", request.APIPath);
            break;
        }
    }
}

#endif
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// ゲーム設定更新イベント
public partial class GameSettings {
    //-------------------------------------------------------------------------- 変数
    // ゲーム設定更新イベントリスナ
    static Action updateEventListener = null;

    //-------------------------------------------------------------------------- イベントリスナ関連
    // ゲーム設定更新イベントリスナの追加
    public static void AddUpdateEventListener(Action eventListener) {
        updateEventListener += eventListener;
    }

    // ゲーム設定更新イベントリスナの削除
    public static void RemoveUpdateEventListener(Action eventListener) {
        updateEventListener -= eventListener;
    }

    //-------------------------------------------------------------------------- 内部処理
    // 更新イベントの発行
    void InvokeUpdateEvent() {
        if (updateEventListener != null) {
            updateEventListener();
        }
    }
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// ゲーム引数のインポート
public partial class GameSettings {
    //-------------------------------------------------------------------------- ゲーム引数のインポート
    public void ImportGameArguments() {
        GameManager.ImportCommandLineStringArgument("-serverAddress",          ref this.serverAddress);
        GameManager.ImportCommandLineIntegerArgument("-serverPort",            ref this.serverPort);
        GameManager.ImportCommandLineIntegerArgument("-serverPortRandomRange", ref this.serverPortRandomRange);
        GameManager.ImportCommandLineStringArgument("-serverDiscoveryAddress", ref this.serverDiscoveryAddress);
        GameManager.ImportCommandLineIntegerArgument("-serverDiscoveryPort",   ref this.serverDiscoveryPort);
        GameManager.ImportCommandLineStringArgument("-webapiUrl",              ref this.webapiUrl);
        #if UNITY_WEBGL
        var webBrowserHostName = default(string);
        if (GameManager.ImportWebBrowserHostName(ref webBrowserHostName)) {
             this.webapiUrl = string.Format("http://{0}:7780", webBrowserHostName);
        }
        #endif
    }
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// NOTE
// 設定インスタンス
// BuildSettings > PlayerSettings の "Preload Assets" に設定して
// ScriptableObject の OnEnable から起動時に読み込まれる前提だが、
// これはエディタ上では読み込まれないことがあるので
// その場合は AssetDatabase から直接ロードする。
public partial class GameSettings {
    //-------------------------------------------------------------------------- 定義
    // ゲーム設定アセット
    public class Asset : ScriptableObject {
        public GameSettings gameSettings = new GameSettings();
        protected void OnEnable()  { GameSettings.SetInstance(gameSettings);   }
        protected void OnDisable() { GameSettings.UnsetInstance(gameSettings); }
    }

    #if UNITY_EDITOR
    public static readonly string ASSET_PATH = "Assets/Game/Settings/GameSettings.asset";
    #endif

    //-------------------------------------------------------------------------- 初期化
    // 内部インスタンス
    static GameSettings _instance = null;

    // インスタンスの取得
    static GameSettings instance {
        get {
            if (_instance == null) {
                #if UNITY_EDITOR
                var gameSettings = ((Asset)AssetDatabase.LoadAssetAtPath(ASSET_PATH, typeof(Asset))).gameSettings;
                SetInstance(gameSettings);
                #else
                Debug.LogError("GameSettings が Preload Assets にない");
                #endif
            }
            return _instance;
        }
    }

    // インスタンス設定
    static void SetInstance(GameSettings instance) {
        if (_instance != null) {
            return;
        }
        _instance = instance;

        // NOTE
        // ここで引数をインポート
        _instance.ImportGameArguments();
    }

    // インスタンス解除
    static void UnsetInstance(GameSettings instance) {
        if (_instance != instance) {
            return;
        }
        _instance = null;
    }
}
