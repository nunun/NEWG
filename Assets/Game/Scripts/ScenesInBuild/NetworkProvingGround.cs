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
    [SerializeField] SceneUI standbyUI = null;

    //-------------------------------------------------------------------------- ロビーの初期化、開始、停止、更新
    void InitStandby() {
        Debug.Assert(standbyUI != null, "standbyUI がない");
        standbyUI.onOpen.AddListener(() => { StartCoroutine("UpdateStandby"); });
        standbyUI.onClose.AddListener(() => { StopCoroutine("UpdateStandby"); });
    }

    IEnumerator UpdateStandby() {
        GameAudio.PlayBGM("Abandoned");
        GameAudio.SetBGMVolume("Abandoned", 1.0f);

        // マッチング結果から接続先を設定
        var networkManager = GameNetworkManager.singleton;
        networkManager.networkAddress = GameManager.ServerAddress;
        networkManager.networkPort    = GameManager.ServerPort;

        // サービス開始
        var serviceMode = GameManager.RuntimeServiceMode;
        switch (serviceMode) {
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

        // TODO
        // ロード処理
        yield return new WaitForSeconds(3.0f);

        // ローディング完了
        standbyUI.Close();
    }
}

//using (var wait = UIWait.RentFromPool()) {
//    var popup = MessagePopup.Open("これはメッセージポップアップです。", wait.Callback);
//    yield return new WaitForSeconds(2.0f);
//    popup.Close();
//    yield return wait;
//    Debug.Log(wait.error);
//}
//using (var wait = UIWait.RentFromPool()) {
//    OkPopup.Open("これは OK ポップアップです。", wait.Callback, "がんばるぞい");
//    yield return wait;
//    Debug.Log(wait.error);
//}
//using (var wait = UIWait<bool>.RentFromPool()) {
//    YesNoPopup.Open("これは OK ポップアップです。", wait.Callback, "やる", "やめた");
//    yield return wait;
//    Debug.Log(wait.error);
//    Debug.Log(wait.Value1);
//}
//[SerializeData]
//public class MyData {
//    public int val = 0;
//}
//
//[SerializeData]
//public class MyDataImporter {
//    MyData my_data = new MyData();
//}
//[Serializable]
//public class TestC {
//    public bool dirty;
//}
//
//[Serializable]
//public class TestB<T> {
//    public T t;
//}
//
////[Serializable]
//public class TestA {
//    [Serializable] public class TestD : TestB<TestC> {}
//    public TestD foo = new TestD();
//    [Serializable] public class TestE : TestB<TestC> {}
//    public TestE bar = new TestE();
//}
//static void Import<T>(string message, T data) {
//    JsonUtility.FromJsonOverwrite(message, data);
//}
//var result = new TestA();
//JsonUtility.FromJsonOverwrite("{}", result);
//Debug.Log(result.foo.t.dirty);
//JsonUtility.FromJsonOverwrite("{\"foo\":{\"t\":{\"dirty\":true}}}", result);
//Debug.Log(result.bar.t.dirty);
//Debug.Log(result.qux.t.dirty);
//yield break;
