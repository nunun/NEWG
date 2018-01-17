using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEditor;
using UnityEditor.SceneManagement;

// ゲームメニューアイテム
public partial class GameBuildMenuItems {
    //-------------------------------------------------------------------------- ビルド (デバッグ)
    [MenuItem("Game/ビルド/デバッグ クライアント (WebGL)", false, 0)]
    public static void BuildDebugClientWebGL() {
        Build(new GameBuildSettings() {
            outputPath  = "Builds/Debug/DebugClient.WebGL",
            buildTarget = BuildTarget.WebGL,
            headless    = false,
            autoRun     = false,
            openFolder  = true,
        });
    }

    [MenuItem("Game/ビルド/デバッグ クライアント (スタンドアローン)", false, 1)]
    public static void BuildDebugClientStandalone() {
        Build(new GameBuildSettings() {
            outputPath  = "Builds/Debug/DebugClient.Standalone",
            buildTarget = (Application.platform == RuntimePlatform.WindowsEditor)? BuildTarget.StandaloneWindows64 : BuildTarget.StandaloneOSXUniversal,
            headless    = false,
            autoRun     = true,
            openFolder  = false,
        });
    }

    [MenuItem("Game/ビルド/デバッグ サーバ (スタンドアローン)", false, 2)]
    public static void BuildDebugServerStandalone() {
        Build(new GameBuildSettings() {
            outputPath  = "Builds/Debug/DebugServer.Standalone",
            buildTarget = (Application.platform == RuntimePlatform.WindowsEditor)? BuildTarget.StandaloneWindows64 : BuildTarget.StandaloneOSXUniversal,
            headless    = false,
            autoRun     = true,
            openFolder  = false,
        });
    }

    //-------------------------------------------------------------------------- ビルド (公開用)
    [MenuItem("Game/ビルド/公開用クライアント (WebGL)", false, 100)]
    public static void BuildReleaseClientWebGL() {
        Build(new GameBuildSettings() {
            outputPath  = "Service/client/Builds/Client",
            buildTarget = BuildTarget.WebGL,
            headless    = false,
            autoRun     = false,
            openFolder  = false,
        });
    }

    [MenuItem("Game/ビルド/公開用サーバ (Linux ヘッドレス)", false, 101)]
    public static void BuildReleaseServerLinuxHeadless() {
        Build(new GameBuildSettings() {
            outputPath  = "Service/server/Builds/Server",
            buildTarget = BuildTarget.StandaloneLinux64,
            headless    = true,
            autoRun     = false,
            openFolder  = false,
        });
    }

    [MenuItem("Game/ビルド/公開用バイナリを全てビルド", false, 102)]
    public static void BuildReleaseAll() {
        BuildReleaseClientWebGL();
        BuildReleaseServerLinuxHeadless();
    }
}

// ゲームのビルド設定とビルド処理
public partial class GameBuildMenuItems {
    //-------------------------------------------------------------------------- 定義 (ビルド設定)
    public struct GameBuildSettings {
        public string      outputPath;
        public BuildTarget buildTarget;
        public bool        headless;
        public bool        autoRun;
        public bool        openFolder;
    }

    //-------------------------------------------------------------------------- ビルド処理
    static void Build(GameBuildSettings gameBuildSettings) {
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

        // ビルド
        var result = BuildPipeline.BuildPlayer(levels, outputPath, gameBuildSettings.buildTarget, options);
        if (!string.IsNullOrEmpty(result)) {
            Debug.LogError(result);
        }

        // シーン設定を復元して終了
        if (sceneSetup.Length > 0) {
            EditorSceneManager.RestoreSceneManagerSetup(sceneSetup);
        }

        // フォルダを開く
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

//[MenuItem("Game/ビルド/デバッグ ホスト (スタンドアローン)", false, 3)]
//public static void BuildDebugHostStandalone() {
//    Build(new GameBuildSettings() {
//        outputPath  = "Builds/Debug/DebugHost.Standalone",
//        buildTarget = (Application.platform == RuntimePlatform.WindowsEditor)? BuildTarget.StandaloneWindows64 : BuildTarget.StandaloneOSXUniversal,
//        headless    = false,
//        autoRun     = true,
//        openFolder  = false,
//    });
//}
