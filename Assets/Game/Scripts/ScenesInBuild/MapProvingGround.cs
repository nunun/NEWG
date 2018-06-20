using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Services.Protocols;

// マップ 試験場
public partial class MapProvingGround : GameScene {
    //-------------------------------------------------------------------------- 実装 (MonoBehaviour)
    void Awake() {
        InitStandby();
        InitBattle();
    }

    void OnDestroy() {
        // NOTE
        // ここでサービス停止
        StopService();
    }

    void Start() {
        standbyUI.Show();
    }
}

///////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////

// スタンバイ処理
public partial class MapProvingGround {
    //-------------------------------------------------------------------------- 変数
    [SerializeField] SceneUI standbyUI  = null;

    //-------------------------------------------------------------------------- ロビーの初期化、開始、停止、更新
    void InitStandby() {
        Debug.Assert(standbyUI  != null, "standbyUI がない");
        standbyUI.onOpen.AddListener(() => { StartCoroutine("UpdateStandby"); });
        standbyUI.onClose.AddListener(() => { StopCoroutine("UpdateStandby"); });
    }

    IEnumerator UpdateStandby() {
        GameAudio.PlayBGM("Abandoned");
        GameAudio.SetBGMVolume("Abandoned", 1.0f);

        // NOTE
        // ここでサービス開始
        StartService();

        // レディ待ち
        while (!isReady) {
            yield return null;
        }

        // しばらくまつ
        yield return new WaitForSeconds(1.0f);

        // スタンバイ完了
        standbyUI.Change(battleUI);
    }
}

///////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////

// バトル処理
public partial class MapProvingGround {
    //-------------------------------------------------------------------------- 変数
    [SerializeField] SceneUI battleUI   = null;
    [SerializeField] SceneUI exitUI     = null;
    [SerializeField] Button  exitButton = null;

    //-------------------------------------------------------------------------- ロビーの初期化、開始、停止、更新
    void InitBattle() {
        Debug.Assert(battleUI   != null, "battleUI がない");
        Debug.Assert(exitUI     != null, "exitUI がない");
        Debug.Assert(exitButton != null, "exitButton がない");
        battleUI.onOpen.AddListener(() => { StartCoroutine("UpdateBattle"); });
        battleUI.onClose.AddListener(() => { StopCoroutine("UpdateBattle"); });
        exitButton.onClick.AddListener(() => { GameSceneManager.ChangeScene("Lobby"); });
    }

    IEnumerator UpdateBattle() {
        if (GameSettings.RuntimeServiceMode == GameSettings.ServiceMode.Server) {
            yield break;
        }
        while (NetworkPlayer.Instance == null) {
            yield return null;
        }
        while (NetworkPlayer.Instance != null) {
            yield return null;
        }
        GameSceneManager.ChangeScene("Lobby");
    }
}

///////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////

// ネットワークサービス処理
public partial class MapProvingGround {
    //-------------------------------------------------------------------------- 定義
    static readonly int LISTEN_PORT_MAX_RETRY = 5;

    //-------------------------------------------------------------------------- 変数
    bool isReady = false; // サービスが開始したかどうか

    //-------------------------------------------------------------------------- ネットワークサービスの開始と停止
    // ネットワークサービスの開始
    void StartService() {
        isReady = false;
        TryStartService(LISTEN_PORT_MAX_RETRY);
    }

    void TryStartService(int retryCount) {
        var networkManager         = GameNetworkManager.singleton;
        var serverAddress          = GameSettings.ServerAddress;
        var serverPort             = GameSettings.ServerPort;
        var serverDiscoveryAddress = GameSettings.ServerDiscoveryAddress;
        var serverDiscoveryPort    = GameSettings.ServerDiscoveryPort;
        var serverPortRandomRange  = GameSettings.ServerPortRandomRange;

        // ポート番号にゼロを指定した場合はランダムポート
        if (serverPortRandomRange > 0) {
            var randomPort = UnityEngine.Random.Range(0, serverPortRandomRange);
            serverPort          += randomPort;
            serverDiscoveryPort += randomPort;
        }

        // サービス設定を適用
        Debug.LogFormat("サービス設定適用 (serverAddress = '{0}', serverPort = {1}) ...", serverAddress, serverPort);
        networkManager.networkAddress = serverAddress;
        networkManager.networkPort    = serverPort;

        // サービス開始
        var success = false;
        switch (GameSettings.RuntimeServiceMode) {
        case GameSettings.ServiceMode.Client:
            Debug.Log("クライアント開始 ...");
            success = (networkManager.StartClient() != null);
            break;
        case GameSettings.ServiceMode.Server:
            Debug.Log("サーバ開始 ...");
            success = networkManager.StartServer();
            break;
        case GameSettings.ServiceMode.Host:
        default:
            Debug.Log("ホスト開始 ...");
            success = (networkManager.StartHost() != null);
            break;
        }

        // クライアントの場合
        if (GameSettings.RuntimeServiceMode == GameSettings.ServiceMode.Client) {
            if (!success) {
                GameManager.Abort("サービス開始失敗");
                return;
            }
            isReady = true;//レディ
            return;
        }

        // サーバまたはホストの場合
        if (!success) {
            if (serverPortRandomRange > 0 && retryCount > 0) {
                Debug.Log("サービス開始リトライ ...");
                TryStartService(retryCount - 1);//リトライ
                return;
            }
            GameManager.Abort("空きポート無し");
            return;
        }

        // サーバ状態を送信
        GameMindlinkManager.ServerStatusData.serverState   = "ready";
        GameMindlinkManager.ServerStatusData.serverAddress = serverDiscoveryAddress;
        GameMindlinkManager.ServerStatusData.serverPort    = serverDiscoveryPort;
        GameMindlinkManager.SendServerStatusData(() => {
            isReady = true;//レディ
        });
    }

    // ネットワークサービスの停止
    void StopService() {
        var networkManager = GameNetworkManager.singleton;

        // サービス停止
        switch (GameSettings.RuntimeServiceMode) {
        case GameSettings.ServiceMode.Client:
            Debug.Log("クライアント停止 ...");
            networkManager.StopClient();
            break;
        case GameSettings.ServiceMode.Server:
            Debug.Log("サーバ停止 ...");
            networkManager.StopServer();
            break;
        case GameSettings.ServiceMode.Host:
        default:
            Debug.Log("ホスト停止 ...");
            networkManager.StopHost();
            break;
        }
    }
}
