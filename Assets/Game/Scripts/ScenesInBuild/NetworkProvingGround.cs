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
    //-------------------------------------------------------------------------- ネットワークサービスの開始と停止
    // ネットワークサービスの開始
    void StartService() {
        var networkManager = GameNetworkManager.singleton;
        var serverAddress  = GameManager.ServerAddress;
        var serverPort     = GameManager.ServerPort;

        // TODO
        // ここで serverAddress と serverPort を決定する。
        // serverPort が 0 の場合は、自動的にポート番号を決める。
        // サーバまたはホストの場合は、決まったポートも含めて
        // サーバ状態 ready をマインドリンクで送信。
        // さらにシーンの初期化が終わったら、ServerSetupDoneMessage を送信。

        // アドレスとポート確定
        networkManager.networkAddress = serverAddress;
        networkManager.networkPort    = serverPort;

        // サービス開始
        switch (GameManager.RuntimeServiceMode) {
        case GameManager.ServiceMode.Client:
            Debug.Log("Start Client ...");
            networkManager.StartClient();
            break;
        case GameManager.ServiceMode.Server:
            Debug.Log("Start Server ...");
            networkManager.StartServer();
            break;
        case GameManager.ServiceMode.Host:
        default:
            Debug.Log("Start Host ...");
            networkManager.StartHost();
            break;
        }
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
