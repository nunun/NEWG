using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

#if UNITY_EDITOR
using System.IO;
using System.Text;
#endif

// ゲーム設定マネージャ
[DefaultExecutionOrder(int.MinValue)]
public partial class GameSettingsManager : MonoBehaviour {
    // NOTE
    // パーシャルクラスを参照
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// ランタイムサービスモード
public partial class GameSettingsManager {
    //-------------------------------------------------------------------------- 定義
    public partial class RuntimeGameSettings {
        public ServiceMode runtimeServiceMode = ServiceMode.Host;
    }

    // サービスモードの定義
    public enum ServiceMode { Client, Server, Host };

    //-------------------------------------------------------------------------- 変数
    public static ServiceMode RuntimeServiceMode { get { return instance.runtimeGameSettings.runtimeServiceMode; }}
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// サーバ設定
public partial class GameSettingsManager {
    //-------------------------------------------------------------------------- 定義
    public partial class RuntimeGameSettings {
        public string serverAddress          = "localhost";
        public int    serverPort             = 7777;
        public int    serverPortRandomRange  = 0;
        public string serverToken            = null;
        public string serverSceneName        = "MapProvingGround";
        public string serverDiscoveryAddress = "localhost";
        public int    serverDiscoveryPort    = 7777;
    }

    //-------------------------------------------------------------------------- 変数
    public static string ServerAddress          { get { return instance.runtimeGameSettings.serverAddress;          }}
    public static int    ServerPort             { get { return instance.runtimeGameSettings.serverPort;             }}
    public static int    ServerPortRandomRange  { get { return instance.runtimeGameSettings.serverPortRandomRange;  }}
    public static string ServerToken            { get { return instance.runtimeGameSettings.serverToken;            }}
    public static string ServerSceneName        { get { return instance.runtimeGameSettings.serverSceneName;        }}
    public static string ServerDiscoveryAddress { get { return instance.runtimeGameSettings.serverDiscoveryAddress; }}
    public static int    ServerDiscoveryPort    { get { return instance.runtimeGameSettings.serverDiscoveryPort;    }}

    //-------------------------------------------------------------------------- 設定
    public static void SetServerInformation(string address, int port, int portRandomRange, string token, string sceneName) {
        instance.runtimeGameSettings.serverAddress         = address;
        instance.runtimeGameSettings.serverPort            = port;
        instance.runtimeGameSettings.serverPortRandomRange = portRandomRange;
        instance.runtimeGameSettings.serverToken           = token;
        instance.runtimeGameSettings.serverSceneName       = sceneName;
        instance.InvokeUpdateEvent();
    }

    public static void SetServerDiscoveryInformation(string address, int port) {
        instance.runtimeGameSettings.serverDiscoveryAddress = address;
        instance.runtimeGameSettings.serverDiscoveryPort    = port;
        instance.InvokeUpdateEvent();
    }
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// WebAPI 設定
public partial class GameSettingsManager {
    //-------------------------------------------------------------------------- 定義
    public partial class RuntimeGameSettings {
        public string webapiUrl = "http://localhost:7780";
    }

    //-------------------------------------------------------------------------- 変数
    public static string WebAPIURL { get { return instance.runtimeGameSettings.webapiUrl; }}
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// マインドリンク設定
public partial class GameSettingsManager {
    //-------------------------------------------------------------------------- 定義
    public partial class RuntimeGameSettings {
        public string mindlinkUrl = "http://localhost:7766";
    }

    //-------------------------------------------------------------------------- 変数
    public static string MindlinkURL { get { return instance.runtimeGameSettings.mindlinkUrl; }}
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// ランタイムゲーム設定の適用
public partial class GameSettingsManager {
    //-------------------------------------------------------------------------- 変数
    // ランタイムゲーム設定
    [SerializeField] RuntimeGameSettings runtimeGameSettings = new RuntimeGameSettings();

    // インポートフラグ
    bool isImported = false;

