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
    //-------------------------------------------------------------------------- ビルド
    public static void Build(string schemeName = null) {
        if (!EditorApplication.isPlaying && !EditorApplication.isPlayingOrWillChangePlaymode) {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) {
                return;
            }
        }

        // スキーム名をコマンドライン引数から確認
        // 引数で指定されている場合は、そちらを優先的に使用する。
        var schemeNameArgument = GetStringArgument("-schemeName");
        if (schemeNameArgument != null) {
            schemeName = schemeNameArgument;
        }

        // ゲーム設定の適用
        var gameSettingsOld = GameSettings.Load(false);
        var gameSettings    = (schemeName != null)? GameSettings.GetScheme(schemeName) : gameSettingsOld;
        if (gameSettings == null) {
            Debug.LogErrorFormat("ゲーム設定が不明または未適用 ({0})", schemeName);
            return;
        }
        gameSettings.Save(false);

        // 現在のシーン設定を保存
        var sceneSetup = EditorSceneManager.GetSceneManagerSetup();

        // ビルド環境
        var appext = "";
        switch (gameSettings.buildTarget) {
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
        var outputPath = gameSettings.buildOutputPath + appext;
        var options    = BuildOptions.None;
        if (gameSettings.buildHeadless) {
            options |= BuildOptions.EnableHeadlessMode;
        }
        if (gameSettings.buildAutoRun) {
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
        var result = BuildPipeline.BuildPlayer(levels, outputPath, gameSettings.buildTarget, options);

        // ゲーム設定を元に戻す
        // 元々無かった場合は消す
        if (gameSettingsOld != null) {
            gameSettingsOld.Save(false);
        } else {
            GameSettings.Remove();
        }

        // シーン設定を復元
        if (sceneSetup.Length > 0) {
            EditorSceneManager.RestoreSceneManagerSetup(sceneSetup);
        }

        // 結果処理
        if (!string.IsNullOrEmpty(result)) {
            Debug.LogError(result);
        }

        // 成功ならフォルダを開く
        if (gameSettings.buildOpenFolder) {
            OpenFolder(gameSettings.buildOutputPath);
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
