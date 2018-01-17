using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Callbacks;

// ゲームビルドプロセッサ
public partial class GameBuildProcessor : IPreprocessBuild, IProcessScene, IPostprocessBuild {
    //---------------------------------------------------------------------- 定義
    // サービスタイプ別バイナリ名
    public const string CLIENT_BINARY_NAME = "Client";
    public const string SERVER_BINARY_NAME = "Server";
    public const string HOST_BINARY_NAME   = "Host";

    // 種別判定用バイナリ名
    public const string DEBUG_BINARY_NAME = "Debug";

    // 使用するポート
    public const int DEBUG_PORT   = 7777;
    public const int RELEASE_PORT = 17777;

    //---------------------------------------------------------------------- 変数
    static GameMain.ServiceMode gameMainServiceMode = GameMain.ServiceMode.Host;
    static bool                 isDebug             = false;

    //---------------------------------------------------------------------- 実装 (IPreprocessBuild, IPostprocessBuild)
    // ビルド前処理
    public void OnPreprocessBuild(UnityEditor.BuildTarget target, string path) {
        //Debug.Log("OnPreprocessBuild: target = " + target + ", path = " + path);
        var binaryName = Path.GetFileNameWithoutExtension(path);

        // 動作モード判定
        if (binaryName.IndexOf(CLIENT_BINARY_NAME) >= 0) {
            gameMainServiceMode = GameMain.ServiceMode.Client;
        } else if (binaryName.IndexOf(SERVER_BINARY_NAME) >= 0) {
            gameMainServiceMode = GameMain.ServiceMode.Server;
        } else if (binaryName.IndexOf(HOST_BINARY_NAME) >= 0) {
            gameMainServiceMode = GameMain.ServiceMode.Host;
        } else {
            gameMainServiceMode = GameMain.ServiceMode.Host;
        }

        // デバッグバイナリ判定
        isDebug = (binaryName.IndexOf(DEBUG_BINARY_NAME) >= 0);
    }

    // シーン処理
    public void OnProcessScene(UnityEngine.SceneManagement.Scene scene) {
        //Debug.Log("OnProcessScene: scene = " + scene.path);
        if (!scene.path.EndsWith("/GameMain.unity")) {
            return;
        }
        if (EditorApplication.isPlaying) {//NOTE ビルド時のみ適用
            return;
        }
        Apply(gameMainServiceMode, isDebug);
    }

    // ビルド後処理
    public void OnPostprocessBuild(BuildTarget target, string path) {
        //Debug.Log("OnPostprocessBuild: target = " + target + ", path = " + path);
    }

    // 実行順
    public int callbackOrder { get { return int.MaxValue; }}
}

// ゲーム設定の適用
public partial class GameBuildProcessor {
    //---------------------------------------------------------------------- モードの適用
    // ゲーム設定の適用
    static void Apply(GameMain.ServiceMode gameMainServiceMode, bool isDebug) {
        // ゲームメイン
        var gameMain = FindObjectOfType<GameMain>();
        if (gameMain != null) {
            gameMain.serviceMode = gameMainServiceMode;
            gameMain.isDebug     = isDebug;
        }

        // ネットワークマネージャ
        var networkManager = FindObjectOfType<NetworkManager>();
        if (networkManager != null) {
            networkManager.logLevel    = (isDebug)? LogFilter.FilterLevel.Debug : LogFilter.FilterLevel.Error;
            networkManager.networkPort = (isDebug)? DEBUG_PORT : RELEASE_PORT;
        }
    }

    //-------------------------------------------------------------------------- ビルド
    // コンポーネントを取得する (Inactive なコンポーネントも含む)
    static T FindObjectOfType<T>() where T : UnityEngine.Component {
        var components = Resources.FindObjectsOfTypeAll<T>();
        foreach (var component in components) {
            var gobj = component.gameObject;
            if (   gobj.hideFlags != HideFlags.NotEditable
                && gobj.hideFlags != HideFlags.HideAndDontSave
                && !EditorUtility.IsPersistent(gobj.transform.root.gameObject)) {
                return component;
            }
        }
        return null;
    }
}
