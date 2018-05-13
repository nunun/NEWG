using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Services.Protocols.Models;

// サーバ起動
public class BootServer : GameScene {
    //-------------------------------------------------------------------------- 変数
    ServerSetupRequestMessage serverSetupRequestMessage = null; // サーバセットアップリクエストメッセージ

    //-------------------------------------------------------------------------- 実装 (MonoBehaviour)
    IEnumerator Start() {
        #if STANDALONE_MODE
        // ネットワークエミュレーションモード時
        if (GameManager.IsStandaloneMode) {
            GameSceneManager.ChangeSceneImmediately(GameManager.ServerSceneName);
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
        connector.SetDataFromRemoteEventListener<ServerSetupRequestMessage,ServerSetupResponseMessage>(0, (req,res) => {
            Debug.Log("サーバ セットアップ リクエスト メッセージ受信");
            serverSetupRequestMessage = req; // NOTE リクエストを記録
            var serverSetupResponseMessage = new ServerSetupResponseMessage();
            serverSetupResponseMessage.matchToken = req.matchToken;
            res.Send(serverSetupResponseMessage);
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
        Debug.Log("サーバ ステータス データを送信 (standby) ...");
        var serverStatusData = new ServerStatusData();
        serverStatusData.serverState = "standby";
        connector.SendStatus(serverStatusData, (error) => {
            Debug.Log("サーバ ステータス データ送信完了");
            if (error != null) {
                Debug.LogError(error);
                GameManager.Quit();
            }
        });

        // 起動パラメータを受け付けるまで待つ
        Debug.Log("サーバセットアップリクエストを待っています ...");
        while (serverSetupRequestMessage == null) {
            yield return null;
        }

        // シーン切り替え
        Debug.Log("シーンを切り替え (" + serverSetupRequestMessage.sceneName + ") ...");
        GameSceneManager.ChangeSceneImmediately(serverSetupRequestMessage.sceneName);

        // TODO
        // 先のシーンでネットワークマネージャを起動して、
        // ポート番号が判明次第 ServerSetupDoneMessage を API サーバに送信。
    }
}
