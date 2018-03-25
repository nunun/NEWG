﻿using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditorInternal;

// ゲームメニューアイテム
public class GameMenuItems {
    //-------------------------------------------------------------------------- ビルド (デバッグ)
    [MenuItem("Game/ゲーム開始", false, 0)]
    public static void PlayGame() {
        GamePlay.Play();
    }

    //-------------------------------------------------------------------------- ビルド (デバッグ)
    [MenuItem("Game/ビルド/デバッグ クライアント (WebGL)", false, 100)]
    public static void BuildDebugClientWebGL() {
        GameBuilder.Build(new GameBuilder.GameBuildSettings() {
            outputPath  = "Builds/Debug/Client.Debug.WebGL",
            buildTarget = BuildTarget.WebGL,
            headless    = false,
            autoRun     = false,
            openFolder  = true,
        });
    }

    [MenuItem("Game/ビルド/デバッグ クライアント (スタンドアローン)", false, 101)]
    public static void BuildDebugClientStandalone() {
        GameBuilder.Build(new GameBuilder.GameBuildSettings() {
            outputPath  = "Builds/Debug/Client.Debug.Standalone",
            buildTarget = (Application.platform == RuntimePlatform.WindowsEditor)? BuildTarget.StandaloneWindows64 : BuildTarget.StandaloneOSXUniversal,
            headless    = false,
            autoRun     = true,
            openFolder  = false,
        });
    }

    [MenuItem("Game/ビルド/デバッグ サーバ (スタンドアローン)", false, 102)]
    public static void BuildDebugServerStandalone() {
        GameBuilder.Build(new GameBuilder.GameBuildSettings() {
            outputPath  = "Builds/Debug/Server.Debug.Standalone",
            buildTarget = (Application.platform == RuntimePlatform.WindowsEditor)? BuildTarget.StandaloneWindows64 : BuildTarget.StandaloneOSXUniversal,
            headless    = false,
            autoRun     = true,
            openFolder  = false,
        });
    }

    //-------------------------------------------------------------------------- ビルド (公開用)
    [MenuItem("Game/ビルド/公開用クライアント (WebGL)", false, 201)]
    public static void BuildReleaseClientWebGL() {
        GameBuilder.Build(new GameBuilder.GameBuildSettings() {
            outputPath  = "Services/client/Builds/Client",
            buildTarget = BuildTarget.WebGL,
            headless    = false,
            autoRun     = false,
            openFolder  = false,
        });
    }

    [MenuItem("Game/ビルド/公開用サーバ (Linux ヘッドレス)", false, 202)]
    public static void BuildReleaseServerLinuxHeadless() {
        GameBuilder.Build(new GameBuilder.GameBuildSettings() {
            outputPath  = "Services/server/Builds/Server",
            buildTarget = BuildTarget.StandaloneLinux64,
            headless    = true,
            autoRun     = false,
            openFolder  = false,
        });
    }

    [MenuItem("Game/ビルド/公開用バイナリを全てビルド", false, 203)]
    public static void BuildReleaseAll() {
        BuildReleaseClientWebGL();
        BuildReleaseServerLinuxHeadless();
    }

    //-------------------------------------------------------------------------- サービス構成
    [MenuItem("Game/サービス構成/ローカルサービスを起動", false, 101)]
    public static void ServicesUp() {
        // TODO
    }

    [MenuItem("Game/サービス構成/ローカルサービスを停止", false, 102)]
    public static void ServicesDown() {
        // TODO
    }

    [MenuItem("Game/サービス構成/プロトコル定義書を編集", false, 200)]
    public static void EditProtocols() {
        InternalEditorUtility.OpenFileAtLineExternal("Services/services/specs.yml", 1);
    }

    [MenuItem("Game/サービス構成/プロトコルコード生成", false, 201)]
    public static void GenerateProtocols() {
        var commandProcess = new CommandProcess();
        commandProcess.Start("docker-compose", "run --rm generator ruby /generate.rb -c", "Services/services");
    }
}
