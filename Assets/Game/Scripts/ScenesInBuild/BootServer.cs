using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Services.Protocols.Models;

// サーバ起動
public class BootServer : GameScene {
    //-------------------------------------------------------------------------- 変数
    ServerSetupRequest serverSetupRequest = null; // サーバ起動リクエスト

    //-------------------------------------------------------------------------- 実装 (MonoBehaviour)
    IEnumerator Start() {
        #if NETWORK_EMULATION_MODE
        // ネットワークエミュレーションモード対応
        // エミュレーション時はそのままサーバシーンへ
        var isNetworkEmulationMode = true;
        if (isNetworkEmulationMode) {
            GameSceneManager.ChangeSceneImmediately(serverSetupRequest.sceneName);
            yield break;
        }
        #endif

        // マインドリンクコネクタ取得
        var connector = MindlinkConnector.GetConnector();

        // イベント設定
        connector.AddConnectEventListner(() => {
            Debug.Log("マインドリンク接続完了");
        });
        connector.AddDisconnectEventListner((error) => {
            Debug.Log("マインドリンク切断");
            Debug.LogError(error);
            GameManager.Quit(); // NOTE マインドリンク切断でサーバ強制終了
        });
        connector.SetDataFromRemoteEventListener<ServerSetupRequest,ServerSetupResponse>(0, (req,res) => {
            Debug.Log("サーバセットアップリクエスト受信");
            serverSetupRequest = req; // NOTE リクエストを記録
            var serverSetupResponse = new ServerSetupResponse();
            serverSetupResponse.matchingId = req.matchingId;
            res.Send(serverSetupResponse);
        });

        // マインドリンクへ接続
        Debug.Log("マインドリンクへ接続 ...");
        connector.url = "ws://localhost:7766";
        connector.Connect();

        // 接続を待つ
        Debug.Log("マインドリンクへの接続をまっています ...");
        while (!connector.IsConnected) {
            yield return null;
        }

        // サーバ状態を送信
        Debug.Log("サーバステータスを送信 (Standby) ...");
        var serverStatus = new ServerStatus();
        serverStatus.state = "Standby";
        connector.SendStatus(serverStatus, (error) => {
            Debug.Log("サーバステータス送信完了");
            if (error != null) {
                Debug.LogError(error);
                GameManager.Quit();
            }
        });

        // 起動パラメータを受け付けるまで待つ
        Debug.Log("サーバセットアップリクエストを待っています ...");
        while (serverSetupRequest == null) {
            yield return null;
        }

        // シーン切り替え
        Debug.Log("シーンを切り替え (" + serverSetupRequest.sceneName + ") ...");
        GameSceneManager.ChangeSceneImmediately(serverSetupRequest.sceneName);

        // TODO
        // 先のシーンでネットワークマネージャを起動して、
        // ポート番号が判明次第 ServerReadyRequest を API サーバに送信。
    }
}
