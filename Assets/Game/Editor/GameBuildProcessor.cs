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

    //---------------------------------------------------------------------- 変数
    static GameManager.ServiceMode binaryServiceMode = GameManager.ServiceMode.Client;
    static bool                    isDebugBinary     = false;

    //---------------------------------------------------------------------- 実装 (IPreprocessBuild, IPostprocessBuild)
    // ビルド前処理
    public void OnPreprocessBuild(UnityEditor.BuildTarget target, string path) {
        Debug.Log("OnPreprocessBuild: target = " + target + ", path = " + path);
        var binaryName = Path.GetFileNameWithoutExtension(path);
        var variants   = binaryName.Split('.').ToList();

        // 動作モード判定
        if (variants.IndexOf(BINARY_NAME_VARIANT_CLIENT) >= 0) {
            binaryServiceMode = GameManager.ServiceMode.Client;
        } else if (variants.IndexOf(BINARY_NAME_VARIANT_SERVER) >= 0) {
            binaryServiceMode = GameManager.ServiceMode.Server;
        } else if (variants.IndexOf(BINARY_NAME_VARIANT_HOST) >= 0) {
            binaryServiceMode = GameManager.ServiceMode.Host;
        } else {
            binaryServiceMode = GameManager.ServiceMode.Host;
        }

        // デバッグバイナリ判定
        isDebugBinary = (variants.IndexOf(BINARY_NAME_VARIANT_DEBUG) >= 0);
    }

    // シーン処理
    public void OnProcessScene(UnityEngine.SceneManagement.Scene scene) {
        //Debug.Log("OnProcessScene: scene = " + scene.path);
        if (!scene.path.EndsWith("/GameManager.unity")) {
            return;
        }
        if (EditorApplication.isPlaying) {//NOTE ビルド時のみ適用
            return;
        }
        Apply(binaryServiceMode, isDebugBinary);
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
    static void Apply(GameManager.ServiceMode binaryServiceMode, bool isDebugBinary) {
        // ゲームメイン
        var gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null) {
            // ランタイムサービスモード
            gameManager.runtimeServiceMode = binaryServiceMode;

            // リリースビルド時
            if (!isDebugBinary) {
                // サーバ情報
                gameManager.serverAddress          = "localhost"; // 引数で指定
                gameManager.serverPort             = 7777;        // 引数で指定
                gameManager.serverPortRandomRange  = 0;           // 引数で指定
                gameManager.serverToken            = null;        // 無効に設定
                gameManager.serverSceneName        = null;        // 無効に設定
                gameManager.serverDiscoveryAddress = "localhost"; // 引数で指定
                gameManager.serverDiscoveryPort    = 7777;        // 引数で指定

                // マッチングサーバ情報
                gameManager.matchingServerUrl = null; // 無効に設定

                // マインドリンク情報
                gameManager.mindlinkUrl = "ws://mindlink:7766"; // サービス構成内から接続

                // スタンドアローンシミュレータ情報
                gameManager.standaloneSimulatorProfile = null; // 無効に設定 (リリース環境で有効にしない)
            }
        }

        // WebAPI クライアント
        var webAPIClient = FindObjectOfType<WebAPIClient>();
        if (webAPIClient != null) {

            // リリースビルド時
            if (!isDebugBinary) {
                if (binaryServiceMode == GameManager.ServiceMode.Server) {
                    webAPIClient.url = "http://api"; // コマンドラインで指定
                } else {
                    webAPIClient.url = "http://localhost:7780"; // ブラウザ URL またはコマンドラインで指定
                }
            }
        }
    }

    //-------------------------------------------------------------------------- ビルド
    // コンポーネントを取得する (アクティブでないコンポーネントも含む)
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

// ゲームネットワークマネージャ
// ログレベルとサービスポートを調整。
//var gameNetworkManager = FindObjectOfType<GameNetworkManager>();
//if (gameNetworkManager != null) {
//    gameNetworkManager.logLevel    = (isDebugBinary)? LogFilter.FilterLevel.Debug : LogFilter.FilterLevel.Error;
//    gameNetworkManager.networkPort = (isDebugBinary)? DEBUG_PORT : RELEASE_PORT;
//}
// マインドリンクコネクタ
// サーバでない場合はマインドリンクにサーバ状態を
// 伝える必要がないのでオブジェクトごと削除。
//var mindlinkConnector = FindObjectOfType<MindlinkConnector>();
//if (mindlinkConnector != null) {
//    if (binaryServiceMode != GameManager.ServiceMode.Server) {
//        GameObject.Destroy(mindlinkConnector.gameObject);
//    }
//}
