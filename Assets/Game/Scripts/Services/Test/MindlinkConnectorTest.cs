using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System;
using System.Collections;

public class MindlinkConnectorTest : MindlinkConnector, IMonoBehaviourTest {
    //-------------------------------------------------------------------------- 変数
    [Serializable]
    public struct ServiceInfo {
        public string alias;
    }

    [Serializable]
    public struct ServiceData {
        public ServiceInfo service;
    }

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
        bool   alias     = false;
        //bool dataType0 = false; // TODO 要isrフラグ対応
        bool   received  = false;

        // イベント登録
        AddConnectEventListner(() => {
            Debug.Log("Connected!");
        });
        AddDisconnectEventListner((error) => {
            Assert.IsNull(error, "エラー発生:" + error);
            IsTestFinished = true;
        });
        // TODO isr フラグが対応したらコメントアウトを外す
        //SetDataFromRemoteEventListener<TestData>(0, (recvTestData) => {
        //    Assert.AreEqual(20, recvTestData.data, "リクエストがおかしい？");
        //    dataType0 = true;
        //});

        // 接続開始
        // テストウェブソケットサーバにつなげる
        url = "ws://localhost:7766";
        Connect();

        // Update (接続メイン処理) を実行
        while (state != State.Connected) {
            yield return null;
        }

        // エイリアスを張る
        var sendServiceData = new ServiceData();
        sendServiceData.service = new ServiceInfo();
        sendServiceData.service.alias = "a_server";
        Send<ServiceData,ServiceData>((int)MindlinkConnector.DataType.S, sendServiceData, (error,recvServiceData) => {
            Assert.IsNull(error, "エラー発生: " + error);
            alias = true;
        });

        // エイリアス待ち
        while (alias) {
            yield return null;
        }

        // type "0" はリクエストをレスポンスで返してくる。
        var sendTestData = new TestData();
        sendTestData.data = 10;
        SendToRemote<TestData,TestData>("a_server", 0, sendTestData, (error,recvTestData) => {
            Assert.IsNull(error, "エラー発生: " + error);
            Assert.AreEqual(10, recvTestData.data, "レスポンスがおかしい？");
            received = true;
        });

        // リモート送受信の完了まで待つ。
        while (/*!dataType0 || TODO 要isrフラグ対応*/ !received) {
            yield return null;
        }

        // 完了
        Disconnect();
    }
}
