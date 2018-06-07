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
    //-------------------------------------------------------------------------- ゲーム開始
    [MenuItem("Game/ゲーム開始", false, 0)]
    public static void PlayGame() {
        GamePlay.Play();
    }

    //-------------------------------------------------------------------------- ゲーム構成
    [MenuItem("Game/ゲーム構成...", false, 100)]
    public static void GameConfiguration() {
        GameConfigurationWindow.Open();
    }

    //-------------------------------------------------------------------------- ビルド
    [MenuItem("Game/ビルド/デバッグ クライアント (WebGL)", false, 101)]
    public static void BuildDebugClientWebGL() {
        GameBuilder.Build("DEBUG_CLIENT"); // TODO ゲーム設定調整
    }

    [MenuItem("Game/ビルド/デバッグ クライアント (スタンドアローン)", false, 102)]
    public static void BuildDebugClientStandalone() {
        GameBuilder.Build("DEBUG_CLIENT"); // TODO ゲーム設定調整
    }

    [MenuItem("Game/ビルド/デバッグ サーバ (スタンドアローン)", false, 103)]
    public static void BuildDebugServerStandalone() {
        GameBuilder.Build("DEBUG_SERVER"); // TODO ゲーム設定調整
    }

    [MenuItem("Game/ビルド/公開用クライアント (WebGL)", false, 200)]
    public static void BuildReleaseClientWebGL() {
        GameBuilder.Build("RELEASE_CLIENT"); // TODO ゲーム設定調整
    }

    [MenuItem("Game/ビルド/公開用サーバ (Linux ヘッドレス)", false, 201)]
    public static void BuildReleaseServerLinuxHeadless() {
        GameBuilder.Build("RELEASE_SERVER"); // TODO ゲーム設定調整
    }

    [MenuItem("Game/ビルド/公開用バイナリを全てビルド", false, 202)]
    public static void BuildReleaseAll() {
        BuildReleaseClientWebGL();
        BuildReleaseServerLinuxHeadless();
    }

    //-------------------------------------------------------------------------- ローカルサービス
    [MenuItem("Game/サービス/ローカルサービス/起動", false, 102)]
    public static void ServicesUp() {
        var commandProcess = new CommandProcess();
        var env = new Dictionary<string,string>() {{"&NONBLOCK", "1"}};
        commandProcess.Start("docker-compose", "up", "Services", env);
    }

    [MenuItem("Game/サービス/ローカルサービス/停止", false, 103)]
    public static void ServicesDown() {
        var commandProcess = new CommandProcess();
        var env = new Dictionary<string,string>() {{"&NONBLOCK", "1"}};
        commandProcess.Start("docker-compose", "down", "Services", env);
    }

    [MenuItem("Game/サービス/ローカルサービス/ビルド", false, 200)]
    public static void ServicesBuild() {
        var commandProcess = new CommandProcess();
        commandProcess.Start("docker-compose", "build", "Services");
    }

    [MenuItem("Game/サービス/サービスプロトコル/プロトコルコード生成/C# のみ", false, 103)]
    public static void GenerateProtocolsCs() {
        var commandProcess = new CommandProcess();
        commandProcess.Start("docker-compose", "run --rm generator ruby /generate.rb -c /output/cs", "Services/services");
        AssetsUtility.PingDirectory("Assets/Game/Scripts/Services/Protocols");
    }

    [MenuItem("Game/サービス/サービスプロトコル/プロトコルコード生成/すべて", false, 104)]
    public static void GenerateProtocolsAll() {
        var commandProcess = new CommandProcess();
        commandProcess.Start("docker-compose", "run --rm generator ruby /generate.rb -c", "Services/services");
        AssetsUtility.PingDirectory("Assets/Game/Scripts/Services/Protocols");
    }

    [MenuItem("Game/サービス/サービスプロトコル/プロトコル定義書を編集", false, 104)]
    public static void EditProtocols() {
        AssetsUtility.EditFile("Services/services/specs.yml");
    }

    //-------------------------------------------------------------------------- セーブデータ
    [MenuItem("Game/セーブデータ/セーブデータをクリア", false, 103)]
    public static void ClearSaveData() {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("セーブデータをクリアしました。");
    }

    //-------------------------------------------------------------------------- シーン編集
    [MenuItem("Game/シーン編集/uGUI/アンカーを現在位置にセット &]", false, 200)]
    public static void SetAnchorToCurrentPosition() {
        var o = Selection.activeGameObject;
        if (o != null && o.GetComponent<RectTransform>() != null) {
            var p = o.transform.parent.GetComponent<RectTransform>();
            var r = o.GetComponent<RectTransform>();

            var parentWidth  = p.rect.width;
            var parentHeight = p.rect.height;

            var offsetMin = r.offsetMin;
            var offsetMax = r.offsetMax;
            var anchorMin = r.anchorMin;
            var anchorMax = r.anchorMax;

            var targetAnchorMin = new Vector2(anchorMin.x + (offsetMin.x / parentWidth), anchorMin.y + (offsetMin.y / parentHeight));
            var targetAnchorMax = new Vector2(anchorMax.x + (offsetMax.x / parentWidth), anchorMax.y + (offsetMax.y / parentHeight));

            r.anchorMin = targetAnchorMin;
            r.anchorMax = targetAnchorMax;
            r.offsetMin = new Vector2(0, 0);
            r.offsetMax = new Vector2(1, 1);
            r.pivot     = new Vector2(0.5f, 0.5f);
        }
    }

    [MenuItem("Game/シーン編集/uGUI/アンカーを中心にセット &^", false, 201)]
    public static void SetAnchorToCenterPosition() {
        var o = Selection.activeGameObject;
        if (o != null && o.GetComponent<RectTransform>() != null) {
            var p = o.transform.parent.GetComponent<RectTransform>();
            var r = o.GetComponent<RectTransform>();

            var parentWidth  = p.rect.width;
            var parentHeight = p.rect.height;

            var x      = r.position.x;
            var y      = r.position.y;
            var width  = r.rect.width;
            var height = r.rect.height;

            var targetAnchorMin = new Vector2(x / parentWidth, y / parentHeight);
            var targetAnchorMax = new Vector2(x / parentWidth, y / parentHeight);

            r.anchorMin = targetAnchorMin;
            r.anchorMax = targetAnchorMax;
            r.offsetMin = new Vector2(-width / 2.0f, -height / 2.0f);
            r.offsetMax = new Vector2( width / 2.0f,  height / 2.0f);
            r.pivot     = new Vector2(0.5f, 0.5f);
        }
    }
}
