using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

// ゲーム構成
[Serializable]
public partial class GameConfiguration : GameSettings {
    //-------------------------------------------------------------------------- 変数
    // ゲーム構成一覧の定義
    // NOTE 追加のゲーム構成をここに定義して下さい。
    public static readonly List<GameConfiguration> GameConfigurationList = new List<GameConfiguration>() {
        new GameConfiguration() {
            gameConfigurationName        = "DEBUG_HOST",
            gameConfigurationDescription = "デバッグホスト (実行可能形式)",
            runtimeServiceMode           = GameSettings.ServiceMode.Host,
            serverAddress                = "localhost",
            serverPort                   = 7778,
            serverPortRandomRange        = 0,
            serverDiscoveryAddress       = "localhost",
            serverDiscoveryPort          = 7778,
            useWebSockets                = false,
            webapiUrl                    = "http://localhost:7780",
            mindlinkUrl                  = "ws://localhost:7766",
            buildTarget                  = BuildTarget.StandaloneWindows64,
            outputPath                   = "Builds/Debug.Host",
            headless                     = false,
            autoRun                      = true,
            openFolder                   = false,
            resolutionDialogSetting      = ResolutionDialogSetting.Disabled,
            screenWidth                  = 1280,
            screenHeight                 = 720,
            isFullScreen                 = false,
            runInBackground              = true,
            showSplashScreen             = false,
            developmentBuild             = true,
            localServerStartUrl          = null,
            localServerStopUrl           = null,
            scriptingDefineSymbols       = new List<string>() {"DEBUG", "SERVER_CODE"},
        },
        new GameConfiguration() {
            gameConfigurationName        = "DEBUG_CLIENT",
            gameConfigurationDescription = "デバッグクライアント (実行可能形式)",
            runtimeServiceMode           = GameSettings.ServiceMode.Client,
            serverAddress                = null,
            serverPort                   = 0,
            serverPortRandomRange        = 0,
            serverDiscoveryAddress       = null,
            serverDiscoveryPort          = 0,
            useWebSockets                = false,
            webapiUrl                    = "http://localhost:7780",
            mindlinkUrl                  = null,
            buildTarget                  = BuildTarget.StandaloneWindows64,
            outputPath                   = "Builds/Debug.Client",
            headless                     = false,
            autoRun                      = true,
            openFolder                   = false,
            resolutionDialogSetting      = ResolutionDialogSetting.Disabled,
            screenWidth                  = 1280,
            screenHeight                 = 720,
            isFullScreen                 = false,
            runInBackground              = true,
            showSplashScreen             = false,
            developmentBuild             = true,
            localServerStartUrl          = null,
            localServerStopUrl           = null,
            scriptingDefineSymbols       = new List<string>() {"DEBUG"},
        },
        new GameConfiguration() {
            gameConfigurationName        = "DEBUG_SERVER",
            gameConfigurationDescription = "デバッグサーバ (実行可能形式)",
            runtimeServiceMode           = GameSettings.ServiceMode.Server,
            serverAddress                = "localhost",
            serverPort                   = 7778,
            serverPortRandomRange        = 0,
            serverDiscoveryAddress       = "localhost",
            serverDiscoveryPort          = 7778,
            useWebSockets                = false,
            webapiUrl                    = "http://localhost:7780",
            mindlinkUrl                  = "ws://localhost:7766",
            buildTarget                  = BuildTarget.StandaloneWindows64,
            outputPath                   = "Builds/Debug.Server",
            headless                     = false,
            autoRun                      = true,
            openFolder                   = false,
            resolutionDialogSetting      = ResolutionDialogSetting.Disabled,
            screenWidth                  = 1280,
            screenHeight                 = 720,
            isFullScreen                 = false,
            runInBackground              = true,
            showSplashScreen             = false,
            developmentBuild             = true,
            localServerStartUrl          = null,
            localServerStopUrl           = null,
            scriptingDefineSymbols       = new List<string>() {"DEBUG", "SERVER_CODE"},
        },
        new GameConfiguration() {
            gameConfigurationName        = "LOCAL_CLIENT",
            gameConfigurationDescription = "ローカルクライアント (WebGL)",
            runtimeServiceMode           = GameSettings.ServiceMode.Client,
            serverAddress                = null,
            serverPort                   = 0,
            serverPortRandomRange        = 0,
            serverDiscoveryAddress       = null,
            serverDiscoveryPort          = 0,
            useWebSockets                = true,
            webapiUrl                    = "http://localhost:7780",
            mindlinkUrl                  = null,
            buildTarget                  = BuildTarget.WebGL,
            outputPath                   = "Builds/Local.Client",
            headless                     = false,
            autoRun                      = true,
            openFolder                   = false,
            resolutionDialogSetting      = ResolutionDialogSetting.Disabled,
            screenWidth                  = 1280,
            screenHeight                 = 720,
            isFullScreen                 = false,
            runInBackground              = true,
            showSplashScreen             = false,
            developmentBuild             = true,
            localServerStartUrl          = null,
            localServerStopUrl           = null,
            scriptingDefineSymbols       = new List<string>() {"DEBUG"},
        },
        new GameConfiguration() {
            gameConfigurationName        = "SERVICES_LOCAL_CLIENT",
            gameConfigurationDescription = "サービス用ローカルクライアント (WebGL)",
            runtimeServiceMode           = GameSettings.ServiceMode.Client,
            serverAddress                = null,
            serverPort                   = 0,
            serverPortRandomRange        = 0,
            serverDiscoveryAddress       = null,
            serverDiscoveryPort          = 0,
            useWebSockets                = true,
            webapiUrl                    = "http://localhost:7780",
            mindlinkUrl                  = null,
            buildTarget                  = BuildTarget.WebGL,
            outputPath                   = "Services/client/Builds/Client",
            headless                     = false,
            autoRun                      = false,
            openFolder                   = true,
            resolutionDialogSetting      = ResolutionDialogSetting.Disabled,
            screenWidth                  = 1280,
            screenHeight                 = 720,
            isFullScreen                 = false,
            runInBackground              = true,
            showSplashScreen             = false,
            developmentBuild             = true,
            localServerStartUrl          = null,
            localServerStopUrl           = null,
            scriptingDefineSymbols       = new List<string>() {"DEBUG"},
        },
        new GameConfiguration() {
            gameConfigurationName        = "SERVICES_LOCAL_SERVER",
            gameConfigurationDescription = "サービス用ローカルサーバ (Linux Headless, Non Host Mode)",
            runtimeServiceMode           = GameSettings.ServiceMode.Server,
            serverAddress                = "0.0.0.0",
            serverPort                   = 7777,
            serverPortRandomRange        = 0,
            serverDiscoveryAddress       = "localhost",
            serverDiscoveryPort          = 7777,
            useWebSockets                = true,
            webapiUrl                    = "http://api:7780",
            mindlinkUrl                  = "ws://mindlink:7766",
            buildTarget                  = BuildTarget.StandaloneLinuxUniversal,
            outputPath                   = "Services/server/Builds/Server",
            headless                     = true,
            autoRun                      = false,
            openFolder                   = true,
            resolutionDialogSetting      = ResolutionDialogSetting.Disabled,
            screenWidth                  = 1280,
            screenHeight                 = 720,
            isFullScreen                 = false,
            runInBackground              = true,
            showSplashScreen             = false,
            developmentBuild             = false,
            localServerStartUrl          = "http://localhost:17777/start",
            localServerStopUrl           = "http://localhost:17777/stop",
            scriptingDefineSymbols       = new List<string>() {"DEBUG", "SERVER_CODE"},
        },
        new GameConfiguration() {
            gameConfigurationName        = "SERVICES_RELEASE_CLIENT",
            gameConfigurationDescription = "サービス用 fu-n.net クライアント (WebGL)",
            runtimeServiceMode           = GameSettings.ServiceMode.Client,
            serverAddress                = null,
            serverPort                   = 0,
            serverPortRandomRange        = 0,
            serverDiscoveryAddress       = null,
            serverDiscoveryPort          = 0,
            useWebSockets                = true,
            webapiUrl                    = "http://localhost:7780",
            mindlinkUrl                  = null,
            buildTarget                  = BuildTarget.WebGL,
            outputPath                   = "Services/client/Builds/Client",
            headless                     = false,
            autoRun                      = false,
            openFolder                   = true,
            resolutionDialogSetting      = ResolutionDialogSetting.Disabled,
            screenWidth                  = 1280,
            screenHeight                 = 720,
            isFullScreen                 = false,
            runInBackground              = true,
            showSplashScreen             = false,
            developmentBuild             = false,
            localServerStartUrl          = null,
            localServerStopUrl           = null,
            scriptingDefineSymbols       = new List<string>() {},
        },
        new GameConfiguration() {
            gameConfigurationName        = "SERVICES_RELEASE_SERVER",
            gameConfigurationDescription = "サービス用 fu-n.net サーバ (Linux Headless, Host Mode)",
            runtimeServiceMode           = GameSettings.ServiceMode.Server,
            serverAddress                = "0.0.0.0",
            serverPort                   = 8000,
            serverPortRandomRange        = 20,
            serverDiscoveryAddress       = "localhost",
            serverDiscoveryPort          = 8000,
            useWebSockets                = true,
            webapiUrl                    = "http://localhost:7780",
            mindlinkUrl                  = "ws://localhost:7766",
            buildTarget                  = BuildTarget.StandaloneLinuxUniversal,
            outputPath                   = "Services/server/Builds/Server",
            headless                     = true,
            autoRun                      = false,
            openFolder                   = true,
            resolutionDialogSetting      = ResolutionDialogSetting.Disabled,
            screenWidth                  = 1280,
            screenHeight                 = 720,
            isFullScreen                 = false,
            runInBackground              = true,
            showSplashScreen             = false,
            developmentBuild             = false,
            localServerStartUrl          = null,
            localServerStopUrl           = null,
            scriptingDefineSymbols       = new List<string>() {"SERVER_CODE"},
        },
    };

