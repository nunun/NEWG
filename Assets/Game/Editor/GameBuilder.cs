﻿using System;
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
        if (GameSettings.ImportCommandLineStringArgument("-gameConfigurationName", ref gameConfigurationNameArgument)) {
            gameConfigurationName = gameConfigurationNameArgument;
        }

        // ゲーム設定を取得
        // ゲーム設定名を指定しない場合は、現在の設定。
        var gameConfiguration = (gameConfigurationName != null)? GameConfiguration.Find(gameConfigurationName) : GameConfiguration.Load(false);
        if (gameConfiguration == null) {
            Debug.LogErrorFormat("ゲーム構成なしまたは未適用 ({0})", gameConfigurationName);
            return;
        }

        // 現在のシーン設定を保存
        var sceneSetup = EditorSceneManager.GetSceneManagerSetup();

        // ビルド環境
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
        // 最初はクイックビルドのシーンから起動する。
        var scenes = new List<string>();
        scenes.AddRange(EditorBuildSettings.scenes.Select(s => s.path));

        // まとめ
        var levels     = scenes.ToArray();
        var outputPath = gameConfiguration.outputPath + appext;
        var options    = BuildOptions.None;
        if (gameConfiguration.headless) {
            options |= BuildOptions.EnableHeadlessMode;
        }
        if (gameConfiguration.autoRun) {
            options |= BuildOptions.AutoRunPlayer;
        }

        // ビルド設定
        PlayerSettings.displayResolutionDialog = gameConfiguration.resolutionDialogSetting;
        PlayerSettings.defaultScreenWidth      = gameConfiguration.screenWidth;
        PlayerSettings.defaultScreenHeight     = gameConfiguration.screenHeight;
        PlayerSettings.defaultIsFullScreen     = gameConfiguration.isFullScreen;
        PlayerSettings.runInBackground         = gameConfiguration.runInBackground;
        PlayerSettings.SplashScreen.show       = gameConfiguration.showSplashScreen;

        // ゲーム設定をバックアップ
        GameConfiguration.Backup();

        // ゲーム設定を作成
        gameConfiguration.Save(false);

        // ビルド
        var result = BuildPipeline.BuildPlayer(levels, outputPath, gameConfiguration.buildTarget, options);

        // ゲーム設定を復元
        GameConfiguration.Restore();

        // シーン設定を復元
        if (sceneSetup.Length > 0) {
            EditorSceneManager.RestoreSceneManagerSetup(sceneSetup);
        }

        // 結果処理
        if (!string.IsNullOrEmpty(result)) {
            Debug.LogError(result);
        }

        // 成功ならフォルダを開く
        if (gameConfiguration.openFolder) {
            OpenFolder(gameConfiguration.outputPath);
        }
        return;
    }

    //-------------------------------------------------------------------------- ユーティリティ
    /// 文字列の引数を取得
    public static string GetStringArgument(string key, string def = null) {
        string[] args = System.Environment.GetCommandLineArgs();
        int index = System.Array.IndexOf(args, key);
        if (index < 0 || (index + 1) >= args.Length) {
            return def;
        }
        return args[index + 1];
    }

    // フォルダを開く
    static void OpenFolder(string path) {
        if (Application.platform == RuntimePlatform.WindowsEditor) {
            System.Diagnostics.Process.Start("explorer.exe", "/select," + path.Replace(@"/", @"\"));//Windows
        } else {
            System.Diagnostics.Process.Start("open", path);//Mac
        }
    }
}
