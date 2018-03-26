using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditorInternal;

// ゲームメニューアイテム
public partial class GameMenuItems {
    //-------------------------------------------------------------------------- ビルド (デバッグ)
    [MenuItem("Game/ゲーム開始", false, 0)]
    public static void PlayGame() {
        GamePlay.Play();
    }

    //-------------------------------------------------------------------------- コンパイル設定
    [MenuItem("Game/コンパイル設定", false, 100)]
    public static void CompileSetings() {
        CompileSettingsWindow.Open();
    }

    //-------------------------------------------------------------------------- ビルド (デバッグ)
    [MenuItem("Game/ビルド/デバッグ クライアント (WebGL)", false, 101)]
    public static void BuildDebugClientWebGL() {
        GameBuilder.Build(new GameBuilder.GameBuildSettings() {
            outputPath      = "Builds/Debug/Client.Debug.WebGL",
            buildTarget     = BuildTarget.WebGL,
            headless        = false,
            autoRun         = false,
            openFolder      = true,
            compileSettings = "DEBUG_CLIENT",
        });
    }

    [MenuItem("Game/ビルド/デバッグ クライアント (スタンドアローン)", false, 102)]
    public static void BuildDebugClientStandalone() {
        GameBuilder.Build(new GameBuilder.GameBuildSettings() {
            outputPath      = "Builds/Debug/Client.Debug.Standalone",
            buildTarget     = (Application.platform == RuntimePlatform.WindowsEditor)? BuildTarget.StandaloneWindows64 : BuildTarget.StandaloneOSXUniversal,
            headless        = false,
            autoRun         = true,
            openFolder      = false,
            compileSettings = "DEBUG_CLIENT",
        });
    }

    [MenuItem("Game/ビルド/デバッグ サーバ (スタンドアローン)", false, 103)]
    public static void BuildDebugServerStandalone() {
        GameBuilder.Build(new GameBuilder.GameBuildSettings() {
            outputPath      = "Builds/Debug/Server.Debug.Standalone",
            buildTarget     = (Application.platform == RuntimePlatform.WindowsEditor)? BuildTarget.StandaloneWindows64 : BuildTarget.StandaloneOSXUniversal,
            headless        = false,
            autoRun         = true,
            openFolder      = false,
            compileSettings = "DEBUG_SERVER",
        });
    }

    //-------------------------------------------------------------------------- ビルド (公開用)
    [MenuItem("Game/ビルド/公開用クライアント (WebGL)", false, 201)]
    public static void BuildReleaseClientWebGL() {
        GameBuilder.Build(new GameBuilder.GameBuildSettings() {
            outputPath      = "Services/client/Builds/Client",
            buildTarget     = BuildTarget.WebGL,
            headless        = false,
            autoRun         = false,
            openFolder      = false,
            compileSettings = "RELEASE_CLIENT",
        });
    }

    [MenuItem("Game/ビルド/公開用サーバ (Linux ヘッドレス)", false, 202)]
    public static void BuildReleaseServerLinuxHeadless() {
        GameBuilder.Build(new GameBuilder.GameBuildSettings() {
            outputPath      = "Services/server/Builds/Server",
            buildTarget     = BuildTarget.StandaloneLinux64,
            headless        = true,
            autoRun         = false,
            openFolder      = false,
            compileSettings = "RELEASE_SERVER",
        });
    }

    [MenuItem("Game/ビルド/公開用バイナリを全てビルド", false, 203)]
    public static void BuildReleaseAll() {
        BuildReleaseClientWebGL();
        BuildReleaseServerLinuxHeadless();
    }

    //-------------------------------------------------------------------------- サービス構成
    [MenuItem("Game/サービス構成/ローカルサービスを起動", false, 102)]
    public static void ServicesUp() {
        // TODO
    }

    [MenuItem("Game/サービス構成/ローカルサービスを停止", false, 103)]
    public static void ServicesDown() {
        // TODO
    }

    [MenuItem("Game/サービス構成/プロトコル定義書を編集", false, 200)]
    public static void EditProtocols() {
        AssetsUtility.EditFile("Services/services/specs.yml");
    }

    [MenuItem("Game/サービス構成/プロトコルコード生成/C# のみ", false, 201)]
    public static void GenerateProtocolsCs() {
        var commandProcess = new CommandProcess();
        commandProcess.Start("docker-compose", "run --rm generator ruby /generate.rb -c /output/cs", "Services/services");
        AssetsUtility.PingDirectory("Assets/Game/Scripts/Services/Protocols");
    }

    [MenuItem("Game/サービス構成/プロトコルコード生成/すべて", false, 202)]
    public static void GenerateProtocolsAll() {
        var commandProcess = new CommandProcess();
        commandProcess.Start("docker-compose", "run --rm generator ruby /generate.rb -c", "Services/services");
        AssetsUtility.PingDirectory("Assets/Game/Scripts/Services/Protocols");
    }
}
