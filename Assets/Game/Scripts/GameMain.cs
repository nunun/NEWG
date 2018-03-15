using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ゲームのメイン処理
// ここからゲームが始まる
public class GameMain : MonoBehaviour {
    //-------------------------------------------------------------------------- 実装 (MonoBehaviour)
    void Start() {
        GameSceneManager.ChangeScene("Logo");
    }
}

// ゲームのメイン処理
// ここからゲームが始まる
//public class GameMain : MonoBehaviour {
//    //-------------------------------------------------------------------------- 定義
//    public enum ServiceMode { Client, Server, Host }; // 動作モード
//
//    //-------------------------------------------------------------------------- 変数
//    public ServiceMode serviceMode = ServiceMode.Host; // サービスモード
//    public bool        isDebug     = false;            // デバッグフラグ
//    public TitleUI     titleUI     = null;             // タイトルの UI
//    public BattleUI    battleUI    = null;             // バトルの UI
//    public ResultUI    resultUI    = null;             // 結果画面の UI
//
//    // ゲームメインのインスタンス
//    static GameMain instance = null;
//
//    // ゲームメインのインスタンスの取得
//    public static GameMain Instance { get { return instance; }}
//
//    //-------------------------------------------------------------------------- 実装 (MonoBehaviour)
//    void Awake() {
//        // インスタンス特定
//        if (instance != null) {
//            GameObject.Destroy(this.gameObject);
//            return;
//        }
//        instance = this;
//
//        // シーンから各種 UI の取得
//        Debug.Assert(titleUI  != null, "タイトル UI なし");
//        Debug.Assert(battleUI != null, "バトル UI なし");
//        Debug.Assert(resultUI != null, "結果 UI なし");
//
//        // デフォルト術tえ非表示
//        titleUI.gameObject.SetActive(false);
//        battleUI.gameObject.SetActive(false);
//        resultUI.gameObject.SetActive(false);
//    }
//
//    void Start() {
//        // タイトル画面へ
//        StartTitle();
//    }
//
//    void OnDestroy() {
//        // インスタンス解除
//        if (instance == this) {
//            instance = null;
//        }
//    }
//
//    //-------------------------------------------------------------------------- ゲーム開始
//    // タイトル画面へ
//    public void StartTitle() {
//        // カメラを初期化
//        GameCamera.Instance.SetMenuMode();
//
//        // NOTE サーバの場合はサービスをすぐに開始する
//        if (serviceMode == ServiceMode.Server) {
//            StartBattle();
//            return;
//        }
//
//        // タイトル UI を表示してゲームを開始を促す
//        // UI のゲーム開始ボタンを押したら StartBattle を発火する
//        titleUI.gameObject.SetActive(true);
//        battleUI.gameObject.SetActive(false);
//        resultUI.gameObject.SetActive(false);
//    }
//
//    // バトル画面へ
//    public void StartBattle() {
//        var networkManager = NetworkManager.singleton;
//        Debug.Assert(networkManager != null, "ネットワークマネージャなし？");
//
//        // メニュー調整
//        titleUI.gameObject.SetActive(false);
//        battleUI.gameObject.SetActive(true);
//        resultUI.gameObject.SetActive(false);
//
//        // 接続先をチェック
//        // WebGL プレイヤーの時はブラウザのホスト名をチェック。
//        // ただし、ローカルホスト上の Docker で動くサーバに接続する場合は
//        // サーバが network_mode=host で動作するので、OS 上の moby linux (10.0.75.2) に接続する。
//        if (!isDebug && Application.platform == RuntimePlatform.WebGLPlayer) {
//            var hostName = WebBrowser.GetLocationHostName();
//            if (hostName == "localhost") {
//                networkManager.networkAddress = "10.0.75.2";
//            } else {
//                networkManager.networkAddress = hostName;
//            }
//        }
//
//        // TODO
//        // サーバならマインドリンクを開始する
//        //if (serviceMode == ServiceMode.Server) {
//        //    var mindlinkConnector = MindlinkConnector.Instance;
//        //    Debug.Assert(mindlinkConnector != null, "マインドリンクコネクタなし？");
//        //    mindlinkConnector.StartConnect();
//        //    // 仮のサーバ状態設定
//        //    var currentServerState = MindlinkServerState.CurrentServerState;
//        //    currentServerState.parameter = 100;
//        //    currentServerState.Publish();
//        //}
//
//        // サービス開始
//        switch (serviceMode) {
//        case ServiceMode.Client:
//            Debug.Log("Start Client");
//            networkManager.StartClient();
//            break;
//        case ServiceMode.Server:
//            Debug.Log("Start Server");
//            networkManager.StartServer();
//            break;
//        case ServiceMode.Host:
//        default:
//            Debug.Log("Start Host");
//            networkManager.StartHost();
//            break;
//        }
//    }
//
//    // 結果画面へ
//    public void StartResult(int killPoint) {
//        // カメラを初期化
//        GameCamera.Instance.SetResultMode();
//
//        // メニュー調整
//        titleUI.gameObject.SetActive(false);
//        battleUI.gameObject.SetActive(false);
//        resultUI.gameObject.SetActive(true);
//
//        // スコア設定
//        resultUI.SetKillPoint(killPoint);
//    }
//
//    // リスポーン
//    public void Respawn() {
//        // メニュー調整
//        titleUI.gameObject.SetActive(false);
//        battleUI.gameObject.SetActive(true);
//        resultUI.gameObject.SetActive(false);
//
//        // リスポーン
//        NetworkPlayer.Instance.player.Spawn();
//    }
//}
