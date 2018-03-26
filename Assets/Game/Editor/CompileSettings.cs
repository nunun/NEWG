using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Collections.Generic;

// コンパイル設定
[Serializable]
public partial class CompileSettings : ISerializationCallbackReceiver {
    //-------------------------------------------------------------------------- 定義
    // シンボル一覧の定義
    // 必要なシンボルはここに定義してください。
    public static readonly Dictionary<string,string[]> ScriptDefineSymbols = new Dictionary<string,string[]>() {
        { "DEBUG",  null },
        { "WEBAPI", new string[] {"SERVER", "STANDALONE"}},
    };

    // スキーマ定義
    public static readonly Dictionary<string,CompileSettings> Schemes = new Dictionary<string,CompileSettings>() {
        { "DEVELOP", new CompileSettings("DEBUG;WEBAPI_SERVER") },
        { "RELEASE", new CompileSettings("WEBAPI_SERVER")       },
    };

    //-------------------------------------------------------------------------- 変数
    // シンボル初期値
    public HashSet<string> symbols = new HashSet<string>();

    // シリアライズ用シンボル一覧 (セミコロン区切り文字列)
    public string scriptingDefineSymbols = "";

    // 作業用キャッシュ
    static Dictionary<string,string[]> caches = null;

    //-------------------------------------------------------------------------- コンストラクタ
    public CompileSettings() {}

    public CompileSettings(params string[] symbols) {
        this.symbols.Clear();
        foreach (var s in symbols) {
            this.symbols.Add(s);
        }
    }

    //-------------------------------------------------------------------------- 実装 (ISerializationCallbackReceiver)
    public void OnBeforeSerialize() {
        scriptingDefineSymbols = HashSetToString(symbols);
    }

    public void OnAfterDeserialize() {
        symbols = StringToHashSet(scriptingDefineSymbols);
    }

    //-------------------------------------------------------------------------- GUI
    public void DrawGUI(bool editable = false) {
        if (caches == null) {
            caches = new Dictionary<string,string[]>();
            foreach (var s in ScriptDefineSymbols) {
                caches.Add(s.Key, (s.Value == null)? null : s.Value.Select((v) => s.Key + "_" + v).ToArray());
            }
        }
        GUILayout.Label("Scripting Define Symbols", "BoldLabel");
        GUILayout.BeginVertical("box");
        {
            foreach (var s in ScriptDefineSymbols) {
                if (s.Value == null) {
                    var oldValue = symbols.Contains(s.Key);
                    var newValue = Toggle(editable, s.Key, oldValue);
                    if (newValue != oldValue) {
                        if (newValue) {
                            symbols.Add(s.Key);
                        } else {
                            symbols.Remove(s.Key);
                        }
                    }
                } else {
                    var oldValue = Mathf.Max(Array.FindIndex(caches[s.Key], (v) => symbols.Contains(v)), 0);
                    var newValue = Popup(editable, s.Key, s.Value, oldValue);
                    if (newValue != oldValue) {
                        foreach (var v in caches[s.Key]) {
                            symbols.Remove(v);
                        }
                        symbols.Add(caches[s.Key][newValue]);
                    }
                }
            }
        }
        GUILayout.EndVertical();
    }
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// その他
public partial class CompileSettings {
    //-------------------------------------------------------------------------- 定義
    // 設定変更を行う対象のビルドターゲットグループ
    public static readonly BuildTargetGroup[] BuildTargetGroups = new BuildTargetGroup[] {
        BuildTargetGroup.Standalone,
        BuildTargetGroup.Android,
        BuildTargetGroup.iOS,
        BuildTargetGroup.WebGL,
    };

    //-------------------------------------------------------------------------- 変数
    static Dictionary<BuildTargetGroup,string> scriptingDefineSymbolsBackup = null;

