using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections.Generic;

// ゲーム設定ウィンドウ
public partial class GameSettingsWindow : EditorWindow {
    //-------------------------------------------------------------------------- 定義 (プロファイル一覧)
    public static List<GameProfile> gameProfiles = new List<GameProfile>() {
        new GameProfile() {
            name                   = "Test Profile",
            description            = "Test Description",
            gameSettings           = Test(),
            scriptingDefineSymbols = new List<string>(),
        }
    };

    //-------------------------------------------------------------------------- 定義 (プロファイル)
    [Serializable]
    public class GameProfile {
        public string       name                   = "Game Profile Name";
        public string       description            = "Game Profile Description";
        public GameSettings gameSettings           = null;
        public List<string> scriptingDefineSymbols = new List<string>();
    }

    //-------------------------------------------------------------------------- ユーティリティ
    static GameSettings Test() {
        return ScriptableObject.CreateInstance<GameSettings>();
    }
}

//using UnityEngine;
//using UnityEditor;
//using System;
//using System.IO;
//using System.Text;
//using System.Linq;
//using System.Collections.Generic;
//
//// ゲーム設定
//[Serializable]
//public partial class GameSettings : GameSettingsManager.RuntimeGameSettings {
//    //-------------------------------------------------------------------------- 定義
//    // ゲーム設定一覧の定義
//    // NOTE 追加のゲーム設定をここに定義して下さい。
//    public static readonly List<GameSettings> GameSettingsList = new List<GameSettings>() {
//        new GameSettings() {
//            gameSettingsName        = "DEFAULT",
//            gameSettingsDescription = "開発用デフォルト設定",
//            runtimeServiceMode      = GameSettingsManager.ServiceMode.Host,
//            serverAddress           = "localhost",
//            serverPort              = 7778,
//            serverPortRandomRange   = 0,
//            serverDiscoveryAddress  = "localhost",
//            serverDiscoveryPort     = 7778,
//            webapiUrl               = "http://localhost:7780",
//            mindlinkUrl             = "ws://localhost:7766",
//            outputPath              = "Builds/DebugHost",
//            buildTarget             = BuildTarget.StandaloneWindows64,
//            headless                = false,
//            autoRun                 = true,
//            openFolder              = false,
//            resolutionDialogSetting = ResolutionDialogSetting.Disabled,
//            screenWidth             = 1280,
//            screenHeight            = 720,
//            isFullScreen            = false,
//            runInBackground         = true,
//            showSplashScreen        = false,
//            scriptingDefineSymbols  = new List<string>() {"DEBUG", "SERVER_CODE"},
//        },
//        new GameSettings() {
//            gameSettingsName        = "DEBUG_CLIENT",
//            gameSettingsDescription = "デバッグ用クライアント",
//            runtimeServiceMode      = GameSettingsManager.ServiceMode.Client,
//            serverAddress           = null,
//            serverPort              = 0,
//            serverPortRandomRange   = 0,
//            serverDiscoveryAddress  = null,
//            serverDiscoveryPort     = 0,
//            webapiUrl               = "http://localhost:7780",
//            mindlinkUrl             = null,
//            outputPath              = "Builds/DebugClient",
//            buildTarget             = BuildTarget.WebGL,
//            headless                = false,
//            autoRun                 = true,
//            openFolder              = false,
//            resolutionDialogSetting = ResolutionDialogSetting.Disabled,
//            screenWidth             = 1280,
//            screenHeight            = 720,
//            isFullScreen            = false,
//            runInBackground         = true,
//            showSplashScreen        = false,
//            scriptingDefineSymbols  = new List<string>() {"DEBUG"},
//        },
//        new GameSettings() {
//            gameSettingsName        = "DEBUG_SERVER",
//            gameSettingsDescription = "デバッグ用サーバ",
//            runtimeServiceMode      = GameSettingsManager.ServiceMode.Server,
//            serverAddress           = "0.0.0.0",
//            serverPort              = 7777,
//            serverPortRandomRange   = 0,
//            serverDiscoveryAddress  = "localhost",
//            serverDiscoveryPort     = 7777,
//            webapiUrl               = "http://api:7780",
//            mindlinkUrl             = "ws://mindlink:7766",
//            outputPath              = "Builds/DebugServer",
//            buildTarget             = BuildTarget.StandaloneWindows64,
//            headless                = false,
//            autoRun                 = true,
//            openFolder              = false,
//            resolutionDialogSetting = ResolutionDialogSetting.Disabled,
//            screenWidth             = 1280,
//            screenHeight            = 720,
//            isFullScreen            = false,
//            runInBackground         = true,
//            showSplashScreen        = false,
//            scriptingDefineSymbols  = new List<string>() {"DEBUG", "SERVER_CODE"},
//        },
//        new GameSettings() {
//            gameSettingsName        = "RELEASE_CLIENT",
//            gameSettingsDescription = "リリース用クライアント",
//            runtimeServiceMode      = GameSettingsManager.ServiceMode.Client,
//            serverAddress           = null,
//            serverPort              = 0,
//            serverPortRandomRange   = 0,
//            serverDiscoveryAddress  = null,
//            serverDiscoveryPort     = 0,
//            webapiUrl               = "http://fu-n.net:7780",
//            mindlinkUrl             = null,
//            outputPath              = "Builds/ReleaseClient",
//            buildTarget             = BuildTarget.WebGL,
//            headless                = false,
//            autoRun                 = false,
//            openFolder              = true,
//            resolutionDialogSetting = ResolutionDialogSetting.Disabled,
//            screenWidth             = 1280,
//            screenHeight            = 720,
//            isFullScreen            = false,
//            runInBackground         = true,
//            showSplashScreen        = false,
//            scriptingDefineSymbols  = new List<string>(),
//        },
//        new GameSettings() {
//            gameSettingsName        = "RELEASE_SERVER",
//            gameSettingsDescription = "リリース用サーバ",
//            runtimeServiceMode      = GameSettingsManager.ServiceMode.Client,
//            serverAddress           = "0.0.0.0",
//            serverPort              = 8000,
//            serverPortRandomRange   = 1000,
//            serverDiscoveryAddress  = "fu-n.net",
//            serverDiscoveryPort     = 8000,
//            webapiUrl               = "http://localhost:7780",
//            mindlinkUrl             = "ws://localhost:7766",
//            outputPath              = "Builds/ReleaseClient",
//            buildTarget             = BuildTarget.StandaloneLinuxUniversal,
//            headless                = true,
//            autoRun                 = false,
//            openFolder              = true,
//            resolutionDialogSetting = ResolutionDialogSetting.Disabled,
//            screenWidth             = 1280,
//            screenHeight            = 720,
//            isFullScreen            = false,
//            runInBackground         = true,
//            showSplashScreen        = false,
//            scriptingDefineSymbols  = new List<string>() {"SERVER_CODE"},
//        },
//    };
//
//    // シンボル一覧の定義
//    // NOTE 追加のシンボルをここに定義してください。
//    public static readonly Dictionary<string,string[]> ScriptingDefineSymbols = new Dictionary<string,string[]>() {
//        { "DEBUG",           null }, // デバッグコードをバイナリに含めるかどうか
//        { "SERVER_CODE",     null }, // サーバコードをバイナリに含めるかどうか
//        { "STANDALONE_MODE", null }, // スタンドアローンモード
//        //{ "ACCESS_SERVER", new string[] {"DEVELOP", "STAGING", "RELEASE"}},
//    };
//
//    //-------------------------------------------------------------------------- 変数
//    public string                  gameSettingsName        = "Game Settings Name";
//    public string                  gameSettingsDescription = "Game Settings Description";
//    public string                  outputPath              = "Builds/DebugLocal";
//    public BuildTarget             buildTarget             = BuildTarget.StandaloneWindows64;
//    public bool                    headless                = false;
//    public bool                    autoRun                 = true;
//    public bool                    openFolder              = false;
//    public ResolutionDialogSetting resolutionDialogSetting = ResolutionDialogSetting.Disabled;
//    public int                     screenWidth             = 1280;
//    public int                     screenHeight            = 720;
//    public bool                    isFullScreen            = false;
//    public bool                    runInBackground         = true;
//    public bool                    showSplashScreen        = false;
//    public List<string>            scriptingDefineSymbols  = new List<string>();
//}
//
//////////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////////
//
//// インポートとエクスポートと設定の適用
//public partial class GameSettings {
//    //-------------------------------------------------------------------------- 定義
//    // ゲーム設定ファイルパス
//    public static readonly string GAME_SETTINGS_JSON_PATH = GameSettingsManager.GAME_SETTINGS_JSON_PATH;
//    // mcs.rsp ファイルパス
//    public static readonly string MCS_RSP_PATH = "Assets/mcs.rsp";
//
//    //-------------------------------------------------------------------------- 操作
//    // ロード
//    public static GameSettings Load(bool createDefaultGameSettings = false) {
//        var gameSettings = ReadJsonFile(GAME_SETTINGS_JSON_PATH);
//        var mcsSymbols   = ReadRspFile(MCS_RSP_PATH);
//        if (gameSettings == null || mcsSymbols == null) {
//            if (!createDefaultGameSettings) {
//                return null;
//            }
//            if (gameSettings == null) {
//                gameSettings = new GameSettings();
//                if (GameSettingsList.Count > 0) {
//                    gameSettings.Assign(GameSettingsList[0]);
//                }
//            }
//            gameSettings.Save();
//        }
//        return gameSettings;
//    }
//
//    // セーブ
//    public void Save(bool recompile = false) {
//        WriteJsonFile(GAME_SETTINGS_JSON_PATH, this);
//        WriteRspFile(MCS_RSP_PATH, this.scriptingDefineSymbols);
//        if (recompile) {
//            // NOTE
//            // 強制的に再コンパイルを走らせる。
//            // EditorAPI から再コンパイルできた気がするが
//            // 忘れたのでひとまずこれで対応。
//            var buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
//            var scriptingDefineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
//            PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, scriptingDefineSymbols + ";REBUILD");
//            PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, scriptingDefineSymbols);
//        }
//        AssetDatabase.Refresh();
//    }
//
//    // ゲーム設定のクリーンアップ
//    public static void CleanUp() {
//        RemoveGameSettingsFiles();
//        AssetDatabase.Refresh();
//    }
//
//    // 別のゲーム設定をアサイン
//    public void Assign(GameSettings gameSettings) {
//        var jsonText = JsonUtility.ToJson(gameSettings);
//        JsonUtility.FromJsonOverwrite(jsonText, this);
//    }
//
//    // ゲーム設定をアサイン
//    public void Assign(string gameSettingsName) {
//        var gameSettings = Find(gameSettingsName);
//        if (gameSettings == null) {
//            Debug.LogErrorFormat("GameSettings: ゲーム設定なし ({0})", gameSettingsName);
//            return;
//        }
//        Assign(gameSettings);
//    }
//
//    // ゲーム設定を取得する
//    public static GameSettings Find(string gameSettingsName) {
//        foreach (var gameSettings in GameSettingsList) {
//            if (gameSettings.gameSettingsName == gameSettingsName) {
//                return gameSettings;
//            }
//        }
//        return null;
//    }
//
//    //-------------------------------------------------------------------------- ファイルの読み書き
//    // json ファイルの読み込み
//    static GameSettings ReadJsonFile(string path) {
//        var text = ReadFile(path);
//        if (text == null) {
//            return null;
//        }
//        try {
//            return JsonUtility.FromJson<GameSettings>(text);
//        } catch {}
//        return null;
//    }
//
//    // json ファイルの書き込み
//    static void WriteJsonFile(string path, GameSettings gameSettings) {
//        WriteFile(path, JsonUtility.ToJson(gameSettings, true));
//    }
//
//    // rsp ファイルの読み込み
//    static List<string> ReadRspFile(string path) {
//        var text = ReadFile(path);
//        if (text == null) {
//            return null;
//        }
//        var symbols = new List<string>();
//        var lines   = text.Split('\n');//new string[] {Environment.NewLine}, StringSplitOptions.None);
//        foreach (var line in lines) {
//            var str = line.Trim();
//            if (str.StartsWith("-define:")) {
//                symbols.Add(str.Substring("-define:".Length));
//            }
//        }
//        return symbols;
//    }
//
//    // rsp ファイルの書き込み
//    static void WriteRspFile(string path, List<string> symbols) {
//        var text = "";
//        foreach (var symbol in symbols) {
//            text = text + "-define:" + symbol + "\n";//Environment.NewLine;
//        }
//        WriteFile(path, text);
//    }
//
//    // ゲーム設定ファイルの削除
//    static void RemoveGameSettingsFiles() {
//        RemoveFile(GAME_SETTINGS_JSON_PATH);
//        RemoveFile(MCS_RSP_PATH);
//    }
//
//    //-------------------------------------------------------------------------- ファイルの読み書き
//    // ファイルの読み込み
//    static string ReadFile(string path) {
//        try {
//            using (var sr = new StreamReader(path, Encoding.UTF8)) {
//                return sr.ReadToEnd();
//            }
//        } catch {}
//        return null;
//    }
//
//    // ファイルの書き込み
//    static void WriteFile(string path, string text) {
//        using (var sw = new StreamWriter(path, false, Encoding.UTF8)) {
//            sw.Write(text);
//        }
//    }
//
//    // ファイルの削除
//    static void RemoveFile(string path) {
//        if (File.Exists(path)) {
//            File.Delete(path);
//        }
//    }
//}
//
//////////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////////
//
//// バックアップと復元
//public partial class GameSettings {
//    //-------------------------------------------------------------------------- 変数
//    static GameSettings gameSettingsBackup = null;
//
//    //-------------------------------------------------------------------------- 操作
//    // 現在のゲーム設定をバックアップする
//    public static void Backup() {
//        Debug.Assert(gameSettingsBackup != null, "既にバックアップしている");
//        gameSettingsBackup = GameSettings.Load(false);
//    }
//
//    // バックアップしたゲーム設定を元に戻す
//    public static void Restore() {
//        Debug.Assert(gameSettingsBackup == null, "バックアップがないので復元不能");
//        var gameSettings = gameSettingsBackup;
//        gameSettingsBackup = null;
//        if (gameSettings == null) {
//            GameSettings.CleanUp(); // NOTE 元々無かった場合は消す
//            return;
//        }
//        gameSettings.Save(false);
//    }
//}
//public partial class GameSettingsWindow : EditorWindow {
//    //-------------------------------------------------------------------------- 定義
//    const string WINDOW_TITLE = "ゲーム設定";
//
//    //-------------------------------------------------------------------------- 変数
//    // ゲーム設定
//    [SerializeField] GameSettings gameSettings = null;
//
//    // ゲーム設定名一覧
//    string[] gameSettingsNames = null;
//
//    // スクロール座標
//    Vector2 scrollPos = Vector2.zero;
//
//    //-------------------------------------------------------------------------- ウインドウ操作
//    // ウインドウを開く
//    public static void Open() {
//        GetWindow<GameSettingsWindow>(true, WINDOW_TITLE);
//    }
//
//    //-------------------------------------------------------------------------- 実装 (EditorWindow)
//    // GUI 表示
//    void OnGUI() {
//        // ゲーム設定名一覧
//        if (gameSettingsNames == null) {
//            gameSettingsNames = GameSettings.GameSettingsList.Select((item) => item.gameSettingsName).ToArray();
//        }
//
//        // ゲーム設定初期化
//        if (gameSettings == null) {
//            gameSettings = GameSettings.Load(true);
//        }
//
//        // シンボル一覧を描画
//        EditorGUILayout.BeginVertical();
//        {
//            GUILayout.Space(5.0f);
//
//            // コントロールボタン
//            EditorGUILayout.BeginHorizontal();
//            {
//                // ゲーム設定選択ボタン
//                var oldIndex = Array.IndexOf(gameSettingsNames, gameSettings.gameSettingsName);
//                var newIndex = EditorGUILayout.Popup(Mathf.Max(0, oldIndex), gameSettingsNames);
//                if (newIndex != oldIndex) {
//                    var gameSettingsName   = gameSettingsNames[newIndex];
//                    var foundGameSettings = GameSettings.Find(gameSettingsName);
//                    if (foundGameSettings != null) {
//                        EditorApplication.delayCall += () => gameSettings.Assign(foundGameSettings);
//                    } else {
//                        Debug.LogErrorFormat("GameSettingsWindow: ゲーム設定なし？ ({0})", gameSettingsName);
//                    }
//                }
//
//                // 保存ボタン
//                if (GUILayout.Button("Save", "MiniButton", GUILayout.ExpandWidth(false))) {
//                    EditorApplication.delayCall += SaveAndApply;
//                }
//            }
//            EditorGUILayout.EndHorizontal();
//
//            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(position.height));
//            {
//                var labelWidth = EditorGUIUtility.labelWidth;
//                EditorGUIUtility.labelWidth = 200.0f;
//                {
//                    DrawGUI();
//                }
//                EditorGUIUtility.labelWidth = labelWidth;
//            }
//            EditorGUILayout.EndScrollView();
//        }
//        EditorGUILayout.EndVertical();
//    }
//
//    //------------------------------------------------------------------------- セーブと適用
//    // 設定の適用
//    void SaveAndApply() {
//        gameSettings.Save(true);
//        Close();
//    }
//}
//
//////////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////////
//
//// ゲーム設定の GUI 描画
//public partial class GameSettingsWindow {
//    //-------------------------------------------------------------------------- 定義
//    // 隠すプロパティ
//    static readonly string[] hideProperties = new string[] {
//        "m_Script", "gameSettings",
//    };
//
//    // 無視するプロパティ
//    static readonly string[] ignoreProperties = new string[] {
//        "gameSettingsName", "gameSettingsDescription", "scriptingDefineSymbols",
//    };
//
//    //-------------------------------------------------------------------------- ゲーム設定 GUI
//    void DrawGUI() {
//        //GUILayout.Label("Scheme", "BoldLabel");
//        GUILayout.BeginVertical("box");
//        {
//            //GUILayout.Label(gameSettings.gameSettingsName);
//            GUILayout.Label(gameSettings.gameSettingsDescription);
//        }
//        GUILayout.EndVertical();
//
//        // ゲーム設定
//        GUILayout.Label("Game Settings", "BoldLabel");
//        GUILayout.BeginVertical("box");
//        {
//            var sobj = new SerializedObject(this);
//            var iter = sobj.GetIterator();
//            var nest = true;
//            while (iter.NextVisible(nest)) {
//                nest = true;
//                if (Array.IndexOf(hideProperties, iter.name) >= 0) {
//                    continue;
//                }
//                if (Array.IndexOf(ignoreProperties, iter.name) >= 0) {
//                    nest = false;
//                    continue;
//                }
//                EditorGUILayout.PropertyField(iter);
//            }
//            sobj.ApplyModifiedProperties();
//        }
//        GUILayout.EndVertical();
//
//        // スクリプト定義シンボル
//        GUILayout.Label("Scripting Define Symbols", "BoldLabel");
//        GUILayout.BeginVertical("box");
//        {
//            DrawScriptingDefineSymbolsGUI(gameSettings.scriptingDefineSymbols);
//        }
//        GUILayout.EndVertical();
//    }
//}
//
//////////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////////
//
//// ScriptingDefineSymbols 編集の GUI 描画
//public partial class GameSettingsWindow {
//    //-------------------------------------------------------------------------- 変数
//    Dictionary<string,string[]> caches = null;
//
//    //-------------------------------------------------------------------------- 定義
//    void DrawScriptingDefineSymbolsGUI(List<string> symbols, bool editable = true) {
//        if (caches == null) {
//            caches = new Dictionary<string,string[]>();
//            foreach (var s in GameSettings.ScriptingDefineSymbols) {
//                caches.Add(s.Key, (s.Value == null)? null : s.Value.Select((v) => s.Key + "_" + v).ToArray());
//            }
//        }
//        foreach (var s in GameSettings.ScriptingDefineSymbols) {
//            if (s.Value == null) {
//                var oldValue = (symbols.IndexOf(s.Key) >= 0);
//                var newValue = Toggle(editable, s.Key, oldValue);
//                if (newValue != oldValue) {
//                    if (newValue) {
//                        symbols.Add(s.Key);
//                    } else {
//                        symbols.Remove(s.Key);
//                    }
//                }
//            } else {
//                var oldValue = Mathf.Max(Array.FindIndex(caches[s.Key], (v) => (symbols.IndexOf(s.Key) >= 0)), 0);
//                var newValue = Popup(editable, s.Key, s.Value, oldValue);
//                if (newValue != oldValue) {
//                    foreach (var v in caches[s.Key]) {
//                        symbols.Remove(v);
//                    }
//                    symbols.Add(caches[s.Key][newValue]);
//                }
//            }
//        }
//    }
//
//    //-------------------------------------------------------------------------- 内部処理 (GUI エレメント)
//    // トグル
//    bool Toggle(bool editable, string label, bool value) {
//        if (editable) {
//            value = EditorGUILayout.Toggle(label, value);
//        } else {
//            EditorGUILayout.LabelField(label, value.ToString());
//        }
//        return value;
//    }
//
//    // 選択
//    int Popup(bool editable, string label, string[] options, int value) {
//        if (editable) {
//            value = EditorGUILayout.Popup(label, value, options);
//        } else {
//            EditorGUILayout.LabelField(label, options[value]);
//        }
//        return value;
//    }
//}
