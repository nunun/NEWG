using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

// Assets ユーティリティ
public class AssetsUtility {
    //-------------------------------------------------------------------------- 内部処理
    // ファイル編集
    public static void EditFile(string path, int line = 1) {
        InternalEditorUtility.OpenFileAtLineExternal(path, line);
    }

    // ディレクトリをハイライト
    public static void PingDirectory(string path) {
        var files = Directory.GetFiles(path);
        if (files.Length > 0) {
            path = files[0];
        }
        var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
        EditorGUIUtility.PingObject(asset);
    }
}
