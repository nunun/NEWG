using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Services.Protocols;

// TODO
// * マッチング処理
// * 排他的な UI の制御
// * 急激に UI を切り替えた場合の切り替えエラーの修正

// ロビー
public partial class Lobby : GameScene {
    //-------------------------------------------------------------------------- 実装 (MonoBehaviour)
    void Awake() {
        InitLobby();
        InitMatching();
    }

    void OnDestroy() {
        StopLobby();
        StopMatching();
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

        // ロビー状態を開始
        StartLobby();

        // セットアップ完了！
        //GameAudio.MixBGM("Revenge2");
    }
}

///////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////

// ロビー処理
public partial class Lobby {
    //-------------------------------------------------------------------------- 変数
    [SerializeField] CanvasGroupAlphaEffect lobbyUI         = null;
    [SerializeField] Button                 gameStartButton = null;

    IEnumerator updateLobby = null;

    //-------------------------------------------------------------------------- ロビーの初期化、開始、停止、更新
    void InitLobby() {
        Debug.Assert(lobbyUI         != null, "lobbyUI がない");
        Debug.Assert(gameStartButton != null, "gameStartButton がない");
        gameStartButton.onClick.AddListener(() => {
            StopLobby();
            StartMatching();
        });
    }

    void StartLobby() {
        lobbyUI.Effect();
        GameAudio.SetBGMVolume("Abandoned", 1.0f);
        updateLobby = updateLobby ?? UpdateLobby();
        StartCoroutine(updateLobby);
    }

    void StopLobby() {
        lobbyUI.Uneffect();
        StopCoroutine(updateLobby);
    }

    IEnumerator UpdateLobby() {
        // NOTE
        // 特に処理なし
        yield break;
    }
}

///////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////

// マッチング処理
public partial class Lobby {
    //-------------------------------------------------------------------------- 変数
    [SerializeField] CanvasGroupAlphaEffect matchingUI   = null;
    [SerializeField] Button                 cancelButton = null;

    IEnumerator updateMatching = null;

    //-------------------------------------------------------------------------- マッチングの初期化、開始、停止、更新
    void InitMatching() {
        Debug.Assert(matchingUI   != null, "matchingUI がない");
        Debug.Assert(cancelButton != null, "cancelButton がない");
        cancelButton.onClick.AddListener(() => {
            StartLobby();
            StopMatching();
        });
    }

    void StartMatching() {
        matchingUI.Effect();
        GameAudio.SetBGMVolume("Abandoned", 0.3f, 5.0f);
        updateMatching = updateMatching ?? UpdateMatching();
        StartCoroutine(updateMatching);
    }

    void StopMatching() {
        matchingUI.Uneffect();
        StopCoroutine(updateMatching);
    }

    IEnumerator UpdateMatching() {
        // TODO
        // マッチング処理
        yield break;
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
