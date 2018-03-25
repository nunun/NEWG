using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEditor;
using UnityEditor.SceneManagement;

// ゲームビルダー
public partial class GameBuilder {
    //-------------------------------------------------------------------------- 定義
    public struct GameBuildSettings {
        public string      outputPath;
        public BuildTarget buildTarget;
        public bool        headless;
        public bool        autoRun;
        public bool        openFolder;
        public string      compileSettings;
    }

    //-------------------------------------------------------------------------- ビルド処理
    public static void Build(GameBuildSettings gameBuildSettings) {
        if (!EditorApplication.isPlaying && !EditorApplication.isPlayingOrWillChangePlaymode) {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) {
                return;
            }
        }

        // 現在のシーン設定を保存
        var sceneSetup = EditorSceneManager.GetSceneManagerSetup();

        // ビルド環境
        var appext = "";
        switch (gameBuildSettings.buildTarget) {
        case BuildTarget.StandaloneWindows:
        case BuildTarget.StandaloneWindows64:
            appext = ".exe";
            break;
        case BuildTarget.StandaloneOSXIntel:
        case BuildTarget.StandaloneOSXIntel64:
        case BuildTarget.StandaloneOSXUniversal:
            appext = ".app";
            break;
        default:
            appext = "";
            break;
        }

        // シーン一覧を作成
        // 最初はクイックビルドのシーンから起動する。
        var scenes = new List<string>();
        scenes.AddRange(EditorBuildSettings.scenes.Select(s => s.path));

        // まとめ
        var levels     = scenes.ToArray();
        var outputPath = gameBuildSettings.outputPath + appext;
        var options    = BuildOptions.None;
        if (gameBuildSettings.headless) {
            options |= BuildOptions.EnableHeadlessMode;
        }
        if (gameBuildSettings.autoRun) {
            options |= BuildOptions.AutoRunPlayer;
        }

        // ビルド設定
        PlayerSettings.displayResolutionDialog = ResolutionDialogSetting.Disabled;
        PlayerSettings.defaultScreenWidth      = 1280;
        PlayerSettings.defaultScreenHeight     = 720;
        PlayerSettings.defaultIsFullScreen     = false;
        PlayerSettings.runInBackground         = true;
        PlayerSettings.SplashScreen.show       = false;

        // コンパイル設定のバックアップ
        CompileSettings.Backup();

        // コンパイル設定の適用
        CompileSettings.Apply(gameBuildSettings.compileSettings);

        // ビルド
        var result = BuildPipeline.BuildPlayer(levels, outputPath, gameBuildSettings.buildTarget, options);

        // コンパイル設定の復元
        CompileSettings.Restore();

        // シーン設定を復元
        if (sceneSetup.Length > 0) {
            EditorSceneManager.RestoreSceneManagerSetup(sceneSetup);
        }

        // 結果処理
        if (!string.IsNullOrEmpty(result)) {
            Debug.LogError(result);
        }

        // 成功ならフォルダを開く
        if (gameBuildSettings.openFolder) {
            OpenFolder(gameBuildSettings.outputPath);
        }
        return;
    }

    //-------------------------------------------------------------------------- ユーティリティ
    // フォルダを開く
    static void OpenFolder(string path) {
        if (Application.platform == RuntimePlatform.WindowsEditor) {
            System.Diagnostics.Process.Start("explorer.exe", "/select," + path.Replace(@"/", @"\"));//Windows
        } else {
            System.Diagnostics.Process.Start("open", path);//Mac
        }
    }
}
