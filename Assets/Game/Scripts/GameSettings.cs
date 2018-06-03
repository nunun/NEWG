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
    // スキーム一覧の定義
    // NOTE 追加のスキームをここに定義して下さい。
    public static readonly List<GameSettings> Schemes = new List<GameSettings>() {
        new GameSettings() {
            schemeName                  = "DEBUG_CLIENT",
            schemeDescription           = "デバッグ用クライアント",
            serverAddress               = null,
            serverPort                  = 0,
            serverPortRandomRange       = 0,
            serverDiscoveryAddress      = null,
            serverDiscoveryPort         = 0,
            webapiUrl                   = "http://localhost:7780",
            mindlinkUrl                 = null,
            buildOutputPath             = "Builds/DebugClient",
            buildTarget                 = BuildTarget.WebGL,
            buildHeadless               = false,
            buildAutoRun                = true,
            buildOpenFolder             = false,
            buildScriptingDefineSymbols = new List<string>() {"DEBUG"},
        },
        new GameSettings() {
            schemeName                = "DEBUG_SERVER",
            schemeDescription         = "デバッグ用サーバ",
            serverAddress               = "0.0.0.0",
            serverPort                  = 7777,
            serverPortRandomRange       = 0,
            serverDiscoveryAddress      = "localhost",
            serverDiscoveryPort         = 7777,
            webapiUrl                   = "http://api:7780",
            mindlinkUrl                 = "http://mindlink:7766",
            buildOutputPath             = "Builds/DebugServer",
            buildTarget                 = BuildTarget.StandaloneWindows64,
            buildHeadless               = false,
            buildAutoRun                = true,
            buildOpenFolder             = false,
            buildScriptingDefineSymbols = new List<string>() {"DEBUG", "SERVER_CODE"},
        },
        new GameSettings() {
            schemeName                  = "RELEASE_CLIENT",
            schemeDescription           = "リリース用クライアント",
            serverAddress               = null,
            serverPort                  = 0,
            serverPortRandomRange       = 0,
            serverDiscoveryAddress      = null,
            serverDiscoveryPort         = 0,
            webapiUrl                   = "http://fu-n.net:7780",
            mindlinkUrl                 = null,
            buildOutputPath             = "Builds/ReleaseClient",
            buildTarget                 = BuildTarget.WebGL,
            buildHeadless               = false,
            buildAutoRun                = false,
            buildOpenFolder             = true,
            buildScriptingDefineSymbols = new List<string>(),
        },
        new GameSettings() {
            schemeName                  = "RELEASE_SERVER",
            schemeDescription           = "リリース用サーバ",
            serverAddress               = "0.0.0.0",
            serverPort                  = 8000,
            serverPortRandomRange       = 1000,
            serverDiscoveryAddress      = "fu-n.net",
            serverDiscoveryPort         = 8000,
            webapiUrl                   = "http://localhost:7780",
            mindlinkUrl                 = "http://localhost:7766",
            buildOutputPath             = "Builds/ReleaseClient",
            buildTarget                 = BuildTarget.StandaloneLinuxUniversal,
            buildHeadless               = true,
            buildAutoRun                = false,
            buildOpenFolder             = true,
            buildScriptingDefineSymbols = new List<string>() {"SERVER_CODE"},
        },
    };

    // シンボル一覧の定義
    // NOTE 追加のシンボルをここに定義してください。
    public static readonly Dictionary<string,string[]> ScriptDefineSymbols = new Dictionary<string,string[]>() {
        { "DEBUG",           null }, // デバッグコードをバイナリに含めるかどうか
        { "SERVER_CODE",     null }, // サーバコードをバイナリに含めるかどうか
        { "STANDALONE_MODE", null }, // スタンドアローンモード
        //{ "ACCESS_SERVER", new string[] {"DEVELOP", "STAGING", "RELEASE"}},
    };

    //-------------------------------------------------------------------------- 変数
    public string       schemeName                  = "Default Scheme Name";
    public string       schemeDescription           = "Default Scheme Description";
    public string       serverAddress               = "localhost";
    public int          serverPort                  = 7777;
    public int          serverPortRandomRange       = 0;
    public string       serverDiscoveryAddress      = "localhost";
    public int          serverDiscoveryPort         = 7777;
    public string       webapiUrl                   = "http://localhost:7780";
    public string       mindlinkUrl                 = "http://localhost:7766";
    public string       buildOutputPath             = "Builds/DebugLocal";
    public BuildTarget  buildTarget                 = BuildTarget.StandaloneWindows64;
    public bool         buildHeadless               = false;
    public bool         buildAutoRun                = true;
    public bool         buildOpenFolder             = false;
    public List<string> buildScriptingDefineSymbols = new List<string>();
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
                if (Schemes.Count > 0) {
                    gameSettings.Assign(Schemes[0]);
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

    // スキーム設定をアサイン
    public void Assign(string schemeName) {
        var gameSettings = GetScheme(schemeName);
        if (gameSettings == null) {
            Debug.LogErrorFormat("GameSettings: スキームなし ({0})", schemeName);
            return;
        }
        Assign(gameSettings);
    }

    // オブジェクトにオーバーライド
    public void Overwrite(object obj) {
        var jsonText = JsonUtility.ToJson(this);
        JsonUtility.FromJsonOverwrite(jsonText, obj);
    }

    // スキーム設定を取得する
    public static GameSettings GetScheme(string schemeName) {
        foreach (var gameSettings in Schemes) {
            if (gameSettings.schemeName == schemeName) {
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
}
#endif

//// コンパイル設定
//[Serializable]
//public partial class GameSettings : ISerializationCallbackReceiver {
//    //-------------------------------------------------------------------------- 定義
//    // シンボル一覧の定義
//    // 必要なシンボルはここに定義してください。
//    public static readonly Dictionary<string,string[]> ScriptDefineSymbols = new Dictionary<string,string[]>() {
//        { "DEBUG",           null }, // デバッグコードをバイナリに含めるかどうか
//        { "SERVER_CODE",     null }, // サーバコードをバイナリに含めるかどうか
//        { "STANDALONE_MODE", null }, // スタンドアローンモード
//        //{ "ACCESS_SERVER", new string[] {"DEVELOP", "STAGING", "RELEASE"}},
//    };
//
//    // スキーム定義
//    public static readonly Dictionary<string,GameSettings> Schemes = new Dictionary<string,GameSettings>() {
//        { "DEBUG_CLIENT",   new GameSettings("DEBUG")                },
//        { "DEBUG_SERVER",   new GameSettings("DEBUG", "SERVER_CODE") },
//        { "RELEASE_CLIENT", new GameSettings()                       },
//        { "RELEASE_SERVER", new GameSettings("SERVER_CODE")          },
//    };
//
//    //-------------------------------------------------------------------------- 変数
//    // シンボル初期値
//    public HashSet<string> symbols = new HashSet<string>();
//
//    // シリアライズ用シンボル一覧 (セミコロン区切り文字列)
//    public string scriptingDefineSymbols = "";
//
//    // 作業用キャッシュ
//    static Dictionary<string,string[]> caches = null;
//
//    //-------------------------------------------------------------------------- コンストラクタ
//    public GameSettings() {}
//
//    public GameSettings(params string[] symbols) {
//        this.symbols.Clear();
//        foreach (var s in symbols) {
//            this.symbols.Add(s);
//        }
//    }
//
//    //-------------------------------------------------------------------------- 実装 (ISerializationCallbackReceiver)
//    public void OnBeforeSerialize() {
//        scriptingDefineSymbols = HashSetToString(symbols);
//    }
//
//    public void OnAfterDeserialize() {
//        symbols = StringToHashSet(scriptingDefineSymbols);
//    }
//
//    //-------------------------------------------------------------------------- GUI
//    public void DrawGUI(bool editable = false) {
//        if (caches == null) {
//            caches = new Dictionary<string,string[]>();
//            foreach (var s in ScriptDefineSymbols) {
//                caches.Add(s.Key, (s.Value == null)? null : s.Value.Select((v) => s.Key + "_" + v).ToArray());
//            }
//        }
//        GUILayout.Label("Scripting Define Symbols", "BoldLabel");
//        GUILayout.BeginVertical("box");
//        {
//            foreach (var s in ScriptDefineSymbols) {
//                if (s.Value == null) {
//                    var oldValue = symbols.Contains(s.Key);
//                    var newValue = Toggle(editable, s.Key, oldValue);
//                    if (newValue != oldValue) {
//                        if (newValue) {
//                            symbols.Add(s.Key);
//                        } else {
//                            symbols.Remove(s.Key);
//                        }
//                    }
//                } else {
//                    var oldValue = Mathf.Max(Array.FindIndex(caches[s.Key], (v) => symbols.Contains(v)), 0);
//                    var newValue = Popup(editable, s.Key, s.Value, oldValue);
//                    if (newValue != oldValue) {
//                        foreach (var v in caches[s.Key]) {
//                            symbols.Remove(v);
//                        }
//                        symbols.Add(caches[s.Key][newValue]);
//                    }
//                }
//            }
//        }
//        GUILayout.EndVertical();
//    }
//}
//
//////////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////////
//
//// その他
//public partial class GameSettings {
//    //-------------------------------------------------------------------------- 定義
//    // 設定変更を行う対象のビルドターゲットグループ
//    public static readonly BuildTargetGroup[] BuildTargetGroups = new BuildTargetGroup[] {
//        BuildTargetGroup.Standalone,
//        BuildTargetGroup.Android,
//        BuildTargetGroup.iOS,
//        BuildTargetGroup.WebGL,
//    };
//
//    //-------------------------------------------------------------------------- 変数
//    static Dictionary<BuildTargetGroup,string> scriptingDefineSymbolsBackup = null;
//
//    //-------------------------------------------------------------------------- 操作
//    // スキームを適用
//    public static void Apply(string schemeName) {
//        Debug.Assert(GameSettings.Schemes.ContainsKey(schemeName), "スキームなし");
//        var compileSettings = GameSettings.Schemes[schemeName];
//        compileSettings.Apply();
//
//        // スキーム適用メッセージ
//        Debug.Log("GameSettings: スキームが適用されました: " + schemeName);
//    }
//
//    // シンボル定義をバックアップ
//    public static void Backup() {
//        scriptingDefineSymbolsBackup = new Dictionary<BuildTargetGroup,string>();
//        foreach (var buildTargetGroup in BuildTargetGroups) {
//            scriptingDefineSymbolsBackup[buildTargetGroup] = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
//        }
//
//        // シンボル定義バックアップメッセージ
//        // NOTE うるさくなるので出さない
//        //Debug.Log("GameSettings: シンボル定義をバックアップしました");
//    }
//
//    // シンボル定義を復元
//    public static void Restore() {
//        Debug.Assert(scriptingDefineSymbolsBackup != null, "バックアップなし");
//        foreach (var pair in scriptingDefineSymbolsBackup) {
//            PlayerSettings.SetScriptingDefineSymbolsForGroup(pair.Key, pair.Value);
//        }
//        scriptingDefineSymbolsBackup = null;
//
//        // シンボル定義復元メッセージ
//        Debug.Log("GameSettings: シンボル定義を復元しました");
//    }
//
//    // シンボル定義を適用
//    public void Apply() {
//        var scriptingDefineSymbols = HashSetToString(symbols);
//        foreach (var buildTargetGroup in BuildTargetGroups) {
//            PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, scriptingDefineSymbols);
//        }
//
//        // シンボル定義適用メッセージ
//        Debug.Log("GameSettings: シンボル定義を適用しました: " + scriptingDefineSymbols);
//    }
//
//    // 設定のコピー
//    public void Overwrite(GameSettings other) {
//        this.symbols.Clear();
//        foreach (var s in other.symbols) {
//            this.symbols.Add(s);
//        }
//    }
//
//    //-------------------------------------------------------------------------- ユーティリティ
//    // シンボル一覧をシンボル文字列に変換
//    string HashSetToString(HashSet<string> symbols) {
//        return string.Join(";", symbols.ToArray());
//    }
//
//    // シンボル文字列をシンボル一覧に変換
//    HashSet<string> StringToHashSet(string scriptingDefineSymbols) {
//        return new HashSet<string>(scriptingDefineSymbols.Split(';'));
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
