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

// NetworkManager がウェブソケットを使うかどうか
public partial class GameSettings {
    //-------------------------------------------------------------------------- 変数
    public bool useWebSockets = true;

    public static bool UseWebSockets { get { return instance.useWebSockets; }}
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
        ImportCommandLineArguments();
        #if UNITY_WEBGL
        ImportWebBrowserArguments();
        #endif
    }
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// コマンドライン引数のインポート
public partial class GameSettings {
    //-------------------------------------------------------------------------- コマンドライン引数のインポート
    // コマンドライン起動引数を取得
    void ImportCommandLineArguments() {
        ImportCommandLineStringArgument(ref  this.serverAddress,          "-serverAddress",          null);
        ImportCommandLineIntegerArgument(ref this.serverPort,             "-serverPort",             null);
        ImportCommandLineIntegerArgument(ref this.serverPortRandomRange,  "-serverPortRandomRange",  null);
        ImportCommandLineStringArgument(ref  this.serverDiscoveryAddress, "-serverDiscoveryAddress", null);
        ImportCommandLineIntegerArgument(ref this.serverDiscoveryPort,    "-serverDiscoveryPort",    null);
        ImportCommandLineStringArgument(ref  this.webapiUrl,              "-webapiUrl",              null);
    }

    //-------------------------------------------------------------------------- コマンドライン引数
    static void ImportCommandLineStringArgument(ref string v, string key, string defval = null) {
        var val = GetCommandLineArgument(key, defval);
        if (val != null) {
            v = val;
        }
    }

    static void ImportCommandLineIntegerArgument(ref int v, string key, string defval = null) {
        var val = GetCommandLineArgument(key, defval);
        if (val != null) {
            v = int.Parse(val);
        }
    }

    static string GetCommandLineArgument(string key, string defval = null) {
        string[] args = System.Environment.GetCommandLineArgs();
        int index = System.Array.IndexOf(args, key);
        if (index < 0 || (index + 1) >= args.Length) {
            return defval;
        }
        return args[index + 1];
    }
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
#if UNITY_WEBGL

// ウェブブラウザ引数のインポート
public partial class GameSettings {
    //-------------------------------------------------------------------------- ウェブブラウザ引数のインポート
    void ImportWebBrowserArguments() {
        var hostName = "localhost";
        ImportWebBrowserHostName(ref hostName);
        this.webapiUrl = string.Format("http://{0}:7780", hostName);
    }

    //-------------------------------------------------------------------------- ウェブブラウザクエリ引数
    static void ImportWebBrowserQueryStringArgument(ref string v, string key, string defval = null) {
        var val = GetWebBrowserQueryArgument(key, defval);
        if (val != null) {
            v = val;
        }
    }

    static void ImportWebBrowserQueryIntegerArgument(ref int v, string key, string defval = null) {
        var val = GetWebBrowserQueryArgument(key, defval);
        if (val != null) {
            v = int.Parse(val);
        }
    }

    static string GetWebBrowserQueryArgument(string key, string defval = null) {
        return WebBrowser.GetLocationQuery(key) ?? defval;
    }

    //-------------------------------------------------------------------------- ウェブブラウザホスト名
    static void ImportWebBrowserHostName(ref v) {
        v = GetWebBrowserHostName();
    }

    static string GetWebBrowserHostName() {
        return WebBrowser.GetLocationHostName();
    }
}

#endif
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
    static GameSettings instance { get { return _instance ?? (_instance = Resources.Load<GameSettingsAsset>("GameSettings").gameSettings); }}
}
