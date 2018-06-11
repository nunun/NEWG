using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

#if UNITY_EDITOR
using UnityEditor;
#endif

// 環境設定
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
    public int    serverPortRandomRange  = 0;
    public string serverToken            = null;
    public string serverSceneName        = "MapProvingGround";
    public string serverDiscoveryAddress = "localhost";
    public int    serverDiscoveryPort    = 7777;

    public static string ServerAddress          { get { return instance.serverAddress;          }}
    public static int    ServerPort             { get { return instance.serverPort;             }}
    public static int    ServerPortRandomRange  { get { return instance.serverPortRandomRange;  }}
    public static string ServerToken            { get { return instance.serverToken;            }}
    public static string ServerSceneName        { get { return instance.serverSceneName;        }}
    public static string ServerDiscoveryAddress { get { return instance.serverDiscoveryAddress; }}
    public static int    ServerDiscoveryPort    { get { return instance.serverDiscoveryPort;    }}

    //-------------------------------------------------------------------------- 設定
    public static void SetServerInformation(string address, int port, int portRandomRange, string token, string sceneName) {
        instance.serverAddress         = address;
        instance.serverPort            = port;
        instance.serverPortRandomRange = portRandomRange;
        instance.serverToken           = token;
        instance.serverSceneName       = sceneName;
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

// インスタンス関連
public partial class GameSettings {
    //-------------------------------------------------------------------------- 定義
    // ゲーム設定アセット
    public class Asset : ScriptableObject {
        public GameSettings gameSettings = new GameSettings();
    }

    //-------------------------------------------------------------------------- 変数
    // 内部インスタンス
    static GameSettings _instance = null;

    // インスタンスの取得
    static GameSettings instance {
        get {
            if (_instance == null) {
                _instance = Resources.Load<GameSettingsAsset>("GameSettings").gameSettings;
                _instance.ImportGameArguments();
            }
            return _instance;
        }
    }
}
