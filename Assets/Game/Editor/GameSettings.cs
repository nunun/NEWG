using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections.Generic;

// ゲーム設定
[Serializable]
public partial class GameSettings : GameSettingsManager.RuntimeGameSettings {
    //-------------------------------------------------------------------------- 定義
    // ゲーム設定一覧の定義
    // NOTE 追加のゲーム設定をここに定義して下さい。
    public static readonly List<GameSettings> GameSettingsList = new List<GameSettings>() {
        new GameSettings() {
            gameSettingsName        = "DEFAULT",
            gameSettingsDescription = "開発用デフォルト設定",
            runtimeServiceMode      = GameSettingsManager.ServiceMode.Host,
            serverAddress           = "localhost",
            serverPort              = 7778,
            serverPortRandomRange   = 0,
            serverDiscoveryAddress  = "localhost",
            serverDiscoveryPort     = 7778,
            webapiUrl               = "http://localhost:7780",
            mindlinkUrl             = "http://localhost:7766",
            outputPath              = "Builds/DebugHost",
            buildTarget             = BuildTarget.StandaloneWindows64,
            headless                = false,
            autoRun                 = true,
            openFolder              = false,
            resolutionDialogSetting = ResolutionDialogSetting.Disabled,
            screenWidth             = 1280,
            screenHeight            = 720,
            isFullScreen            = false,
            runInBackground         = true,
            showSplashScreen        = false,
            scriptingDefineSymbols  = new List<string>() {"DEBUG", "SERVER_CODE"},
        },
        new GameSettings() {
            gameSettingsName        = "DEBUG_CLIENT",
            gameSettingsDescription = "デバッグ用クライアント",
            runtimeServiceMode      = GameSettingsManager.ServiceMode.Client,
            serverAddress           = null,
            serverPort              = 0,
            serverPortRandomRange   = 0,
            serverDiscoveryAddress  = null,
            serverDiscoveryPort     = 0,
            webapiUrl               = "http://localhost:7780",
            mindlinkUrl             = null,
            outputPath              = "Builds/DebugClient",
            buildTarget             = BuildTarget.WebGL,
            headless                = false,
            autoRun                 = true,
            openFolder              = false,
            resolutionDialogSetting = ResolutionDialogSetting.Disabled,
            screenWidth             = 1280,
            screenHeight            = 720,
            isFullScreen            = false,
            runInBackground         = true,
            showSplashScreen        = false,
            scriptingDefineSymbols  = new List<string>() {"DEBUG"},
        },
        new GameSettings() {
            gameSettingsName        = "DEBUG_SERVER",
            gameSettingsDescription = "デバッグ用サーバ",
            runtimeServiceMode      = GameSettingsManager.ServiceMode.Server,
            serverAddress           = "0.0.0.0",
            serverPort              = 7777,
            serverPortRandomRange   = 0,
            serverDiscoveryAddress  = "localhost",
            serverDiscoveryPort     = 7777,
            webapiUrl               = "http://api:7780",
            mindlinkUrl             = "http://mindlink:7766",
            outputPath              = "Builds/DebugServer",
            buildTarget             = BuildTarget.StandaloneWindows64,
            headless                = false,
            autoRun                 = true,
            openFolder              = false,
            resolutionDialogSetting = ResolutionDialogSetting.Disabled,
            screenWidth             = 1280,
            screenHeight            = 720,
            isFullScreen            = false,
            runInBackground         = true,
            showSplashScreen        = false,
            scriptingDefineSymbols  = new List<string>() {"DEBUG", "SERVER_CODE"},
        },
        new GameSettings() {
            gameSettingsName        = "RELEASE_CLIENT",
            gameSettingsDescription = "リリース用クライアント",
            runtimeServiceMode      = GameSettingsManager.ServiceMode.Client,
            serverAddress           = null,
            serverPort              = 0,
            serverPortRandomRange   = 0,
            serverDiscoveryAddress  = null,
            serverDiscoveryPort     = 0,
            webapiUrl               = "http://fu-n.net:7780",
            mindlinkUrl             = null,
            outputPath              = "Builds/ReleaseClient",
            buildTarget             = BuildTarget.WebGL,
            headless                = false,
            autoRun                 = false,
            openFolder              = true,
            resolutionDialogSetting = ResolutionDialogSetting.Disabled,
            screenWidth             = 1280,
            screenHeight            = 720,
            isFullScreen            = false,
            runInBackground         = true,
            showSplashScreen        = false,
            scriptingDefineSymbols  = new List<string>(),
        },
        new GameSettings() {
            gameSettingsName        = "RELEASE_SERVER",
            gameSettingsDescription = "リリース用サーバ",
            runtimeServiceMode      = GameSettingsManager.ServiceMode.Client,
            serverAddress           = "0.0.0.0",
            serverPort              = 8000,
            serverPortRandomRange   = 1000,
            serverDiscoveryAddress  = "fu-n.net",
            serverDiscoveryPort     = 8000,
            webapiUrl               = "http://localhost:7780",
            mindlinkUrl             = "http://localhost:7766",
            outputPath              = "Builds/ReleaseClient",
            buildTarget             = BuildTarget.StandaloneLinuxUniversal,
            headless                = true,
            autoRun                 = false,
            openFolder              = true,
            resolutionDialogSetting = ResolutionDialogSetting.Disabled,
            screenWidth             = 1280,
            screenHeight            = 720,
            isFullScreen            = false,
            runInBackground         = true,
            showSplashScreen        = false,
            scriptingDefineSymbols  = new List<string>() {"SERVER_CODE"},
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
    public string                  gameSettingsName        = "Game Settings Name";
    public string                  gameSettingsDescription = "Game Settings Description";
    public string                  outputPath              = "Builds/DebugLocal";
    public BuildTarget             buildTarget             = BuildTarget.StandaloneWindows64;
    public bool                    headless                = false;
    public bool                    autoRun                 = true;
    public bool                    openFolder              = false;
    public ResolutionDialogSetting resolutionDialogSetting = ResolutionDialogSetting.Disabled;
    public int                     screenWidth             = 1280;
    public int                     screenHeight            = 720;
    public bool                    isFullScreen            = false;
    public bool                    runInBackground         = true;
    public bool                    showSplashScreen        = false;
    public List<string>            scriptingDefineSymbols  = new List<string>();
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// インポートとエクスポートと設定の適用
public partial class GameSettings {
    //-------------------------------------------------------------------------- 定義
    // ゲーム設定ファイルパス
    public static readonly string GAME_SETTINGS_JSON_PATH = GameManager.GAME_SETTINGS_JSON_PATH;
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

    // ゲーム設定のクリーンアップ
    public static void CleanUp() {
        RemoveGameSettingsFiles();
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

    // ゲーム設定を取得する
    public static GameSettings Find(string gameSettingsName) {
        foreach (var gameSettings in GameSettingsList) {
            if (gameSettings.gameSettingsName == gameSettingsName) {
                return gameSettings;
            }
        }
        return null;
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

    // ゲーム設定ファイルの削除
    static void RemoveGameSettingsFiles() {
        RemoveFile(GAME_SETTINGS_JSON_PATH);
        RemoveFile(MCS_RSP_PATH);
    }

    //-------------------------------------------------------------------------- ファイルの読み書き
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

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// バックアップと復元
public partial class GameSettings {
    //-------------------------------------------------------------------------- 変数
    static GameSettings gameSettingsBackup = null;

    //-------------------------------------------------------------------------- 操作
    // 現在のゲーム設定をバックアップする
    public static void Backup() {
        Debug.Assert(gameSettingsBackup != null, "既にバックアップしている");
        gameSettingsBackup = GameSettings.Load(false);
    }

    // バックアップしたゲーム設定を元に戻す
    public static void Restore() {
        Debug.Assert(gameSettingsBackup == null, "バックアップがないので復元不能");
        var gameSettings = gameSettingsBackup;
        gameSettingsBackup = null;
        if (gameSettings == null) {
            GameSettings.CleanUp(); // NOTE 元々無かった場合は消す
            return;
        }
        gameSettings.Save(false);
    }
}
