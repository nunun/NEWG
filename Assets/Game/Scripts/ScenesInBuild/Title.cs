using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Services.Protocols;

// タイトル
public class Title : GameScene {
    //-------------------------------------------------------------------------- 実装 (MonoBehaviour)
    IEnumerator Start() {
        GameAudio.PlayBGM("Revenge1");
        GameAudio.SetBGMVolume("Revenge1", 1.0f);

        using (var subject = StatusLine.Observe()) {
            subject.message = "初期化中 ...";
            yield return new WaitForSeconds(1.0f);
            subject.message = "ロード中 ...";
            yield return new WaitForSeconds(1.0f);
        }

        using (var wait = UIWait<WebAPI.SignupResponse>.RentFromPool()) {
            WebAPI.Signup("i_am_tester", wait.Callback);
            yield return wait;
            Debug.Log(wait.error);
        }
        Debug.Log(GameDataManager.PlayerData.playerName);

        using (var wait = UIWait.RentFromPool()) {
            var popup = MessagePopup.Open("これはメッセージポップアップです。", wait.Callback);
            yield return new WaitForSeconds(2.0f);
            popup.Close();
            yield return wait;
            Debug.Log(wait.error);
        }

        using (var wait = UIWait.RentFromPool()) {
            OkPopup.Open("これは OK ポップアップです。", wait.Callback, "がんばるぞい");
            yield return wait;
            Debug.Log(wait.error);
        }

        using (var wait = UIWait<bool>.RentFromPool()) {
            YesNoPopup.Open("これは OK ポップアップです。", wait.Callback, "やる", "やめた");
            yield return wait;
            Debug.Log(wait.error);
            Debug.Log(wait.Value1);
        }

        //GameAudio.MixBGM("Revenge2");
    }
}

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