    //-------------------------------------------------------------------------- 操作
    // スキームを適用
    public static void Apply(string schemeName) {
        Debug.Assert(CompileSettings.Schemes.ContainsKey(schemeName), "スキームなし");
        var compileSettings = CompileSettings.Schemes[schemeName];
        compileSettings.Apply();

        // スキーム適用メッセージ
        Debug.Log("CompileSettings: スキームが適用されました: " + schemeName);
    }

    // シンボル定義をバックアップ
    public static void Backup() {
        scriptingDefineSymbolsBackup = new Dictionary<BuildTargetGroup,string>();
        foreach (var buildTargetGroup in BuildTargetGroups) {
            scriptingDefineSymbolsBackup[buildTargetGroup] = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
        }

        // シンボル定義バックアップメッセージ
        // NOTE うるさくなるので出さない
        //Debug.Log("CompileSettings: シンボル定義をバックアップしました");
    }

    // シンボル定義を復元
    public static void Restore() {
        Debug.Assert(scriptingDefineSymbolsBackup != null, "バックアップなし");
        foreach (var pair in scriptingDefineSymbolsBackup) {
            PlayerSettings.SetScriptingDefineSymbolsForGroup(pair.Key, pair.Value);
        }
        scriptingDefineSymbolsBackup = null;

        // シンボル定義復元メッセージ
        Debug.Log("CompileSettings: シンボル定義を復元しました");
    }

    // シンボル定義を適用
    public void Apply() {
        var scriptingDefineSymbols = HashSetToString(symbols);
        foreach (var buildTargetGroup in BuildTargetGroups) {
            PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, scriptingDefineSymbols);
        }

        // シンボル定義適用メッセージ
        Debug.Log("CompileSettings: シンボル定義を適用しました: " + scriptingDefineSymbols);
    }

    // 設定のコピー
    public void Overwrite(CompileSettings other) {
        this.symbols.Clear();
        foreach (var s in other.symbols) {
            this.symbols.Add(s);
        }
    }

    //-------------------------------------------------------------------------- ユーティリティ
    // シンボル一覧をシンボル文字列に変換
    string HashSetToString(HashSet<string> symbols) {
        return string.Join(";", symbols.ToArray());
    }

    // シンボル文字列をシンボル一覧に変換
    HashSet<string> StringToHashSet(string scriptingDefineSymbols) {
        return new HashSet<string>(scriptingDefineSymbols.Split(';'));
    }

    //-------------------------------------------------------------------------- 内部処理 (GUI エレメント)
    // トグル
    bool Toggle(bool editable, string label, bool value) {
        if (editable) {
            value = EditorGUILayout.Toggle(label, value);
        } else {
            EditorGUILayout.LabelField(label, value.ToString());
        }
        return value;
    }

    // 選択
    int Popup(bool editable, string label, string[] options, int value) {
        if (editable) {
            value = EditorGUILayout.Popup(label, value, options);
        } else {
            EditorGUILayout.LabelField(label, options[value]);
        }
        return value;
    }
}

