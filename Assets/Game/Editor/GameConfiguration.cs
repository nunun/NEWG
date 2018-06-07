using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

// ゲーム構成設定
[CreateAssetMenu(fileName = "GameConfiguration", menuName = "ScriptableObject/GameConfiguration", order = 1000)]
public partial class GameConfiguration : GameSettings {
    //-------------------------------------------------------------------------- 変数
    // ゲーム構成表示設定 (ゲーム構成ウインドウ用)
    [Serializable]
    public class GameConfigurationDisplaySettings {
        public int   displayOrder = 0;
        public Color displayColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);
    }

    //-------------------------------------------------------------------------- 変数
    public List<string>                     scriptingDefineSymbols           = new List<string>();
    public GameConfigurationDisplaySettings gameConfigurationDisplaySettings = new GameConfigurationDisplaySettings();
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// インポートとエクスポートと設定の適用
public partial class GameConfiguration {
    //-------------------------------------------------------------------------- 定義
    // ゲーム構成ファイルパス
    static readonly string GAME_CONFIGURATION_ASSET_PATH = "Assets/GameConfiguration.asset";
    // ゲーム構成ファイル検索パターン
    static readonly string GAME_CONFIGURATION_ASSET_PATH_FORMAT = "Assets/Game/Configurations/{0}.asset";
    // ゲーム設定ファイルパス
    static readonly string GAME_SETTINGS_ASSET_PATH = "Assets/Game/Settings/GameSettings.asset";
    // mcs.rsp ファイルパス
    static readonly string MCS_RSP_PATH = "Assets/mcs.rsp";

    //-------------------------------------------------------------------------- 操作
    // ロード
    public static GameConfiguration Load(bool createGameConfiguration = false) {
        var gameConfiguration = ReadAssetFile<GameConfiguration>(GAME_CONFIGURATION_ASSET_PATH);
        var gameSettings      = ReadAssetFile<GameSettings>(GAME_SETTINGS_ASSET_PATH);
        var mcsSymbols        = ReadRspFile(MCS_RSP_PATH);
        if (gameConfiguration == null || gameSettings == null || mcsSymbols != null) {
            if (!createGameConfiguration) {
                return null;
            }
            gameConfiguration = ScriptableObject.CreateInstance<GameConfiguration>();
            gameConfiguration.Save();
        }
        return gameConfiguration;
    }

    // セーブ
    public void Save(bool recompile = false) {
        WriteAssetFile<GameConfiguration>(GAME_CONFIGURATION_ASSET_PATH, this);
        WriteAssetFile<GameSettings>(GAME_SETTINGS_ASSET_PATH, this);
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
        EditorUtility.CopySerialized(gameConfiguration, this);
    }

    // ゲーム構成をアサイン
    public void Assign(string name) {
        var gameConfiguration = Find(name);
        if (gameConfiguration == null) {
            Debug.LogErrorFormat("GameConfiguration: ゲーム設定なし ({0})", name);
            return;
        }
        Assign(gameConfiguration);
    }

    // ゲーム構成を取得する
    public static GameConfiguration Find(string name) {
        var gameConfigurationList = List();
        foreach (var gameConfiguration in gameConfigurationList) {
            if (gameConfiguration.name == name) {
                return gameConfiguration;
            }
        }
        return null;
    }

