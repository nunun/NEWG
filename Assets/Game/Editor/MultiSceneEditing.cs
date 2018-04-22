using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using System.IO;
using System.Collections;
using System.Collections.Generic;

// マルチシーン編集設定
[InitializeOnLoad]
public class MultiSceneEditing {
    //---------------------------------------------------------------------- コンストラクタ
    static MultiSceneEditing() {
        EditorSceneManager.sceneOpened += OnSceneOpening;
    }

    static void OnSceneOpening(Scene scene, OpenSceneMode mode) {
        // シングルオープンでないと反応しない
        if (mode != OpenSceneMode.Single) {
            return;
        }

        // アセットからの相対パスに修正
        var scenePath = scene.path;
        if (scenePath.StartsWith(Application.dataPath)) {
            scenePath =  "Assets" + scenePath.Substring(Application.dataPath.Length);
        }

        // ファイル名が "_" から始まるのは
        // そもそもデバッグシーンなのでやらない
        var fileName = Path.GetFileName(scenePath);
        if (fileName.StartsWith("_")) {
            return;
        }

        // NOTE
        // "Boot" から始まるシーンは適用しない
        if (fileName.StartsWith("Boot")) {
            return;
        }

        // ディレクトリ名をチェック
        var dirName = Path.GetDirectoryName(scenePath);
        if (!dirName.EndsWith("ScenesInBuild")) {
            return;
        }
        dirName = Path.GetDirectoryName(dirName);

        // 共通デバッグシーンがあれば開く
        var commonScenePath = dirName + "/_.unity";
        if (File.Exists(commonScenePath)) {
            EditorSceneManager.OpenScene(commonScenePath, OpenSceneMode.Additive);
        }

        // そのシーンのデバッグシーンがあれば開く
        var additionalScenePath = dirName + "/_" + fileName;
        if (File.Exists(additionalScenePath)) {
            EditorSceneManager.OpenScene(additionalScenePath, OpenSceneMode.Additive);
        }
    }
}
