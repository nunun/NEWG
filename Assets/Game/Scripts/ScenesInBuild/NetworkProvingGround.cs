using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Services.Protocols;

// ネットワーク試験場
public partial class NetworkProvingGround : GameScene {
    //-------------------------------------------------------------------------- 実装 (MonoBehaviour)
    void Awake() {
        InitStandby();
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
public partial class NetworkProvingGround {
    //-------------------------------------------------------------------------- 変数
    [SerializeField] SceneUI standbyUI  = null;
    [SerializeField] SceneUI exitUI     = null;
    [SerializeField] Button  exitButton = null;

    //-------------------------------------------------------------------------- ロビーの初期化、開始、停止、更新
    void InitStandby() {
        Debug.Assert(standbyUI  != null, "standbyUI がない");
        Debug.Assert(exitUI     != null, "exitUI がない");
        Debug.Assert(exitButton != null, "exitButton がない");
        standbyUI.onOpen.AddListener(() => { StartCoroutine("UpdateStandby"); });
        standbyUI.onClose.AddListener(() => { StopCoroutine("UpdateStandby"); });
        exitButton.onClick.AddListener(() => { GameSceneManager.ChangeScene("Lobby"); });
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

        // TODO
        // ロード処理
        yield return new WaitForSeconds(3.0f);

        // スタンバイ完了
        standbyUI.Close();
    }
}

///////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////

// ネットワークサービス処理
public partial class NetworkProvingGround {
    //-------------------------------------------------------------------------- 定義
    static readonly int LISTEN_PORT_MAX_RETRY = 5;
    static readonly int LISTEN_PORT_LOW       = 8000;
    static readonly int LISTEN_PORT_HIGH      = 9999;

    //-------------------------------------------------------------------------- 変数
    bool isReady = false; // サービスが開始したかどうか

    //-------------------------------------------------------------------------- ネットワークサービスの開始と停止
    // ネットワークサービスの開始
    void StartService() {
        isReady = false;
        TryStartService(LISTEN_PORT_MAX_RETRY);
    }

    void TryStartService(int retryCount) {
        var networkManager = GameNetworkManager.singleton;

        // アドレスとポート確定
        networkManager.networkAddress = GameManager.ServerAddress;
        networkManager.networkPort    = GameManager.ServerPort;

        // ポート番号にゼロを指定した場合はランダムポート
        if (networkManager.networkPort == 0) {
            networkManager.networkPort = UnityEngine.Random.Range(LISTEN_PORT_LOW, LISTEN_PORT_HIGH);
        }

        // サービス開始
        var server  = false;
        var success = true;
        switch (GameManager.RuntimeServiceMode) {
        case GameManager.ServiceMode.Client:
            Debug.Log("Start Client ...");
            server  = false;
            success = (networkManager.StartClient() != null);
            break;
        case GameManager.ServiceMode.Server:
            Debug.Log("Start Server ...");
            server  = true;
            success = networkManager.StartServer();
            break;
        case GameManager.ServiceMode.Host:
        default:
            Debug.Log("Start Host ...");
            server  = true;
            success = (networkManager.StartHost() != null);
            break;
        }

        // サーバの場合
        if (server) {
            // ポート確保失敗
            if (!success) {
                if (retryCount > 0) {
                    TryStartService(retryCount - 1); // NOTE リトライ
                    return;
                }
                GameManager.Abort("空きポートなし");
                return;
            }

            // サーバ状態を送信
            Debug.Log("サーバ状態を送信 (ready) ...");
            GameMindlinkManager.ServerStatusData.serverState = "ready";
            GameMindlinkManager.SendServerStatusData(() => {
                Debug.Log("サーバ状態送信完了");
                isReady = true; // NOTE サーバレディ
            });
            return;
        }

        // クライアントの場合
        isReady = true; // NOTE サーバレディ
    }

    // ネットワークサービスの停止
    void StopService() {
        var networkManager = GameNetworkManager.singleton;

        // サービス停止
        switch (GameManager.RuntimeServiceMode) {
        case GameManager.ServiceMode.Client:
            Debug.Log("Stop Client ...");
            networkManager.StopClient();
            break;
        case GameManager.ServiceMode.Server:
            Debug.Log("Stop Server ...");
            networkManager.StopServer();
            break;
        case GameManager.ServiceMode.Host:
        default:
            Debug.Log("Stop Host ...");
            networkManager.StopHost();
            break;
        }
    }
}