    // ゲーム構成一覧を取得する
    public static List<GameConfiguration> List() {
        var path     = string.Format(GAME_CONFIGURATION_ASSET_PATH_FORMAT, "*");
        var fileName = Path.GetFileName(path);
        var dirName  = Path.GetDirectoryName(path);
        return Directory.GetFiles(dirName, fileName)
            .Select((p) => ReadAssetFile<GameConfiguration>(p))
            .OrderBy((c) => c.gameConfigurationDisplaySettings.displayOrder)
            .ThenBy((c) => c.name).ToList();
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
        RemoveFile(GAME_CONFIGURATION_ASSET_PATH);
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

//  public static List<string> List() {
//    var pathFormat = GAME_CONFIGURATION_ASSET_PATH_FORMAT;
//    var headLength = pathFormat.IndexOf("{");
//    var tailLength = pathFormat.Length - pathFormat.LastIndexOf("}") - 1;
//    var path       = string.Format(pathFormat, "*");
//    var fileName   = Path.GetFileName(path);
//    var dirName    = Path.GetDirectoryName(path);
//    return Directory.GetFiles(dirName, fileName).Select((f) => {
//        return f.Substring(0, f.Length - tailLength).Substring(headLength);
//    }).ToList();
//// json ファイルの読み込み
//static GameConfiguration ReadJsonFile(string path) {
//    var text = ReadFile(path);
//    if (text == null) {
//        return null;
//    }
//    try {
//        return JsonUtility.FromJson<GameConfiguration>(text);
//    } catch {}
//    return null;
//}
//
//// json ファイルの書き込み
//static void WriteJsonFile(string path, GameConfiguration gameConfiguration) {
//    WriteFile(path, JsonUtility.ToJson(gameConfiguration, true));
//}
////-------------------------------------------------------------------------- 実装 (MonoBehaviour)
//void OnValidate() {
//    foreach (var settingsAsset in settingsAssets) {
//        if (settingsAsset != null) {
//            var subAsset = ScriptableObject.CreateInstance(settingsAsset.GetType());
//            EditorUtility.CopySerialized(settingsAsset, subAsset);
//            AssetDatabase.AddObjectToAsset(subAsset, this);
//            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(settingsAsset));
//        }
//    }
//    AssetDatabase.SaveAssets();
//}
//////////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////////
//
//// インポートとエクスポートと設定の適用
//public partial class GameConfiguration {
//    //-------------------------------------------------------------------------- 定義
//    // ゲーム構成ファイルパス
//    public static readonly string GAME_CONFIGURATION_JSON_PATH = "Assets/GameConfiguration.json";
//    // ゲーム設定ファイルパス
//    public static readonly string GAME_SETTINGS_ASSET_PATH = "Assets/Game/Settings/GameSettings.asset";
//    // mcs.rsp ファイルパス
//    public static readonly string MCS_RSP_PATH = "Assets/mcs.rsp";
//
//    //-------------------------------------------------------------------------- 操作
//    // ロード
//    public static GameConfiguration Load(bool createDefaultGameConfiguration = false) {
//        var gameConfiguration = ReadJsonFile(GAME_CONFIGURATION_JSON_PATH);
//        var mcsSymbols   = ReadRspFile(MCS_RSP_PATH);
//        if (gameConfiguration == null || mcsSymbols == null) {
//            if (!createDefaultGameConfiguration) {
//                return null;
//            }
//            if (gameConfiguration == null) {
//                gameConfiguration = new GameConfiguration();
//                if (gameConfigurationList.Count > 0) {
//                    gameConfiguration.Assign(gameConfigurationList[0]);
//                }
//            }
//            gameConfiguration.Save();
//        }
//        return gameConfiguration;
//    }
//
//    // セーブ
//    public void Save(bool recompile = false) {
//        WriteJsonFile(GAME_CONFIGURATION_JSON_PATH, this);
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
//    // ゲーム構成のクリーンアップ
//    public static void CleanUp() {
//        RemoveGameConfigurationFiles();
//        AssetDatabase.Refresh();
//    }
//
//    // 別のゲーム構成をアサイン
//    public void Assign(GameConfiguration gameConfiguration) {
//        var jsonText = JsonUtility.ToJson(gameConfiguration);
//        JsonUtility.FromJsonOverwrite(jsonText, this);
//    }
//
//    // ゲーム構成をアサイン
//    public void Assign(string name) {
//        var gameConfiguration = Find(name);
//        if (gameConfiguration == null) {
//            Debug.LogErrorFormat("GameConfiguration: ゲーム設定なし ({0})", name);
//            return;
//        }
//        Assign(gameConfiguration);
//    }
//
//    // ゲーム構成を取得する
//    public static GameConfiguration Find(string name) {
//        foreach (var gameConfiguration in gameConfigurationList) {
//            if (gameConfiguration.name == name) {
//                return gameConfiguration;
//            }
//        }
//        return null;
//    }
//
//    //-------------------------------------------------------------------------- ファイルの読み書き
//    // json ファイルの読み込み
//    static GameConfiguration ReadJsonFile(string path) {
//        var text = ReadFile(path);
//        if (text == null) {
//            return null;
//        }
//        try {
//            return JsonUtility.FromJson<GameConfiguration>(text);
//        } catch {}
//        return null;
//    }
//
//    // json ファイルの書き込み
//    static void WriteJsonFile(string path, GameConfiguration gameConfiguration) {
//        WriteFile(path, JsonUtility.ToJson(gameConfiguration, true));
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
//    static void RemoveGameConfigurationFiles() {
//        RemoveFile(GAME_CONFIGURATION_JSON_PATH);
//        RemoveFile(GAME_SETTINGS_ASSET_PATH);
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
//// JSON 変換用データとスクリプタブルオブジェクトのインスタンス生成
//public partial class GameConfiguration {
//    //-------------------------------------------------------------------------- 定義
//    public class JsonData : Dictionary<string,object> {}
//
//    //-------------------------------------------------------------------------- 定義
//    public static TScriptableObject CreateScriptableObject<TScriptableObject>(JsonData jsonData) where TScriptableObject : ScriptableObject{
//        var scriptableObject = ScriptableObject.CreateInstance<TScriptableObject>();
//        return scriptableObject;
//    }
//}
////-------------------------------------------------------------------------- 定義
//// ゲーム構成設定一覧
//static readonly List<GameConfiguration> gameConfigurationList = new List<GameConfiguration>() {
//    new GameConfiguration() {
//        name         = "Default",
//        description  = "デバッグ設定",
//        gameSettings = CreateScriptableObject<GameSettings>(new JsonData() {
//            {"serverAddress", "localhost"},
//        }),
//        scriptingDefineSymbols = new List<string>() {"DEBUG"},
//    }
//};
