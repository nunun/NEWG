using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections.Generic;

// ゲーム設定ウィンドウ
public partial class GameSettingsWindow : EditorWindow {
    //-------------------------------------------------------------------------- 定義
    const string WINDOW_TITLE = "ゲーム設定";

    //-------------------------------------------------------------------------- 変数
    // ゲーム設定
    [SerializeField] GameSettings gameSettings = null;

    // ゲーム設定名一覧
    string[] gameSettingsNames = null;

    // スクロール座標
    Vector2 scrollPos = Vector2.zero;

    //-------------------------------------------------------------------------- ウインドウ操作
    // ウインドウを開く
    public static void Open() {
        GetWindow<GameSettingsWindow>(true, WINDOW_TITLE);
    }

    //-------------------------------------------------------------------------- 実装 (EditorWindow)
    // GUI 表示
    void OnGUI() {
        // ゲーム設定名一覧
        if (gameSettingsNames == null) {
            gameSettingsNames = GameSettings.GameSettingsList.Select((item) => item.gameSettingsName).ToArray();
        }

        // ゲーム設定初期化
        if (gameSettings == null) {
            gameSettings = GameSettings.Load(true);
        }

        // シンボル一覧を描画
        EditorGUILayout.BeginVertical();
        {
            GUILayout.Space(5.0f);

            // コントロールボタン
            EditorGUILayout.BeginHorizontal();
            {
                // ゲーム設定選択ボタン
                var oldIndex = Array.IndexOf(gameSettingsNames, gameSettings.gameSettingsName);
                var newIndex = EditorGUILayout.Popup(Mathf.Max(0, oldIndex), gameSettingsNames);
                if (newIndex != oldIndex) {
                    var gameSettingsName   = gameSettingsNames[newIndex];
                    var foundGameSettings = GameSettings.Find(gameSettingsName);
                    if (foundGameSettings != null) {
                        EditorApplication.delayCall += () => gameSettings.Assign(foundGameSettings);
                    } else {
                        Debug.LogErrorFormat("GameSettingsWindow: ゲーム設定なし？ ({0})", gameSettingsName);
                    }
                }

                // 保存ボタン
                if (GUILayout.Button("Save", "MiniButton", GUILayout.ExpandWidth(false))) {
                    EditorApplication.delayCall += SaveAndApply;
                }
            }
            EditorGUILayout.EndHorizontal();

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(position.height));
            {
                var labelWidth = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = 200.0f;
                {
                    DrawGUI();
                }
                EditorGUIUtility.labelWidth = labelWidth;
            }
            EditorGUILayout.EndScrollView();
        }
        EditorGUILayout.EndVertical();
    }

    //------------------------------------------------------------------------- セーブと適用
    // 設定の適用
    void SaveAndApply() {
        gameSettings.Save(true);
        Close();
    }
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// ゲーム設定の GUI 描画
public partial class GameSettingsWindow {
    //-------------------------------------------------------------------------- 定義
    // 隠すプロパティ
    static readonly string[] hideProperties = new string[] {
        "m_Script", "gameSettings",
    };

    // 無視するプロパティ
    static readonly string[] ignoreProperties = new string[] {
        "gameSettingsName", "gameSettingsDescription", "scriptingDefineSymbols",
    };

    //-------------------------------------------------------------------------- ゲーム設定 GUI
    void DrawGUI() {
        //GUILayout.Label("Scheme", "BoldLabel");
        GUILayout.BeginVertical("box");
        {
            //GUILayout.Label(gameSettings.gameSettingsName);
            GUILayout.Label(gameSettings.gameSettingsDescription);
        }
        GUILayout.EndVertical();

        // ゲーム設定
        GUILayout.Label("Game Settings", "BoldLabel");
        GUILayout.BeginVertical("box");
        {
            var sobj = new SerializedObject(this);
            var iter = sobj.GetIterator();
            var nest = true;
            while (iter.NextVisible(nest)) {
                nest = true;
                if (Array.IndexOf(hideProperties, iter.name) >= 0) {
                    continue;
                }
                if (Array.IndexOf(ignoreProperties, iter.name) >= 0) {
                    nest = false;
                    continue;
                }
                EditorGUILayout.PropertyField(iter);
            }
            sobj.ApplyModifiedProperties();
        }
        GUILayout.EndVertical();

        // スクリプト定義シンボル
        GUILayout.Label("Scripting Define Symbols", "BoldLabel");
        GUILayout.BeginVertical("box");
        {
            DrawScriptingDefineSymbolsGUI(gameSettings.scriptingDefineSymbols);
        }
        GUILayout.EndVertical();
    }
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// ScriptingDefineSymbols 編集の GUI 描画
public partial class GameSettingsWindow {
    //-------------------------------------------------------------------------- 変数
    Dictionary<string,string[]> caches = null;

    //-------------------------------------------------------------------------- 定義
    void DrawScriptingDefineSymbolsGUI(List<string> symbols, bool editable = true) {
        if (caches == null) {
            caches = new Dictionary<string,string[]>();
            foreach (var s in GameSettings.ScriptingDefineSymbols) {
                caches.Add(s.Key, (s.Value == null)? null : s.Value.Select((v) => s.Key + "_" + v).ToArray());
            }
        }
        foreach (var s in GameSettings.ScriptingDefineSymbols) {
            if (s.Value == null) {
                var oldValue = (symbols.IndexOf(s.Key) >= 0);
                var newValue = Toggle(editable, s.Key, oldValue);
                if (newValue != oldValue) {
                    if (newValue) {
                        symbols.Add(s.Key);
                    } else {
                        symbols.Remove(s.Key);
                    }
                }
            } else {
                var oldValue = Mathf.Max(Array.FindIndex(caches[s.Key], (v) => (symbols.IndexOf(s.Key) >= 0)), 0);
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