    //-------------------------------------------------------------------------- 操作
    // 実行時ゲーム設定の適用
    public static void ApplyRuntimeGameSettings(UnityEngine.Object obj) {
        var instance = GameSettingsManager.instance ?? GameObject.FindObjectOfType<GameSettingsManager>();
        Debug.Assert(instance != null, "GameSettingsManager がいない");

        // 一度もインポートしていないならインポート
        if (!instance.isImported) {
            instance.isImported = true;
            instance.ImportRuntimeGameSettings();
        }

        // 設定を適用
        // NOTE 適用できる型を増やしたい場合はここに追記します。
        if (obj is WebAPIClient) {
            var webapiClient = (WebAPIClient)obj;
            webapiClient.url = instance.runtimeGameSettings.webapiUrl;
        } else if (obj is MindlinkConnector) {
            var mindlinkConnector = (MindlinkConnector)obj;
            mindlinkConnector.url = instance.runtimeGameSettings.mindlinkUrl;
        }
    }
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// ランタイムゲーム設定更新イベント
public partial class GameSettingsManager {
    //-------------------------------------------------------------------------- 変数
    // ゲーム設定更新イベントリスナ
    static Action updateEventListener = null;

    //-------------------------------------------------------------------------- イベントリスナ関連
    // ランタイムゲーム設定更新イベントリスナの追加
    public static void AddUpdateEventListener(Action eventListener) {
        updateEventListener += eventListener;
    }

    // ランタイムゲーム設定更新イベントリスナの削除
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

// 実行時ゲーム引数のインポート
public partial class GameSettingsManager {
    //-------------------------------------------------------------------------- ゲーム引数
    public void ImportRuntimeGameSettings() {
        #if UNITY_EDITOR
        // ゲーム設定 JSON ファイル
        ImportGameSettingsJsonFile();
        #endif

        // コマンドライン引数
        ImportCommandLineArguments();

        #if UNITY_WEBGL
        // ウェブブラウザ引数
        ImportWebBrowserArguments();
        #endif
    }
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// コマンドライン引数のインポート
public partial class GameSettingsManager {
    //-------------------------------------------------------------------------- インポート処理
    // コマンドライン起動引数を取得
    void ImportCommandLineArguments() {
        ImportCommandLineStringArgument(ref  this.runtimeGameSettings.serverAddress,          "-serverAddress",          null);
        ImportCommandLineIntegerArgument(ref this.runtimeGameSettings.serverPort,             "-serverPort",             null);
        ImportCommandLineIntegerArgument(ref this.runtimeGameSettings.serverPortRandomRange,  "-serverPortRandomRange",  null);
        ImportCommandLineStringArgument(ref  this.runtimeGameSettings.serverDiscoveryAddress, "-serverDiscoveryAddress", null);
        ImportCommandLineIntegerArgument(ref this.runtimeGameSettings.serverDiscoveryPort,    "-serverDiscoveryPort",    null);
        ImportCommandLineStringArgument(ref  this.runtimeGameSettings.webapiUrl,              "-webapiUrl",              null);
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
public partial class GameSettingsManager {
    //-------------------------------------------------------------------------- ウェブブラウザ引数
    void ImportWebBrowserArguments() {
        var hostName = "localhost";
        ImportWebBrowserHostName(ref hostName);
        this.runtimeGameSettings.webapiUrl = string.Format("http://{0}:7780", hostName);
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
#if UNITY_EDITOR

// ゲーム設定 JSON ファイルのインポート
public partial class GameSettingsManager {
    //-------------------------------------------------------------------------- 定義
    // ゲーム設定へのパス
    public static readonly string GAME_SETTINGS_JSON_PATH = "Assets/GameSettings.json";

    //-------------------------------------------------------------------------- ゲーム設定
    void ImportGameSettingsJsonFile() {
        var path = GAME_SETTINGS_JSON_PATH;
        if (File.Exists(path)) {
            try {
                using (var sr = new StreamReader(path, Encoding.UTF8)) {
                    JsonUtility.FromJsonOverwrite(sr.ReadToEnd(), this.runtimeGameSettings);
                }
                Debug.LogFormat("GameSettingsManager: ランタイムゲーム設定を適用しました ({0})", path);
            } catch (Exception e) {
                Debug.LogFormat("GameSettingsManager: ランタイムゲーム設定が不正 ({0}, {1})", path, e.ToString());
            }
        }
    }
}

#endif
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// MonoBehaviour 実装
public partial class GameSettingsManager {
    //-------------------------------------------------------------------------- 変数
    // インスタンス
    static GameSettingsManager instance = null;

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
