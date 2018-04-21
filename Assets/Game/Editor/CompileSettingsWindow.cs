using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections.Generic;

// コンパイル設定ウィンドウ
public class CompileSettingsWindow : EditorWindow {
    //---------------------------------------------------------------------- 定義
    // コンパイル設定データ
    [Serializable]
    public class Data {
        public string          currentOptionName = null;
        public CompileSettings currentSettings   = new CompileSettings();
    }

    const string WINDOW_TITLE = "コンパイル設定";
    const string DATA_FILE    = "Assets/CompileSettings.json";

    //---------------------------------------------------------------------- 変数
    Data currentData = null; // 現在のデータ

    static Vector2           scrollPos    = Vector2.zero; // スクロール座標
    static string[]          optionNames  = null;         // オプション名一覧
    static CompileSettings[] optionValues = null;         // オプション値一覧

    //---------------------------------------------------------------------- ウインドウ操作
    // ウインドウを開く
    public static void Open() {
        GetWindow<CompileSettingsWindow>(true, WINDOW_TITLE);
    }

    //---------------------------------------------------------------------- 実装 (EditorWindow)
    // GUI 表示
    void OnGUI() {
        // 初期化
        if (currentData == null) {
            currentData = Load();

            // スキーマ読み込み
            var optionNameList  = new List<string>();
            var optionValueList = new List<CompileSettings>();
            foreach (var pair in CompileSettings.Schemes) {
                optionNameList.Add(pair.Key);
                optionValueList.Add(pair.Value);
            }
            optionNames  = optionNameList.ToArray();
            optionValues = optionValueList.ToArray();
        }

        // シンボル一覧を描画
        EditorGUILayout.BeginVertical();
        {
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(position.height));
            {
                GUILayout.Space(5.0f);

                // コントロールボタン
                EditorGUILayout.BeginHorizontal();
                {
                    // プリセット選択ボタン
                    var oldIndex = Array.IndexOf(optionNames, currentData.currentOptionName);
                    var newIndex = EditorGUILayout.Popup(oldIndex, optionNames);
                    newIndex = Mathf.Max(0, newIndex); //■ 注意:未選択時は自動的に 0 にする
                    if (newIndex != oldIndex) {
                        currentData.currentOptionName = optionNames[newIndex];
                        currentData.currentSettings.Overwrite(optionValues[newIndex]);
                    }

                    // 保存ボタン
                    if (GUILayout.Button("Save", "MiniButton", GUILayout.ExpandWidth(false))) {
                        EditorApplication.delayCall += SaveAndApply;
                    }
                }
                EditorGUILayout.EndHorizontal();

                var labelWidth = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = 200.0f;
                //EditorGUILayout.BeginVertical("Box");
                //{
                    currentData.currentSettings.DrawGUI(true);
                //}
                //EditorGUILayout.EndVertical();
                EditorGUIUtility.labelWidth = labelWidth;
            }
            EditorGUILayout.EndScrollView();
        }
        EditorGUILayout.EndVertical();
    }

    //---------------------------------------------------------------------- セーブとロード
    // ロード (JSON から復元)
    Data Load() {
        try {
            using (var sr = new StreamReader(DATA_FILE, Encoding.UTF8)) {
                return JsonUtility.FromJson<Data>(sr.ReadToEnd());
            }
        } catch {
            return new Data();
        }
    }

    // セーブ (JSON で保存)
    void Save(Data data) {
        using (var sw = new StreamWriter(DATA_FILE, false, Encoding.UTF8)) {
            sw.Write(JsonUtility.ToJson(data));
        }
        AssetDatabase.Refresh();
    }

    // 設定の適用
    void SaveAndApply() {
        Save(currentData);
        currentData.currentSettings.Apply();
        Close();
    }
}