    // シンボル一覧の定義
    // NOTE 追加のシンボルをここに定義してください。
    public static readonly Dictionary<string,string[]> ScriptingDefineSymbols = new Dictionary<string,string[]>() {
        { "DEBUG",           null }, // デバッグコードをバイナリに含めるかどうか
        { "SERVER_CODE",     null }, // サーバコードをバイナリに含めるかどうか
        { "STANDALONE_MODE", null }, // スタンドアローンモード
        //{ "ACCESS_SERVER", new string[] {"DEVELOP", "STAGING", "RELEASE"}},
    };

    //-------------------------------------------------------------------------- 変数
    public string                  gameConfigurationName        = "Game Configuration Name";
    public string                  gameConfigurationDescription = "Game Configuration Description";
    public BuildTarget             buildTarget                  = BuildTarget.StandaloneWindows64;
    public string                  outputPath                   = "Builds/DebugLocal";
    public bool                    headless                     = false;
    public bool                    autoRun                      = true;
    public bool                    openFolder                   = false;
    public ResolutionDialogSetting resolutionDialogSetting      = ResolutionDialogSetting.Disabled;
    public int                     screenWidth                  = 1280;
    public int                     screenHeight                 = 720;
    public bool                    isFullScreen                 = false;
    public bool                    runInBackground              = true;
    public bool                    showSplashScreen             = false;
    public bool                    developmentBuild             = false;
    public string                  localServerStartUrl          = null;
    public string                  localServerStopUrl           = null;
    public List<string>            scriptingDefineSymbols       = new List<string>();
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// インポートとエクスポートと設定の適用
public partial class GameConfiguration {
    //-------------------------------------------------------------------------- 定義
    // ゲーム構成ファイルパス
    static readonly string GAME_CONFIGURATION_JSON_PATH = "Assets/GameConfiguration.json";
    // ゲーム設定ファイルパス
    static readonly string GAME_SETTINGS_ASSET_PATH = "Assets/Game/Resources/GameSettings.asset";
    // mcs.rsp ファイルパス
    static readonly string MCS_RSP_PATH = "Assets/mcs.rsp";

