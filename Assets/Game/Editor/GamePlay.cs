using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

// ゲーム起動
[InitializeOnLoad]
public class GamePlay {
    //------------------------------------------------------------------------- コンストラクタ
    static GamePlay() {
        EditorApplication.playmodeStateChanged += HandleOnPlayModeChanged;
    }

    //------------------------------------------------------------------------- プレイ開始と停止時
    // プレイ開始
    public static void Play() {
        var editorScenes = EditorBuildSettings.scenes;
        for (int i = 0; i < editorScenes.Length; i++) {
            var editorScene = editorScenes[i];
            if (editorScene.enabled) {
                Play(editorScene.path);
                return;
            }
        }
        Debug.LogError("BuildSettings に何もシーンが登録されていません。");
    }

    // プレイ開始
    public static void Play(string scenePath) {
        // ■ 注意
        // 再生中なら止める
        if (EditorApplication.isPlaying) {
            EditorApplication.isPlaying = false;
            return;
        }

        // まずシーン保存を促す
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) {
            return;
        }

        // 今のシーン名は記録
        string currentScenePath = EditorSceneManager.GetActiveScene().path;
        EditorPrefs.SetString("lastScenePath", currentScenePath);

        // シーンを開く
        var scene = EditorSceneManager.OpenScene(scenePath);
        if (!scene.IsValid()) {
            Debug.LogError("シーン '" + scenePath + "' をロードできませんでした。プレイを中止します。");
            return;
        }

        // 再生
        EditorApplication.isPlaying = true;
    }

    // プレイモード停止時
    static void HandleOnPlayModeChanged() {
        string lastScenePath = EditorPrefs.GetString("lastScenePath", null);
        if (   EditorApplication.isPlaying == false
            && EditorApplication.isPlayingOrWillChangePlaymode == false
            && !string.IsNullOrEmpty(lastScenePath)) {
            EditorSceneManager.OpenScene(lastScenePath);
            EditorPrefs.SetString("lastScenePath", null);
        }
    }
}