//// スキーム適用メッセージ
//Debug.Log("CompileSettings: スキームが適用されました: " + schemeName);
//// シンボル定義復元メッセージ
//Debug.Log("CompileSettings: シンボル定義を復元しました");
//// シンボル定義適用メッセージ
//Debug.Log("CompileSettings: シンボル定義を適用しました: " + scriptingDefineSymbols);
/// <summary>
/// ■ 注意
/// ビルド設定の保存と復元
/// 本来は CompileSettings に無くてもよいが、
/// CompileSettings.BuildTargetGroup メンバに依存しているので、
/// 他の場所に書くことができなかった。
/// ひとまずここに実装しておき、今後コードを整理して移動する。
/// </summary>
//public partial class CompileSettings
//{
//    /// <summary>
//    /// 保存されたシンボル設定
//    /// </summary>
//    public class BuildSettings : Dictionary<BuildTargetGroup,string> {
//        public BuildTarget buildTarget;
//    }
//
//    /// <summary>
//    /// 現在のビルドターゲットと定義シンボルを保存します。
//    /// </summary>
//    public static BuildSettings SaveBuildSettings() {
//        var buildSettings = new BuildSettings();
//        buildSettings.buildTarget = EditorUserBuildSettings.activeBuildTarget;
//        foreach (var targetGroup in CompileSettings.BuildTargetGroups) { //■ 注意:CompileSettings を参照しているので、いずれ直す
//            buildSettings[targetGroup] = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);
//        }
//        //Debug.Log("ビルド設定を保存しました。");
//        return buildSettings;
//    }
//
//    /// <summary>
//    /// 保存したビルドターゲットと定義シンボルを復元します。
//    /// </summary>
//    public static void RestoreBuildSettings(BuildSettings buildSettings) {
//        foreach (var pair in buildSettings) {
//            PlayerSettings.SetScriptingDefineSymbolsForGroup(pair.Key, pair.Value);
//        }
//        if (EditorUserBuildSettings.activeBuildTarget != buildSettings.buildTarget) {
//            EditorUserBuildSettings.SwitchActiveBuildTarget(buildSettings.buildTarget);
//        }
//        //Debug.Log("ビルド設定を保存時の状態に復元しました。");
//    }
//}
//        public const string DEFAULT_COMPILE_SETTINGS_PATH = "Assets/CompileSettings.xml"; // 読み込む.xmlのファイルパス
//
//        /// <summary>
//        /// 設定変更を行う対象のビルドターゲットグループ
//        /// </summary>
//        public static readonly BuildTargetGroup[] BuildTargetGroups = new BuildTargetGroup[] {
//            BuildTargetGroup.Standalone,
//            BuildTargetGroup.Android,
//            BuildTargetGroup.iOS,
//            BuildTargetGroup.WebGL,
//        };
//
//        /// <summary>
//        /// シンボル一覧のデータを管理するクラス
//        /// </summary>
//        [XmlRootAttribute("root", IsNullable = false)]
//        public class CompileSettingsData
//        {
//            public string           loadedPath = null; // ロードしたパス
//            public List<SchemeData> schemeList = null; // スキームリスト
//            public List<SymbolData> symbolList = null; // シンボルリスト
//        }
//
//        /// <summary>
//        /// スキームデータ
//        /// </summary>
//        [Serializable]
//        public class SchemeData
//        {
//            [XmlAttribute("name")]    public string   Name          { get; private set; } // スキーム名
//            [XmlAttribute("symbols")] public string[] DefineSymbols { get; private set; } // 定義シンボル一覧
//
//            /// <summary>
//            /// コンストラクタ (デフォルト)
//            /// </summary>
//            public SchemeData()
//            {
//                Name          = "";
//                DefineSymbols = new string[0];
//            }
//
//            /// <summary>
//            /// コンストラクタ
//            /// </summary>
//            public SchemeData(string name, params string[] defineSymbols)
//            {
//                Name          = name;
//                DefineSymbols = defineSymbols;
//            }
//        }
//
//        /// <summary>
//        /// シンボルデータ
//        /// </summary>
//        [Serializable]
//        public class SymbolData
//        {
//            [XmlAttribute("name")]    public string   Name     { get; private set; }  // 定義名
//            [XmlIgnore]               public bool     IsEnable { get; set;    }       // 有効かどうか
//            [XmlAttribute("options")] public string[] Options  { get; private set; }  // 選択アイテム
//            [XmlIgnore]               public string   Selected { get; set;    }       // 選択したもの
//            [XmlAttribute("comment")] public string   Comment  { get; private set; }  // コメント
//
//            /// <summary>
//            /// コンストラクタ (デフォルト)
//            /// </summary>
//            public SymbolData()
//            {
//                Name     = "";
//                IsEnable = false;
//                Options  = null;
//                Selected = null;
//                Comment  = "";
//            }
//
//            /// <summary>
//            /// コンストラクタ (チェックボックス)
//            /// </summary>
//            public SymbolData(string name, bool isEnable, string comment)
//            {
//                Name     = name;
//                IsEnable = isEnable;
//                Options  = null;
//                Selected = null;
//                Comment  = comment;
//            }
//
//            /// <summary>
//            /// コンストラクタ (選択)
//            /// </summary>
//            public SymbolData(string name, string[] options, string selected, string comment)
//            {
//                Name     = name;
//                IsEnable = true;
//                Options  = options;
//                Selected = selected;
//                Comment  = comment;
//            }
//        }
//
//        /// <summary>
//        /// スキームを変更します。
//        /// 一時設定フラグを付与します。
//        /// </summary>
//        public static bool SetScheme(string schemeName, string path = null, params string[] extraSymbols)
//        {
//            var compileSettingsData = LoadCompileSettings(path ?? DEFAULT_COMPILE_SETTINGS_PATH);
//            var schemeList = compileSettingsData.schemeList;
//            var symbolList = compileSettingsData.symbolList;
//            if (!ImportDefineSymbols(symbolList, schemeList, schemeName)) {
//                return false;
//            }
//            if (!SaveDefineSymbols(symbolList, extraSymbols)) {
//                return false;
//            }
//            return true;
//        }
//
//        /// <summary>
//        /// スキームで定義されているシンボル一覧を取得します。
//        /// </summary>
//        public static List<SymbolData> GetScemeSymbols(string schemeName, string path = null, params string[] extraSymbols)
//        {
//            var compileSettingsData = LoadCompileSettings(path ?? DEFAULT_COMPILE_SETTINGS_PATH);
//            var schemeList = compileSettingsData.schemeList;
//            var symbolList = compileSettingsData.symbolList;
//            ImportDefineSymbols(symbolList, schemeList, schemeName);
//            if (symbolList.Count > 0) {
//                symbolList.RemoveAt(0);//■ 注意:スキーム名の特別設定値は表示されないように除外しておく
//            }
//            return symbolList;
//        }
//
//        /// <summary>
//        /// 設定ファイルの読み込み。
//        /// </summary>
//        public static CompileSettingsData LoadCompileSettings(string path = DEFAULT_COMPILE_SETTINGS_PATH)
//        {
//            // ファイルがなければ作成
//            if (!File.Exists(path)) {
//                CreateCompileSettings(path);
//            }
//
//            // シンボルのロード
//            var xmlSerializer       = new XmlSerializer(typeof(CompileSettingsData));
//            var reader              = new StreamReader(path);
//            var compileSettingsData = xmlSerializer.Deserialize(reader) as CompileSettingsData;
//            reader.Close();
//
//            // ■ 注意
//            // スキーム設定を挿入
//            // シンボルをエクスポートしやすいように、シンボルリストの 0 番目に
//            // スキーム設定を投入して特別扱いする。
//            var schemeNames    = compileSettingsData.schemeList.Select(s => s.Name).ToArray();
//            var schemeSettings = new SymbolData("", schemeNames, null, "");
//            compileSettingsData.symbolList.Insert(0, schemeSettings);
//
//            // ■ 注意
//            // スキーム設定の調整
//            // スキーム名をスキーム定義シンボルに含める
//            // shmbolList の 0 番目に schemeList の設定を挿入してしまうので、
//            // スキームが変更された際に、スキーム名のシンボルが有効になる必要が
//            // あるため。
//            var schemeList = new List<SchemeData>();
//            foreach (var scheme in compileSettingsData.schemeList) {
//                if (Array.IndexOf(scheme.DefineSymbols, scheme.Name) >= 0) {
//                    schemeList.Add(scheme);
//                    continue;
//                }
//                var schemeDefineSymbols = scheme.DefineSymbols.ToList();
//                schemeDefineSymbols.Insert(0, scheme.Name);
//                schemeList.Add(new SchemeData(scheme.Name, schemeDefineSymbols.ToArray()));
//            }
//            compileSettingsData.schemeList = schemeList;
//
//            // シンボルの有効化
//            var defineSymbols = PlayerSettings
//                .GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup)
//                .Split( ';' );
//            ImportDefineSymbols(compileSettingsData.symbolList, defineSymbols);
//
//            // ロードしたパス
//            compileSettingsData.loadedPath = path;
//
//            // 終わり
//            return compileSettingsData;
//        }
//
//        /// <summary>
//        /// 設定ファイルを新しく作成します。
//        /// </summary>
//        public static void CreateCompileSettings(string path = DEFAULT_COMPILE_SETTINGS_PATH)
//        {
//            // ディレクトリがなければ作成
//            var dirName = Path.GetDirectoryName(path);
//            if (!Directory.Exists(dirName)) {
//                Directory.CreateDirectory(dirName);
//            }
//
//            // シンボルのセーブ (初期化)
//            var xmlSerializer       = new XmlSerializer(typeof(CompileSettingsData));
//            var writer              = new StreamWriter(path);
//            var compileSettingsData = new CompileSettingsData();
//            compileSettingsData.schemeList = new List<SchemeData>();
//            compileSettingsData.schemeList.Add(new SchemeData("DEVELOP", "SYMBOL1", "SYMBOL2_OPTION1"));
//            compileSettingsData.schemeList.Add(new SchemeData("STAGING", "SYMBOL1", "SYMBOL2_OPTION1"));
//            compileSettingsData.schemeList.Add(new SchemeData("RELEASE"));
//            compileSettingsData.symbolList = new List<SymbolData>();
//            compileSettingsData.symbolList.Add(new SymbolData("SYMBOL1", true, "コメント1"));
//            compileSettingsData.symbolList.Add(new SymbolData("SYMBOL2", new string[] {"OPTION1", "OPTION2", "OPTION3"}, "OPTION1", "コメント2"));
//            xmlSerializer.Serialize(writer, compileSettingsData);
//            writer.Close();
//            AssetDatabase.Refresh();
//
//            // ハイライト
//            var obj = AssetDatabase.LoadAssetAtPath(path, typeof(UnityEngine.Object));
//            EditorGUIUtility.PingObject(obj);
//        }
//
//        /// <summary>
//        /// 定義シンボル一覧をスキームから取り込みます
//        /// </summary>
//        public static bool ImportDefineSymbols(List<SymbolData> symbolList, List<SchemeData> schemeList, string schemeName)
//        {
//            for (int i = 0; i < schemeList.Count; i++) {
//                var scheme = schemeList[i];
//                if (scheme.Name == schemeName) {
//                    ImportDefineSymbols(symbolList, scheme.DefineSymbols);
//                    return true;
//                }
//            }
//            return false;
//        }
//
//        /// <summary>
//        /// 定義シンボル一覧を取り込みます
//        /// </summary>
//        public static void ImportDefineSymbols(List<SymbolData> symbolList, string[] defineSymbols)
//        {
//            foreach (var symbol in symbolList) {
//                if (symbol.Options == null) {
//                    symbol.IsEnable = defineSymbols.Any(s => s == symbol.Name);
//                } else {
//                    foreach (var option in symbol.Options) {
//                        if (defineSymbols.Any(s => s == MakeSymbolNameFromSelected(symbol.Name, option))) {
//                            symbol.Selected = option;
//                        }
//                    }
//                }
//            }
//
//            // エラーチェック
//            var count = symbolList.Count;
//            for (int i = count - 1; i >= 0; i--) {
//                var symbol = symbolList[i];
//                if (string.IsNullOrEmpty(symbol.Name) && symbol.Options == null) {
//                    var mesg = "name と options の両方の指定の無い SymbolData は使用できません。"
//                             + "シンボルは内部的に削除されたため、設定上には現れなくなります。"
//                             + "コンパイル設定の定義を修正して下さい。"
//                             + "(シンボルの comment='" + symbol.Comment + "')";
//                    Debug.LogWarning(mesg);
//                    symbolList.RemoveAt(i);
//                }
//            }
//        }
//
//        /// <summary>
//        /// 定義シンボル一覧を出力します
//        /// </summary>
//        public static string[] ExportDefineSymbols(List<SymbolData> symbolList)
//        {
//            var defineSymbolList = new List<string>();
//            foreach (var symbol in symbolList) {
//                if (symbol.Options == null) {
//                    if (symbol.IsEnable) {
//                        defineSymbolList.Add(symbol.Name);
//                    }
//                } else {
//                    if (Array.IndexOf(symbol.Options, symbol.Selected) >= 0) {
//                        defineSymbolList.Add(MakeSymbolNameFromSelected(symbol.Name, symbol.Selected));
//                    }
//                }
//            }
//            return defineSymbolList.ToArray();
//        }
//
//        /// <summary>
//        /// 定義シンボル一覧をビルド設定に保存します。
//        /// </summary>
//        public static bool SaveDefineSymbols(List<SymbolData> symbolList, params string[] extraSymbols)
//        {
//            // シンボル一覧を作成
//            var defineSymbols = ExportDefineSymbols(symbolList).ToList();
//            defineSymbols.AddRange(extraSymbols);
//
//            // 保存
//            var defineSymbolsString = string.Join(";", defineSymbols.ToArray());
//            foreach (var buildTargetGroup in BuildTargetGroups) {
//                PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, defineSymbolsString);
//            }
//            return true;
//        }
//
//        /// <summary>
//        /// 現在のシンボルをクリップボードにコピーします
//        /// </summary>
//        public static void CopySymbolName(SymbolData symbol)
//        {
//            // シンボルのコピー
//            var isSymbolCopied = false;
//            if (symbol.Options == null) {
//                if (symbol.IsEnable) {
//                    EditorGUIUtility.systemCopyBuffer = symbol.Name;
//                    isSymbolCopied = true;
//                }
//            } else {
//                if (Array.IndexOf(symbol.Options, symbol.Selected) >= 0){
//                    EditorGUIUtility.systemCopyBuffer = MakeSymbolNameFromSelected(symbol.Name, symbol.Selected);
//                    isSymbolCopied = true;
//                }
//            }
//
//            // コピーしたシンボルの表示
//            if (isSymbolCopied) {
//                var mesg = "Symbol Copied: '" + EditorGUIUtility.systemCopyBuffer + "'";
//                Debug.Log(mesg);
//            }
//        }
//
//        /// <summary>
//        /// 選択したシンボルからシンボル名を作成します
//        /// </summary>
//        public static string MakeSymbolNameFromSelected(string name, string selected)
//        {
//            if (string.IsNullOrEmpty(name)) {
//                if (string.IsNullOrEmpty(selected)) {
//                    return null;
//                } else {
//                    return selected;
//                }
//            } else {
//                if (string.IsNullOrEmpty(selected)) {
//                    return name;
//                } else {
//                    return name + "_" + selected;
//                }
//            }
//        }
//public void OnBeforeSerialize() {
//            defineSymbols = string.Join(";", DefineSymbols.Select((s) => {
//                if (!symbols.ContainsKey(s.Key)) {
//                    return null;
//                } else if (s.Value == null && symbols[s.Key] > 0) {
//                    return s.Key;
//                } else if (s.Value != null) {
//                    return s.Key + "_" + s.Value[Mathf.Clamp(symbols[s.Key], 0, s.Value.Length)];
//                }
//                return null;
//            }).Where((s) => (s != null && s != "")).ToArray());
//        }
//
//        public void OnAfterDeserialize() {
//            symbols = new Dictionary<string,int>();
//            foreach (var s in defineSymbols.Split(';')) {
//                if (DefineSymbols.ContainsKey(s)) {
//                    var key = s; var value = DefineSymbols[s]; var flag = 0;
//                    if (value == null) {
//                        flag = 1;
//                    } else if (value != null) {
//                        for (int i = 0; i < value.Length; i++) {
//                            if ((key + "_" + value[i]) == s) {
//                                flag = i;
//                                break;
//                            }
//                        }
//                    }
//                    symbols.Add(key, flag);
//                }
//            }
//        }
