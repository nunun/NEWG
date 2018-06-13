#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System;
using System.Collections;

public class WebSocketConnectorTest : WebSocketConnector, IMonoBehaviourTest {
    //-------------------------------------------------------------------------- 変数
    [Serializable]
    public struct TestData {
        public int data;
    }

    //-------------------------------------------------------------------------- 変数
    public bool IsTestFinished { get; private set; }

    //-------------------------------------------------------------------------- 実装 (WebSocketConnectorc)
    protected override void Init() {
        base.Init();

        // NOTE
        // Start からテストを始める用
        enabled = true;
    }

    //-------------------------------------------------------------------------- 実装 (MonoBehaviour)
    IEnumerator Start() {
        bool dataType1 = false;

        // イベント登録
        AddConnectEventListner(() => {
            Debug.Log("Connected!");
        });
        AddDisconnectEventListner((error) => {
            Assert.IsNull(error, "エラー発生:" + error);
            IsTestFinished = true;
        });
        SetDataEventListener<TestData>(1, (recvTestData) => {
            Assert.AreEqual(15, recvTestData.data, "リクエストがおかしい？");
            dataType1 = true;
        });

        // 接続開始
        // テストウェブソケットサーバにつなげる
        url = "ws://localhost:7799";
        Connect();

        // Update (接続メイン処理) を実行
        while (state != State.Connected) {
            yield return null;
        }

        // type "0" はリクエストをレスポンスで返してくる。
        var sendTestData = new TestData();
        sendTestData.data = 10;
        Send<TestData,TestData>(0, sendTestData, (error,recvTestData) => {
            Assert.IsNull(error, "エラー発生: " + error);
            Assert.AreEqual(10, recvTestData.data, "レスポンスがおかしい？");
            sendTestData.data = 15;
            Send<TestData>(1, sendTestData);
        });

        // data type "1" を受信するまで待つ
        while (!dataType1) {
            yield return null;
        }

        // 完了
        Disconnect();
    }
}
#endif
