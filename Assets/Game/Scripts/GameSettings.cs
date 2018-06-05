using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

// 環境設定
[CreateAssetMenu(fileName = "GameSettings", menuName = "ScriptableObject/GameSettings", order = 1000)]
public partial class GameSettings : ScriptableObject {
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

// ScriptableObject 実装
public partial class GameSettings {
    //-------------------------------------------------------------------------- 変数
    // インスタンス
    static GameSettings instance = null;

    //-------------------------------------------------------------------------- 実装 (ScriptableObject)
    void OnEnable() {
        if (instance != null) {
            return;
        }
        instance = this;

        // NOTE
        // 即座に引数をインポート
        ImportGameArguments();
    }

    void OnDestroy() {
        if (instance != this) {
            return;
        }
        instance = null;
    }
}

//using System.IO;
//using System.Linq;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.Networking;
//using UnityEditor;
//using UnityEditor.Build;
//using UnityEditor.Callbacks;
//
//// ゲームビルドプロセッサ
//public partial class GameBuildProcessor : IPreprocessBuild, IProcessScene, IPostprocessBuild {
//    //---------------------------------------------------------------------- 実装 (IPreprocessBuild, IPostprocessBuild)
//    // ビルド前処理
//    public void OnPreprocessBuild(UnityEditor.BuildTarget target, string path) {
//        //Debug.Log("OnPreprocessBuild: target = " + target + ", path = " + path);
//    }
//
//    // シーン処理
//    public void OnProcessScene(UnityEngine.SceneManagement.Scene scene) {
//        //Debug.Log("OnProcessScene: scene = " + scene.path);
//        if (!scene.path.EndsWith("/GameManager.unity")) {
//            return;
//        }
//        if (EditorApplication.isPlaying) {//NOTE ビルド時のみ適用
//            return;
//        }
//        Apply();
//    }
//
//    // ビルド後処理
//    public void OnPostprocessBuild(BuildTarget target, string path) {
//        //Debug.Log("OnPostprocessBuild: target = " + target + ", path = " + path);
//    }
//
//    // このプロセッサの実行順
//    public int callbackOrder { get { return int.MaxValue; }}
//}
//
//// ゲーム設定の適用
//public partial class GameBuildProcessor {
//    //-------------------------------------------------------------------------- 内部処理
//    static void Apply() {
//        var gameSettingsManager = FindObjectOfType<GameSettingsManager>();
//        Debug.Assert(gameSettingsManager != null);
//        gameSettingsManager.ImportRuntimeGameSettings();
//    }
//
//    //-------------------------------------------------------------------------- ユーティリティ
//    // コンポーネントを取得する (アクティブでないコンポーネントも含む)
//    static T FindObjectOfType<T>() where T : UnityEngine.Component {
//        var components = Resources.FindObjectsOfTypeAll<T>();
//        foreach (var component in components) {
//            var gobj = component.gameObject;
//            if (   gobj.hideFlags != HideFlags.NotEditable
//                && gobj.hideFlags != HideFlags.HideAndDontSave
//                && !EditorUtility.IsPersistent(gobj.transform.root.gameObject)) {
//                return component;
//            }
//        }
//        return null;
//    }
//}
//
//// ゲームネットワークマネージャ
//// ログレベルとサービスポートを調整。
////var gameNetworkManager = FindObjectOfType<GameNetworkManager>();
////if (gameNetworkManager != null) {
////    gameNetworkManager.logLevel    = (isDebugBinary)? LogFilter.FilterLevel.Debug : LogFilter.FilterLevel.Error;
////    gameNetworkManager.networkPort = (isDebugBinary)? DEBUG_PORT : RELEASE_PORT;
////}
//// マインドリンクコネクタ
//// サーバでない場合はマインドリンクにサーバ状態を
//// 伝える必要がないのでオブジェクトごと削除。
////var mindlinkConnector = FindObjectOfType<MindlinkConnector>();
////if (mindlinkConnector != null) {
////    if (binaryServiceMode != GameManager.ServiceMode.Server) {
////        GameObject.Destroy(mindlinkConnector.gameObject);
////    }
////}
////    //---------------------------------------------------------------------- 定義
////    // バイナリ名バリアント文字列
////    public const string BINARY_NAME_VARIANT_CLIENT = "Client";
////    public const string BINARY_NAME_VARIANT_SERVER = "Server";
////    public const string BINARY_NAME_VARIANT_HOST   = "Host";
////    public const string BINARY_NAME_VARIANT_DEBUG  = "Debug";
////
////    //---------------------------------------------------------------------- 変数
////    static GameManager.ServiceMode binaryServiceMode = GameManager.ServiceMode.Client;
////    static bool                    isDebugBinary     = false;
////
////
////        var binaryName = Path.GetFileNameWithoutExtension(path);
////        var variants   = binaryName.Split('.').ToList();
////
////        // 動作モード判定
////        if (variants.IndexOf(BINARY_NAME_VARIANT_CLIENT) >= 0) {
////            binaryServiceMode = GameManager.ServiceMode.Client;
////        } else if (variants.IndexOf(BINARY_NAME_VARIANT_SERVER) >= 0) {
////            binaryServiceMode = GameManager.ServiceMode.Server;
////        } else if (variants.IndexOf(BINARY_NAME_VARIANT_HOST) >= 0) {
////            binaryServiceMode = GameManager.ServiceMode.Host;
////        } else {
////            binaryServiceMode = GameManager.ServiceMode.Host;
////        }
////
////        // デバッグバイナリ判定
////        isDebugBinary = (variants.IndexOf(BINARY_NAME_VARIANT_DEBUG) >= 0);
