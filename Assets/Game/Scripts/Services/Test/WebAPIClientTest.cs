using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System;
using System.Collections;
using WebAPI = Services.Protocols.WebAPI;

public class WebAPIClientTest : WebAPIClient, IMonoBehaviourTest {
    //-------------------------------------------------------------------------- 変数
    public bool IsTestFinished { get; private set; }

    //-------------------------------------------------------------------------- 実装 (WebAPIClient)
    protected override void Init() {
        base.Init();
        this.url = "http://localhost:7780";

        // NOTE
        // Start からテストを始める用
        enabled = true;
    }

    //-------------------------------------------------------------------------- 実装 (MonoBehaviour)
    IEnumerator Start() {
        var isDone = false;

        // type "0" はリクエストをレスポンスで返してくる。
        WebAPI.Test(10, (err, data) => {
            Assert.IsNull(err, "err が発生 (" + err + ")");
            Assert.AreEqual(15, data.resValue, "レスポンスデータがおかしい");
            isDone = true;
        });

        // data type "1" を受信するまで待つ
        while (!isDone) {
            yield return null;
        }

        // テスト完了
        IsTestFinished = true;
    }
}
