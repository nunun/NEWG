#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections.Generic;

// ゲーム設定
[Serializable]
public partial class GameSettings {
    //-------------------------------------------------------------------------- 定義
    // ゲーム設定一覧の定義
    // NOTE 追加のゲーム設定をここに定義して下さい。
    public static readonly List<GameSettings> GameSettingsList = new List<GameSettings>() {
        new GameSettings() {
            gameSettingsName             = "DEFAULT",
            gameSettingsDescription      = "開発用デフォルト設定",
            runtimeServiceMode           = GameManager.ServiceMode.Host,
            serverAddress                = "localhost",
            serverPort                   = 7778,
            serverPortRandomRange        = 0,
            serverDiscoveryAddress       = "localhost",
            serverDiscoveryPort          = 7778,
            webapiUrl                    = "http://localhost:7780",
            mindlinkUrl                  = "http://localhost:7766",
            buildOutputPath              = "Builds/DebugHost",
            buildTarget                  = BuildTarget.StandaloneWindows64,
            buildHeadless                = false,
            buildAutoRun                 = true,
            buildOpenFolder              = false,
            buildResolutionDialogSetting = ResolutionDialogSetting.Disabled;
            buildScreenWidth             = 1280,
            buildScreenHeight            = 720,
            buildIsFullScreen            = false,
            buildRunInBackground         = true,
            buildShowSplashScreen        = false,
            buildScriptingDefineSymbols  = new List<string>() {"DEBUG", "SERVER_CODE"},
        },
        new GameSettings() {
            gameSettingsName             = "DEBUG_CLIENT",
            gameSettingsDescription      = "デバッグ用クライアント",
            runtimeServiceMode           = GameManager.ServiceMode.Client,
            serverAddress                = null,
            serverPort                   = 0,
            serverPortRandomRange        = 0,
            serverDiscoveryAddress       = null,
            serverDiscoveryPort          = 0,
            webapiUrl                    = "http://localhost:7780",
            mindlinkUrl                  = null,
            buildOutputPath              = "Builds/DebugClient",
            buildTarget                  = BuildTarget.WebGL,
            buildHeadless                = false,
            buildAutoRun                 = true,
            buildOpenFolder              = false,
            buildResolutionDialogSetting = ResolutionDialogSetting.Disabled;
            buildScreenWidth             = 1280,
            buildScreenHeight            = 720,
            buildIsFullScreen            = false,
            buildRunInBackground         = true,
            buildShowSplashScreen        = false,
            buildScriptingDefineSymbols  = new List<string>() {"DEBUG"},
        },
        new GameSettings() {
            gameSettingsName             = "DEBUG_SERVER",
            gameSettingsDescription      = "デバッグ用サーバ",
            runtimeServiceMode           = GameManager.ServiceMode.Server,
            serverAddress                = "0.0.0.0",
            serverPort                   = 7777,
            serverPortRandomRange        = 0,
            serverDiscoveryAddress       = "localhost",
            serverDiscoveryPort          = 7777,
            webapiUrl                    = "http://api:7780",
            mindlinkUrl                  = "http://mindlink:7766",
            buildOutputPath              = "Builds/DebugServer",
            buildTarget                  = BuildTarget.StandaloneWindows64,
            buildHeadless                = false,
            buildAutoRun                 = true,
            buildOpenFolder              = false,
            buildResolutionDialogSetting = ResolutionDialogSetting.Disabled;
            buildScreenWidth             = 1280,
            buildScreenHeight            = 720,
            buildIsFullScreen            = false,
            buildRunInBackground         = true,
            buildShowSplashScreen        = false,
            buildScriptingDefineSymbols  = new List<string>() {"DEBUG", "SERVER_CODE"},
        },
        new GameSettings() {
            gameSettingsName             = "RELEASE_CLIENT",
            gameSettingsDescription      = "リリース用クライアント",
            runtimeServiceMode           = GameManager.ServiceMode.Client,
            serverAddress                = null,
            serverPort                   = 0,
            serverPortRandomRange        = 0,
            serverDiscoveryAddress       = null,
            serverDiscoveryPort          = 0,
            webapiUrl                    = "http://fu-n.net:7780",
            mindlinkUrl                  = null,
            buildOutputPath              = "Builds/ReleaseClient",
            buildTarget                  = BuildTarget.WebGL,
            buildHeadless                = false,
            buildAutoRun                 = false,
            buildOpenFolder              = true,
            buildResolutionDialogSetting = ResolutionDialogSetting.Disabled;
            buildScreenWidth             = 1280,
            buildScreenHeight            = 720,
            buildIsFullScreen            = false,
            buildRunInBackground         = true,
            buildShowSplashScreen        = false,
            buildScriptingDefineSymbols  = new List<string>(),
        },
        new GameSettings() {
            gameSettingsName             = "RELEASE_SERVER",
            gameSettingsDescription      = "リリース用サーバ",
            runtimeServiceMode           = GameManager.ServiceMode.Client,
            serverAddress                = "0.0.0.0",
            serverPort                   = 8000,
            serverPortRandomRange        = 1000,
            serverDiscoveryAddress       = "fu-n.net",
            serverDiscoveryPort          = 8000,
            webapiUrl                    = "http://localhost:7780",
            mindlinkUrl                  = "http://localhost:7766",
            buildOutputPath              = "Builds/ReleaseClient",
            buildTarget                  = BuildTarget.StandaloneLinuxUniversal,
            buildHeadless                = true,
            buildAutoRun                 = false,
            buildOpenFolder              = true,
            buildResolutionDialogSetting = ResolutionDialogSetting.Disabled;
            buildScreenWidth             = 1280,
            buildScreenHeight            = 720,
            buildIsFullScreen            = false,
            buildRunInBackground         = true,
            buildShowSplashScreen        = false,
            buildScriptingDefineSymbols  = new List<string>() {"SERVER_CODE"},
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
    public string                  gameSettingsName             = "Game Settings Name";
    public string                  gameSettingsDescription      = "Game Settings Description";
    public GameManager.ServiceMode runtimeServiceMode           = GameManager.ServiceMode.Host;
    public string                  serverAddress                = "localhost";
    public int                     serverPort                   = 7777;
    public int                     serverPortRandomRange        = 0;
    public string                  serverDiscoveryAddress       = "localhost";
    public int                     serverDiscoveryPort          = 7777;
    public string                  webapiUrl                    = "http://localhost:7780";
    public string                  mindlinkUrl                  = "http://localhost:7766";
    public string                  buildOutputPath              = "Builds/DebugLocal";
    public BuildTarget             buildTarget                  = BuildTarget.StandaloneWindows64;
    public bool                    buildHeadless                = false;
    public bool                    buildAutoRun                 = true;
    public bool                    buildOpenFolder              = false;
    public ResolutionDialogSetting buildResolutionDialogSetting = ResolutionDialogSetting.Disabled;
    public int                     buildScreenWidth             = 1280;
    public int                     buildScreenHeight            = 720;
    public bool                    buildIsFullScreen            = false;
    public bool                    buildRunInBackground         = true;
    public bool                    buildShowSplashScreen        = false;
    public List<string>            buildScriptingDefineSymbols  = new List<string>();
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// インポートとエクスポートと設定の適用
public partial class GameSettings {
    //-------------------------------------------------------------------------- 定義
    // ゲーム設定ファイルパス
    public static readonly string GAME_SETTINGS_JSON_PATH = "Assets/GameSettings.json";
    // mcs.rsp ファイルパス
    public static readonly string MCS_RSP_PATH = "Assets/mcs.rsp";

    //-------------------------------------------------------------------------- 操作
    // ロード
    public static GameSettings Load(bool createDefaultGameSettings = false) {
        var gameSettings = ReadJsonFile(GAME_SETTINGS_JSON_PATH);
        var mcsSymbols   = ReadRspFile(MCS_RSP_PATH);
        if (gameSettings == null || mcsSymbols == null) {
            if (!createDefaultGameSettings) {
                return null;
            }
            if (gameSettings == null) {
                gameSettings = new GameSettings();
                if (GameSettingsList.Count > 0) {
                    gameSettings.Assign(GameSettingsList[0]);
                }
            }
            gameSettings.Save();
        }
        return gameSettings;
    }

    // セーブ
    public void Save(bool recompile = false) {
        WriteJsonFile(GAME_SETTINGS_JSON_PATH, this);
        WriteRspFile(MCS_RSP_PATH, this.buildScriptingDefineSymbols);

        // 再コンパイルあり？
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

        // アセットをリフレッシュ
        AssetDatabase.Refresh();
    }

    // 別のゲーム設定をアサイン
    public void Assign(GameSettings gameSettings) {
        var jsonText = JsonUtility.ToJson(gameSettings);
        JsonUtility.FromJsonOverwrite(jsonText, this);
    }

    // ゲーム設定をアサイン
    public void Assign(string gameSettingsName) {
        var gameSettings = Find(gameSettingsName);
        if (gameSettings == null) {
            Debug.LogErrorFormat("GameSettings: ゲーム設定なし ({0})", gameSettingsName);
            return;
        }
        Assign(gameSettings);
    }

    // オブジェクトにオーバーライド
    public void Overwrite(object obj) {
        var jsonText = JsonUtility.ToJson(this);
        JsonUtility.FromJsonOverwrite(jsonText, obj);
    }

    // ゲーム設定を取得する
    public static GameSettings Find(string gameSettingsName) {
        foreach (var gameSettings in GameSettingsList) {
            if (gameSettings.gameSettingsName == gameSettingsName) {
                return gameSettings;
            }
        }
        return null;
    }

    // ゲーム設定の削除
    public static void Remove() {
        RemoveGameSettingsFiles();
    }

    //-------------------------------------------------------------------------- ファイルの読み書き
    // json ファイルの読み込み
    static GameSettings ReadJsonFile(string path) {
        var text = ReadFile(path);
        if (text == null) {
            return null;
        }
        try {
            return JsonUtility.FromJson<GameSettings>(text);
        } catch {}
        return null;
    }

    // json ファイルの書き込み
    static void WriteJsonFile(string path, GameSettings gameSettings) {
        WriteFile(path, JsonUtility.ToJson(gameSettings, true));
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

    // ゲーム設定の削除
    static void RemoveGameSettingsFiles() {
        RemoveFile(GAME_SETTINGS_JSON_PATH);
        RemoveFile(MCS_RSP_PATH);
        AssetDatabase.Refresh();
    }

    // ファイルの読み込み
    static string ReadFile(string path) {
        try {
            using (var sr = new StreamReader(path, Encoding.UTF8)) {
                return sr.ReadToEnd();
            }
        } catch {}
        return null;
    }

    // ファイルの書き込み
    static void WriteFile(string path, string text) {
        using (var sw = new StreamWriter(path, false, Encoding.UTF8)) {
            sw.Write(text);
        }
    }

    // ファイルの削除
    static void RemoveFile(string path) {
        if (File.Exists(path)) {
            File.Delete(path);
        }
    }
}
#endif
