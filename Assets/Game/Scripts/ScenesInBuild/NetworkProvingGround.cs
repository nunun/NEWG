﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Services.Protocols;

// ネットワーク試験場
public partial class NetworkProvingGround : GameScene {
    //-------------------------------------------------------------------------- 実装 (MonoBehaviour)
    void Awake() {
        InitLoading();
    }

    void Start() {
        loadingUI.Show();
    }
}

///////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////

// ロード処理
public partial class NetworkProvingGround {
    //-------------------------------------------------------------------------- 変数
    [SerializeField] SceneUI loadingUI = null;

    //-------------------------------------------------------------------------- ロビーの初期化、開始、停止、更新
    void InitLoading() {
        Debug.Assert(loadingUI != null, "loadingUI がない");
        loadingUI.onOpen.AddListener(() => { StartCoroutine("UpdateLoading"); });
        loadingUI.onClose.AddListener(() => { StopCoroutine("UpdateLoading"); });
    }

    IEnumerator UpdateLoading() {
        //GameAudio.PlayBGM("Abandoned");
        //GameAudio.SetBGMVolume("Abandoned", 1.0f);

        // TODO
        // ロード処理
        yield return new WaitForSeconds(3.0f);

        // ローディング完了
        loadingUI.Close();
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
