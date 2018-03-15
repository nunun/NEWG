using System.IO;
using System.Linq;
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
    // バイナリ名バリアント文字列
    public const string BINARY_NAME_VARIANT_CLIENT = "Client";
    public const string BINARY_NAME_VARIANT_SERVER = "Server";
    public const string BINARY_NAME_VARIANT_HOST   = "Host";
    public const string BINARY_NAME_VARIANT_DEBUG  = "Debug";

    // 使用するポート
    public const int DEBUG_PORT   = 7777;
    public const int RELEASE_PORT = 17777;

    //---------------------------------------------------------------------- 変数
    //static GameMain.ServiceMode gameMainServiceMode = GameMain.ServiceMode.Host;
    //static bool                 isDebug             = false;

    //---------------------------------------------------------------------- 実装 (IPreprocessBuild, IPostprocessBuild)
    // ビルド前処理
    public void OnPreprocessBuild(UnityEditor.BuildTarget target, string path) {
        //Debug.Log("OnPreprocessBuild: target = " + target + ", path = " + path);
        //var binaryName = Path.GetFileNameWithoutExtension(path);
        //var variants   = binaryName.Split('.').ToList();

        // 動作モード判定
        //if (variants.IndexOf(BINARY_NAME_VARIANT_CLIENT) >= 0) {
        //    gameMainServiceMode = GameMain.ServiceMode.Client;
        //} else if (variants.IndexOf(BINARY_NAME_VARIANT_SERVER) >= 0) {
        //    gameMainServiceMode = GameMain.ServiceMode.Server;
        //} else if (variants.IndexOf(BINARY_NAME_VARIANT_HOST) >= 0) {
        //    gameMainServiceMode = GameMain.ServiceMode.Host;
        //} else {
        //    gameMainServiceMode = GameMain.ServiceMode.Host;
        //}

        // デバッグバイナリ判定
        //isDebug = (variants.IndexOf(BINARY_NAME_VARIANT_DEBUG) >= 0);
    }

    // シーン処理
    public void OnProcessScene(UnityEngine.SceneManagement.Scene scene) {
        //Debug.Log("OnProcessScene: scene = " + scene.path);
        //if (!scene.path.EndsWith("/GameMain.unity")) {
        //    return;
        //}
        //if (EditorApplication.isPlaying) {//NOTE ビルド時のみ適用
        //    return;
        //}
        //Apply(gameMainServiceMode, isDebug);
    }

    // ビルド後処理
    public void OnPostprocessBuild(BuildTarget target, string path) {
        //Debug.Log("OnPostprocessBuild: target = " + target + ", path = " + path);
    }

    // このプロセッサの実行順
    public int callbackOrder { get { return int.MaxValue; }}
}

// ゲーム設定の適用
public partial class GameBuildProcessor {
    //---------------------------------------------------------------------- モードの適用
    // ゲーム設定の適用
    static void Apply(/*GameMain.ServiceMode gameMainServiceMode,*/ bool isDebug) {
        //// ゲームメイン
        //// 動作モードとデバッグフラグを変更。
        //var gameMain = FindObjectOfType<GameMain>();
        //if (gameMain != null) {
        //    gameMain.serviceMode = gameMainServiceMode;
        //    gameMain.isDebug     = isDebug;
        //}
        //
        //// ゲームネットワークマネージャ
        //// ログレベルとサービスポートを調整。
        //var gameNetworkManager = FindObjectOfType<GameNetworkManager>();
        //if (gameNetworkManager != null) {
        //    gameNetworkManager.logLevel    = (isDebug)? LogFilter.FilterLevel.Debug : LogFilter.FilterLevel.Error;
        //    gameNetworkManager.networkPort = (isDebug)? DEBUG_PORT : RELEASE_PORT;
        //}

        // TODO
        // マインドリンクコネクタ
        // サーバでない場合はマインドリンクにサーバ状態を
        // 伝える必要がないのでオブジェクトごと削除。
        //var mindlinkConnector = FindObjectOfType<MindlinkConnector>();
        //if (mindlinkConnector != null) {
        //    if (gameMainServiceMode != GameMain.ServiceMode.Server) {
        //        GameObject.Destroy(mindlinkConnector.gameObject);
        //    }
        //}
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