//var jsonString = EditorPrefs.GetString("CompileSettingsWindow.Data", "{}");
//return JsonUtility.FromJson<Data>(jsonString);
//var jsonString = JsonUtility.ToJson(data);
//EditorPrefs.SetString("CompileSettingsWindow.Data", jsonString);
// 設定ハイライトボタン
//if (GUILayout.Button("Compile Settings", "MiniButton", GUILayout.ExpandWidth(false))) {
//    var path = compileSettingsData.loadedPath;
//    Selection.activeObject = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
//    EditorGUIUtility.PingObject(Selection.activeObject);
//}
//GUILayout.Label("/", "MiniLabel");
// 編集ボタン
//if (GUILayout.Button("Edit", "MiniButton", GUILayout.ExpandWidth(false))) {
//    var path  = compileSettingsData.loadedPath;
//    var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
//    if (asset != null) {
//        EditorGUIUtility.PingObject(asset);
//    }
//    InternalEditorUtility.OpenFileAtLineExternal(path, 0);
//}
//        private const string               WINDOW_TITLE        = "コンパイル設定";          // ウィンドウのタイトル
//        private const float                LABEL_WIDTH         = 180.0f;                    // ラベルフィールドの横幅
//        private const float                FIELD_WIDTH         = 120.0f;                    // 設定フィールドの横幅
//        private const float                CONFIG_WIDTH        = LABEL_WIDTH + FIELD_WIDTH; // フィールドの横幅
//        private const float                COMMENT_WIDTH       = 100.0f;                    // コメントの横幅
//        private static Vector2             scrollPos           = Vector2.zero;              // スクロール座標
//        private static CompileSettingsData compileSettingsData = null;                      // シンボルのリスト
//
//        /// <summary>
//        /// ウインドウを開く
//        /// </summary>
//        public static void Open()
//        {
//            var window = GetWindow<CompileSettingsWindow>(true, WINDOW_TITLE);
//            window.Init();
//        }
//
//        /// <summary>
//        /// 初期化する時に呼び出します
//        /// </summary>
//        private void Init()
//        {
//            // ■ 注意
//            // 今のところ処理なし
//        }
//
//        /// <summary>
//        /// GUI を表示する時に呼び出されます
//        /// </summary>
//        private void OnGUI()
//        {
//            // ■ 注意
//            // 未初期化ならやらない
//            if (compileSettingsData == null) {
//                compileSettingsData = CompileSettings.LoadCompileSettings();
//            }
//            var schemeList = compileSettingsData.schemeList;
//            var symbolList = compileSettingsData.symbolList;
//
//            // シンボル一覧を描画
//            EditorGUILayout.BeginVertical();
//            {
//                scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(position.height));
//                {
//                    GUILayout.Space(5.0f);
//
//                    // コントロールボタン
//                    EditorGUILayout.BeginHorizontal();
//                    {
//                        // スキーム設定選択ボタン
//                        var schemeSettings = symbolList[0];
//                        var oldIndex = Array.IndexOf(schemeSettings.Options, schemeSettings.Selected);
//                        var newIndex = EditorGUILayout.Popup(schemeSettings.Name, oldIndex, schemeSettings.Options);
//                        newIndex = Mathf.Max(0, newIndex); //■ 注意:未選択時は自動的に 0 にする
//                        if (newIndex != oldIndex) {
//                            schemeSettings.Selected = schemeSettings.Options[newIndex];
//                            CompileSettings.ImportDefineSymbols(symbolList, schemeList, schemeSettings.Selected);
//                        }
//
//                        // 保存ボタン
//                        if (GUILayout.Button("Save", "MiniButton", GUILayout.ExpandWidth(false))) {
//                            CompileSettings.SaveDefineSymbols(symbolList);
//                            Close();
//                        }
//                        //GUILayout.Label("/", GUILayout.ExpandWidth(false));
//
//                    }
//                    EditorGUILayout.EndHorizontal();
//
//                    // シンボル一覧の描画
//                    EditorGUILayout.BeginVertical("Box");
//                    {
//                        // 上下空間調整
//                        GUILayout.Space(-3.0f);
//
//                        // 設定がない
//                        if (symbolList.Count <= 1) {
//                            var path = compileSettingsData.loadedPath;
//                            var mesg = "No compile settings exists."
//                                     + " Please change '" + path + "'"
//                                     + " or simply delete to restore sample settings.";
//                            EditorGUILayout.HelpBox(mesg, MessageType.Warning);
//                        }
//
//                        // 設定一覧
//                        var labelWidth = EditorGUIUtility.labelWidth;
//                        var fieldWidth = EditorGUIUtility.fieldWidth;
//                        EditorGUIUtility.labelWidth = LABEL_WIDTH;
//                        EditorGUIUtility.fieldWidth = FIELD_WIDTH;
//                        {
//                            for (int i = 1; i < symbolList.Count; i++) {
//                                var symbol = symbolList[i];
//                                EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
//                                {
//                                    // シンボルのコピー
//                                    GUILayout.BeginVertical();
//                                    {
//                                        GUILayout.Space(3.0f);
//                                        if (GUILayout.Button("", "flow triggerPin out", GUILayout.ExpandWidth(false))) {
//                                            CompileSettings.CopySymbolName(symbol);
//                                        }
//                                    }
//                                    GUILayout.EndVertical();
//
//                                    // チェックボックス、名前ラベル、コメントラベル...
//                                    if (symbol.Options == null) {
//                                        symbol.IsEnable = EditorGUILayout.Toggle(symbol.Name, symbol.IsEnable, GUILayout.Width(CONFIG_WIDTH));
//                                    } else {
//                                        var oldIndex = Array.IndexOf(symbol.Options, symbol.Selected);
//                                        var newIndex = EditorGUILayout.Popup(symbol.Name, oldIndex, symbol.Options, GUILayout.Width(CONFIG_WIDTH));
//                                        newIndex = Mathf.Max(0, newIndex); //■ 注意:未選択時は自動的に 0 にする
//                                        if (newIndex != oldIndex) {
//                                            symbol.Selected = symbol.Options[newIndex];
//                                        }
//                                    }
//                                    EditorGUILayout.LabelField(symbol.Comment, GUILayout.ExpandWidth(true), GUILayout.MinWidth(COMMENT_WIDTH));
//                                    GUILayout.FlexibleSpace();
//                                }
//                                EditorGUILayout.EndHorizontal();
//                            }
//                        }
//                        EditorGUIUtility.labelWidth = labelWidth;
//                        EditorGUIUtility.fieldWidth = fieldWidth;
//
//                        // 上下空間調整
//                        GUILayout.Space(2.0f);
//                    }
//                    EditorGUILayout.EndVertical();
//                }
//                EditorGUILayout.EndScrollView();
//            }
//            EditorGUILayout.EndVertical();
//        }
//    }