    //-------------------------------------------------------------------------- 操作
    // ロード
    public static GameConfiguration Load(bool createGameConfiguration = false) {
        var gameConfiguration = ReadJsonFile<GameConfiguration>(GAME_CONFIGURATION_JSON_PATH);
        var gameSettings      = ReadAssetFile<GameSettingsAsset>(GAME_SETTINGS_ASSET_PATH);
        var mcsSymbols        = ReadRspFile(MCS_RSP_PATH);
        if (gameConfiguration == null || gameSettings == null || mcsSymbols == null) {
            if (!createGameConfiguration) {
                return null;
            }
            if (gameConfiguration == null) {
                gameConfiguration = new GameConfiguration();
                if (GameConfigurationList.Count > 0) {
                    gameConfiguration.Assign(GameConfigurationList[0]);
                }
            }
            gameConfiguration.Save();
        }
        return gameConfiguration;
    }

    // セーブ
    public void Save(bool recompile = false) {
        var gameSettingsAsset = ScriptableObject.CreateInstance<GameSettingsAsset>();
        JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(this), gameSettingsAsset.gameSettings);
        WriteJsonFile(GAME_CONFIGURATION_JSON_PATH, this);
        WriteAssetFile<GameSettingsAsset>(GAME_SETTINGS_ASSET_PATH, gameSettingsAsset);
        WriteRspFile(MCS_RSP_PATH, this.scriptingDefineSymbols);
        if (recompile) {
            // NOTE
            // 強制的に再コンパイルを走らせる。
            // EditorAPI から再コンパイルできた気がするが
            // 忘れたのでひとまずこれで対応。
            var buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            var scriptingDefineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
            PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, scriptingDefineSymbols + ";REBUILD");
            PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, scriptingDefineSymbols);
        }
        AssetDatabase.Refresh();
    }

    // ゲーム構成のクリーンアップ
    public static void CleanUp() {
        RemoveGameConfigurationFiles();
        AssetDatabase.Refresh();
    }

    // 別のゲーム構成をアサイン
    public void Assign(GameConfiguration gameConfiguration) {
        var jsonText = JsonUtility.ToJson(gameConfiguration);
        JsonUtility.FromJsonOverwrite(jsonText, this);
    }

    // ゲーム構成をアサイン
    public void Assign(string gameConfigurationName) {
        var gameConfiguration = Find(gameConfigurationName);
        if (gameConfiguration == null) {
            Debug.LogErrorFormat("GameConfiguration: ゲーム構成なし ({0})", gameConfigurationName);
            return;
        }
        Assign(gameConfiguration);
    }

    // ゲーム構成を探す
    public static GameConfiguration Find(string gameConfigurationName) {
        foreach (var gameConfiguration in GameConfigurationList) {
            if (gameConfiguration.gameConfigurationName == gameConfigurationName) {
                return gameConfiguration;
            }
        }
        return null;
    }

    //-------------------------------------------------------------------------- ファイルの読み書き
    // asset ファイルの読み込み
    static TScriptableObject ReadAssetFile<TScriptableObject>(string path) where TScriptableObject : ScriptableObject {
        if (File.Exists(path)) {
            var scriptableObject = ScriptableObject.CreateInstance<TScriptableObject>();
            var savedScriptableObject = AssetDatabase.LoadAssetAtPath<TScriptableObject>(path);
            if (savedScriptableObject != null) {
                EditorUtility.CopySerialized(savedScriptableObject, scriptableObject);
            }
            return scriptableObject;
        }
        return null;
    }

    // asset ファイルの書き込み
    static void WriteAssetFile<TScriptableObject>(string path, TScriptableObject scriptableObject) where TScriptableObject : ScriptableObject {
        if (File.Exists(path)) {
            var savedScriptableObject = AssetDatabase.LoadAssetAtPath<TScriptableObject>(path);
            EditorUtility.CopySerialized(scriptableObject, savedScriptableObject);
            EditorUtility.SetDirty(savedScriptableObject);
            AssetDatabase.SaveAssets();
        } else {
            var newScriptableObject = ScriptableObject.CreateInstance<TScriptableObject>();
            EditorUtility.CopySerialized(scriptableObject, newScriptableObject);
            AssetDatabase.CreateAsset(newScriptableObject, path);
        }
        AssetDatabase.Refresh();
    }

    // json ファイルの読み込み
    static TObject ReadJsonFile<TObject>(string path) {
        var text = ReadFile(path);
        if (text == null) {
            return default(TObject);
        }
        try {
            return JsonUtility.FromJson<TObject>(text);
        } catch {}
        return default(TObject);
    }

    // json ファイルの書き込み
    static void WriteJsonFile(string path, object obj) {
        WriteFile(path, JsonUtility.ToJson(obj, true));
    }

    // rsp ファイルの読み込み
    static List<string> ReadRspFile(string path) {
        var text = ReadFile(path);
        if (text == null) {
            return null;
        }
        var symbols = new List<string>();
        var lines   = text.Split('\n');//new string[] {Environment.NewLine}, StringSplitOptions.None);
        foreach (var line in lines) {
            var str = line.Trim();
            if (str.StartsWith("-define:")) {
                symbols.Add(str.Substring("-define:".Length));
            }
        }
        return symbols;
    }

    // rsp ファイルの書き込み
    static void WriteRspFile(string path, List<string> symbols) {
        var text = "";
        foreach (var symbol in symbols) {
            text = text + "-define:" + symbol + "\n";//Environment.NewLine;
        }
        WriteFile(path, text);
    }

    // ゲーム設定ファイルの削除
    static void RemoveGameConfigurationFiles() {
        RemoveFile(GAME_CONFIGURATION_JSON_PATH);
        RemoveFile(GAME_SETTINGS_ASSET_PATH);
        RemoveFile(MCS_RSP_PATH);
    }

    //-------------------------------------------------------------------------- ファイルの読み書き
    // ファイルの読み込み
    static string ReadFile(string path) {
        try {
            using (var sr = new StreamReader(path, Encoding.UTF8)) {
                return sr.ReadToEnd();
            }
        } catch (Exception e) {
            Debug.LogError(e);
        }
        return null;
    }

    // ファイルの書き込み
    static void WriteFile(string path, string text, int count = 0) {
        try {
            using (var sw = new StreamWriter(path, false, Encoding.UTF8)) {
                sw.Write(text);
            }
        } catch (Exception e) {
            if (e is IOException) {
                if (count < 3) { // NOTE ビルド後に IOException が起こっているので数回リトライして回避
                    EditorApplication.delayCall += () =>  WriteFile(path, text, ++count);
                    return;
                }
            }
            Debug.LogError(e);
        }
    }

    // ファイルの削除
    static void RemoveFile(string path) {
        if (File.Exists(path)) {
            File.Delete(path);
        }
    }
}

//////////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////////

// バックアップと復元
public partial class GameConfiguration {
    //-------------------------------------------------------------------------- 変数
    static GameConfiguration gameConfigurationBackup = null;

    //-------------------------------------------------------------------------- 操作
    // 現在のゲーム設定をバックアップする
    public static void Backup() {
        Debug.Assert(gameConfigurationBackup == null, "既にバックアップしている");
        gameConfigurationBackup = GameConfiguration.Load(false);
    }

    // バックアップしたゲーム設定を元に戻す
    public static void Restore() {
        Debug.Assert(gameConfigurationBackup != null, "バックアップがないので復元不能");
        var gameConfiguration = gameConfigurationBackup;
        gameConfigurationBackup = null;
        if (gameConfiguration == null) {
            GameConfiguration.CleanUp(); // NOTE 元々無かった場合は消す
            return;
        }
        gameConfiguration.Save(false);
    }
}
