﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Services.Protocols;

// ロビー
public class Lobby : GameScene {
    //-------------------------------------------------------------------------- インスタンスの確保と解放
    public CanvasGroupAlphaEffect uiAlphaEffect   = null;
    public Button                 gameStartButton = null;

    //-------------------------------------------------------------------------- 実装 (MonoBehaviour)
    void Awake() {
        Debug.Assert(uiAlphaEffect   != null, "uiAlphaEffect がない");
        Debug.Assert(gameStartButton != null, "gameStartButton がない");

        uiAlphaEffect.SetUneffected();
    }

    IEnumerator Start() {
        GameAudio.PlayBGM("Abandoned");
        GameAudio.SetBGMVolume("Abandoned", 1.0f);

        // セットアップ
        var subject = StatusLine.Observe();
        try {
            // ロード中
            subject.message = "ロード中 ...";
            yield return new WaitForSeconds(1.0f);

            // サインイン
            var signinToken = GameDataManager.CredentialData.signinToken;
            do {
                subject.message = "サーバに接続しています ...";

                // サインアップしていないならサインイン
                // そうでないならサインアップ
                var error = default(string);
                if (string.IsNullOrEmpty(signinToken)) {
                    using (var wait = UIWait<WebAPI.SignupResponse>.RentFromPool()) {
                        WebAPI.Signup(wait.Callback);
                        yield return wait;
                        error = wait.error;
                    }
                } else {
                    using (var wait = UIWait<WebAPI.SigninResponse>.RentFromPool()) {
                        WebAPI.Signin(signinToken, wait.Callback);
                        yield return wait;
                        error = wait.error;
                    }
                }
                if (error == default(string)) {
                    subject.message = "接続完了！";
                    GameAudio.Play("BootUp");
                    yield return new WaitForSeconds(1.75f);
                    break;//サインイン成功!
                }

                // エラー
                Debug.LogError(error);

                // エラーの場合はリトライ
                subject.Dispose();
                using (var wait = UIWait.RentFromPool()) {
                    OkPopup.Open("ログインに失敗しました。", wait.Callback);
                    yield return wait;
                }
                subject = StatusLine.Observe();
            } while(true);
        } finally {
            subject.Dispose();
        }

        // TODO
        // サインイン結果
        Debug.Log("Player Name = " + GameDataManager.PlayerData.playerName);

        // UI の表示を開始
        uiAlphaEffect.Effect();

        // セットアップ完了！
        //GameAudio.MixBGM("Revenge2");
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