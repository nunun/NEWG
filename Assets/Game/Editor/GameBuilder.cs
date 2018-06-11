using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEditor;
using UnityEditor.SceneManagement;

// ゲームビルダー
public partial class GameBuilder {
    //-------------------------------------------------------------------------- ビルド
    public static void Build(string gameConfigurationName = null) {
        if (!EditorApplication.isPlaying && !EditorApplication.isPlayingOrWillChangePlaymode) {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) {
                return;
            }
        }

        // ゲーム設定名をコマンドライン引数から確認
        // 引数で指定されている場合は、そちらを優先的に使用する。
        var gameConfigurationNameArgument = default(string);
        if (GameManager.ImportCommandLineStringArgument("-gameConfigurationName", ref gameConfigurationNameArgument)) {
            gameConfigurationName = gameConfigurationNameArgument;
        }

        // ゲーム設定を取得
        // ゲーム設定名を指定しない場合は、現在の設定。
        var gameConfiguration = (gameConfigurationName != null)? GameConfiguration.Find(gameConfigurationName) : GameConfiguration.Load(false);
        if (gameConfiguration == null) {
            Debug.LogErrorFormat("ゲーム構成なしまたは未適用 ({0})", gameConfigurationName);
            return;
        }

        // エラーチェック
        if (gameConfiguration.headless && gameConfiguration.developmentBuild) {
            Debug.LogErrorFormat("ヘッドレスかつ開発ビルドはできない ({0})", gameConfigurationName);
            return;
        }

        // ビルドターゲットと拡張子
        var appext = "";
        switch (gameConfiguration.buildTarget) {
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
        var scenes = new List<string>();
        scenes.AddRange(EditorBuildSettings.scenes.Select(s => s.path));

        // ビルドオプション設定
        var options = BuildOptions.None;
        if (gameConfiguration.headless) {
            options |= BuildOptions.EnableHeadlessMode;
        }
        if (gameConfiguration.autoRun) {
            options |= BuildOptions.AutoRunPlayer;
        }
        if (gameConfiguration.developmentBuild) {
            options |= BuildOptions.Development;
            options |= BuildOptions.AllowDebugging;
        }

        // プレイヤー設定
        PlayerSettings.displayResolutionDialog = gameConfiguration.resolutionDialogSetting;
        PlayerSettings.defaultScreenWidth      = gameConfiguration.screenWidth;
        PlayerSettings.defaultScreenHeight     = gameConfiguration.screenHeight;
        PlayerSettings.defaultIsFullScreen     = gameConfiguration.isFullScreen;
        PlayerSettings.runInBackground         = gameConfiguration.runInBackground;
        PlayerSettings.SplashScreen.show       = gameConfiguration.showSplashScreen;

        // 現在のシーン設定を保存
        var sceneSetup = EditorSceneManager.GetSceneManagerSetup();

        // ゲーム設定をバックアップ
        GameConfiguration.Backup();

        // ゲーム設定を作成
        gameConfiguration.Save(false);

        // サーバ停止
        if (!string.IsNullOrEmpty(gameConfiguration.localServerStopUrl)) {
            SendGetRequest(gameConfiguration.localServerStopUrl);
        }

        // 出力先調整
        var outputPath = gameConfiguration.outputPath;
        CreateOutputDirectory(outputPath);
        CleanOutputFiles(outputPath);

        // ビルド
        var buildScenes     = scenes.ToArray();
        var buildOutputPath = outputPath + appext;
        var buildTarget     = gameConfiguration.buildTarget;
        var buildOptions    = options;
        var result = BuildPipeline.BuildPlayer(buildScenes, buildOutputPath, buildTarget, buildOptions);

        // ゲーム設定を復元
        GameConfiguration.Restore();

        // シーン設定を復元
        if (sceneSetup.Length > 0) {
            EditorSceneManager.RestoreSceneManagerSetup(sceneSetup);
        }

        // ビルド失敗時
        if (!string.IsNullOrEmpty(result)) {
            Debug.LogError(result);
        }

        // ビルド成功時
        if (string.IsNullOrEmpty(result)) {
            // サーバ再開
            if (!string.IsNullOrEmpty(gameConfiguration.localServerStartUrl)) {
                SendGetRequest(gameConfiguration.localServerStartUrl);
            }

            // フォルダを開く
            if (gameConfiguration.openFolder) {
                OpenFolder(gameConfiguration.outputPath);
            }
        }
        return;
    }

    //-------------------------------------------------------------------------- ユーティリティ
    // 出力ディレクトリ作成
    static void CreateOutputDirectory(string outputPath) {
        var outputDir = Path.GetDirectoryName(outputPath);
        if (!Directory.Exists(outputDir)) {
            Directory.CreateDirectory(outputDir);
        }
    }

    // 出力ファイルクリーン
    static void CleanOutputFiles(string outputPath) {
        var outputDir = Path.GetDirectoryName(outputPath);
        if (Directory.Exists(outputDir)) {
            var files = Directory.GetFiles(outputDir, Path.GetFileName(outputPath) + ".*");
            foreach (var file in files) {
                var filePath = file.Replace(@"\", @"/");
                var fileExt  = Path.GetExtension(filePath);
                if ((outputPath + fileExt) == filePath) {
                    File.Delete(filePath);
                }
            }
        }
        if (Directory.Exists(outputPath)) {
            Directory.Delete(outputPath, true);
        }
        var dataPath = outputPath + "_Data";
        if (Directory.Exists(dataPath)) {
            Directory.Delete(dataPath, true);
        }
    }

    // ウェブサーバに GET リクエスト送信
    static string SendGetRequest(string url) {
        using(UnityWebRequest request = UnityWebRequest.Get(url)) {
            request.Send();
            while (!request.isDone) {}
            if(request.isError) {
                return request.error;
            }
            return request.downloadHandler.text;
        }
    }

    // フォルダを開く
    static void OpenFolder(string path) {
        if (File.Exists(path)) {
            path = Path.GetDirectoryName(path);
        }
        if (Application.platform == RuntimePlatform.WindowsEditor) {
            System.Diagnostics.Process.Start("explorer.exe", path.Replace(@"/", @"\"));//Windows
        } else {
            System.Diagnostics.Process.Start("open", path);//Mac
        }
    }
}
