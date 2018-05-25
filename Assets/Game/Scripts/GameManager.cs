using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

// ゲームマネージャ
// ゲーム自体の制御と設定保持を行います。
[DefaultExecutionOrder(int.MinValue)]
public partial class GameManager : MonoBehaviour {
    // NOTE
    // パーシャルクラスを参照
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// サービスモード
public partial class GameManager {
    //-------------------------------------------------------------------------- 定義
    public enum ServiceMode { Client, Server, Host }; // サービスモードの定義

    //-------------------------------------------------------------------------- 変数
    public ServiceMode runtimeServiceMode = ServiceMode.Client; // 実行時サービスモード

    // 実行時サービスモードの取得
    public static ServiceMode RuntimeServiceMode { get { return instance.runtimeServiceMode; }}
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// サーバ情報
// マッチングで決定したサーバ接続先の一時的な情報です。
public partial class GameManager {
    //-------------------------------------------------------------------------- 変数
    public string serverAddress         = "localhost";            // サーバアドレス
    public int    serverPort            = 7777;                   // サーバポート
    public int    serverPortRandomRange = 0;                      // サーバポート ランダム幅
    public string serverToken           = null;                   // サーバトークン
    public string serverSceneName       = "NetworkProvingGround"; // サーバシーン名

    public static string ServerAddress         { get { return instance.serverAddress;         }}
    public static int    ServerPort            { get { return instance.serverPort;            }}
    public static int    ServerPortRandomRange { get { return instance.serverPortRandomRange; }}
    public static string ServerToken           { get { return instance.serverToken;           }}
    public static string ServerSceneName       { get { return instance.serverSceneName;       }}

    //-------------------------------------------------------------------------- 設定
    public static void SetServerInformation(string address, int port, int portRandomRange, string token, string sceneName) {
        instance.serverAddress         = address;
        instance.serverPort            = port;
        instance.serverPortRandomRange = portRandomRange;
        instance.serverToken           = token;
        instance.serverSceneName       = sceneName;
    }
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// マッチングサーバ情報
public partial class GameManager {
    //-------------------------------------------------------------------------- 変数
    public string matchingServerUrl = "ws://localhost:7755"; // マッチングサーバアドレス

    public static string MatchingServerUrl { get { return instance.matchingServerUrl; }}

    //-------------------------------------------------------------------------- 設定
    public static void SetMatchingServerInformation(string matchingServerUrl) {
        instance.matchingServerUrl = matchingServerUrl;
    }
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// マインドリンク情報
public partial class GameManager {
    //-------------------------------------------------------------------------- 変数
    public string mindlinkUrl           = "ws://localhost:7766"; // マインドリンク接続先
    public string mindlinkServerAddress = "localhost";           // マインドリンク上の広報サーバアドレス
    public int    mindlinkServerPort    = 7777;                  // マインドリンク上の広報サーバポート

    public static string MindlinkUrl           { get { return instance.mindlinkUrl;           }}
    public static string MindlinkServerAddress { get { return instance.mindlinkServerAddress; }}
    public static int    MindlinkServerPort    { get { return instance.mindlinkServerPort;    }}

    //-------------------------------------------------------------------------- 設定
    public static void SetMindlinkInformation(string mindlinkServerAddress, int mindlinkServerPort) {
        instance.mindlinkServerAddress = mindlinkServerAddress;
        instance.mindlinkServerPort    = mindlinkServerPort;
    }
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// スタンドアローンモード情報
public partial class GameManager {
    //-------------------------------------------------------------------------- 変数
    public StandaloneSimulator standaloneSimulator = null; // スタンドアローンシミュレータ

    // スタンドアローンシミュレータの取得
    public static StandaloneSimulator StandaloneSimulator { get { return instance.standaloneSimulator; }}

    // スタンドアローンモードかどうか
    // このフラグの if 文を置くことによって Unreachable コードの警告を回避
    public static bool IsStandaloneMode {
        get {
            #if STANDALONE_MODE
            return true;
            #else
            return false;
            #endif
        }
    }
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// ゲームの中断と終了
public partial class GameManager {
    //-------------------------------------------------------------------------- 変数
    static string abortSceneName = "Abort";
    static string lastAbortError = null;

    //-------------------------------------------------------------------------- 中断
    public static void Abort(string error = null) {
        error = error ?? "Game Aborted. Fatal Error occurred.";
        Debug.LogErrorFormat("GameManager.Abort: error='{0}'", error);
        SetLastAbortErorr(error);
        if (!GameSceneManager.ChangeSceneImmediately(abortSceneName)) {
            Quit();
            return;
        }
    }

    static void SetAbortSceneName(string sceneName) {
        abortSceneName = sceneName;
    }

    static void SetLastAbortErorr(string error) {
        lastAbortError = error;
    }

    public static string GetLastAbortError() {
        return lastAbortError;
    }

    public static void ClearLastAbortError() {
        lastAbortError = null;
    }

    //-------------------------------------------------------------------------- 終了
    public static void Quit() {
        Application.Quit();
        #if UNITY_EDITOR
        EditorApplication.isPlaying = false;
        #endif
    }
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// 起動引数のインポート
public partial class GameManager {
    //-------------------------------------------------------------------------- 操作
    // 起動引数のインポート
    public static void ImportLaunchArguments() {
        instance.ImportCommandLineLaunchArguments();
        #if UNITY_WEBGL
        instance.ImportWebBrowserLaunchArguments();
        #endif
    }

    //-------------------------------------------------------------------------- インポート処理
    // コマンドライン起動引数を取得
    void ImportCommandLineLaunchArguments() {
        ImportCommandLineStringArgument(ref serverAddress,         "-serverAddress",         null);
        ImportCommandLineIntegerArgument(ref serverPort,           "-serverPort",            null);
        ImportCommandLineStringArgument(ref mindlinkServerAddress, "-mindlinkServerAddress", null);
        ImportCommandLineIntegerArgument(ref mindlinkServerPort,   "-mindlinkServerPort",    null);
    }

    #if UNITY_WEBGL
    // ウェブブラウザクエリ起動引数を取得
    void ImportWebBrowserLaunchArguments() {
        // NOTE
        // 今のところ何もなし
    }
    #endif

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

    //-------------------------------------------------------------------------- ウェブブラウザクエリ引数
    #if UNITY_WEBGL
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
    #endif
    //-------------------------------------------------------------------------- ウェブブラウザホスト名
    #if UNITY_WEBGL
    static void ImportWebBrowserHostName(ref v) {
        v = GetWebBrowserHostName();
    }

    static string GetWebBrowserHostName() {
        return WebBrowser.GetLocationHostName();
    }
    #endif
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// MonoBehaviour 実装
public partial class GameManager {
    //-------------------------------------------------------------------------- 変数
    // ゲームサービスインスタンス
    static GameManager instance = null;

    //-------------------------------------------------------------------------- 実装 (MonoBehaviour)
    void Awake() {
        if (instance != null) {
            GameObject.Destroy(this.gameObject);
            return;
        }
        instance = this;
    }

    void OnDestroy() {
        if (instance != this) {
            return;
        }
        instance = null;
    }
}
