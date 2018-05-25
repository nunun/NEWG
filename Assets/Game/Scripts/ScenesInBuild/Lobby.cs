using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Services.Protocols;

// ロビー
public partial class Lobby : GameScene {
    //-------------------------------------------------------------------------- 実装 (MonoBehaviour)
    void Awake() {
        InitSignin();
        InitLobby();
        InitMatching();
    }

    void Start() {
        signinUI.Open();
    }
}

///////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////

// サインイン処理
public partial class Lobby {
    //-------------------------------------------------------------------------- 変数
    [SerializeField] SceneUI signinUI = null;

    //-------------------------------------------------------------------------- ロビーの初期化、開始、停止、更新
    void InitSignin() {
        Debug.Assert(signinUI        != null, "signinUI がない");
        Debug.Assert(gameStartButton != null, "gameStartButton がない");
        signinUI.onOpen.AddListener(() => { StartCoroutine("UpdateSignin"); });
        signinUI.onClose.AddListener(() => { StopCoroutine("UpdateSignin"); });
    }

    IEnumerator UpdateSignin() {
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

        // ロビーへ
        signinUI.Change(lobbyUI);
    }
}

///////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////

// ロビー処理
public partial class Lobby {
    //-------------------------------------------------------------------------- 変数
    [SerializeField] SceneUI lobbyUI         = null;
    [SerializeField] Button  gameStartButton = null;

    //-------------------------------------------------------------------------- ロビーの初期化、開始、停止、更新
    void InitLobby() {
        Debug.Assert(lobbyUI         != null, "lobbyUI がない");
        Debug.Assert(gameStartButton != null, "gameStartButton がない");
        lobbyUI.onOpen.AddListener(() => { StartCoroutine("UpdateLobby"); });
        lobbyUI.onClose.AddListener(() => { StopCoroutine("UpdateLobby"); });
        gameStartButton.onClick.AddListener(() => { lobbyUI.Change(matchingUI); });
    }

    IEnumerator UpdateLobby() {
        GameAudio.SetBGMVolume("Abandoned", 1.0f);
        yield break;
    }
}

///////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////

// マッチング処理
public partial class Lobby {
    //-------------------------------------------------------------------------- 変数
    [SerializeField] SceneUI matchingUI   = null;
    [SerializeField] Button  cancelButton = null;

    //-------------------------------------------------------------------------- マッチングの初期化、開始、停止、更新
    void InitMatching() {
        Debug.Assert(matchingUI   != null, "matchingUI がない");
        Debug.Assert(cancelButton != null, "cancelButton がない");
        matchingUI.onOpen.AddListener(() => { StartCoroutine("UpdateMatching"); });
        matchingUI.onClose.AddListener(() => { StopCoroutine("UpdateMatching"); });
        cancelButton.onClick.AddListener(() => { matchingUI.Change(lobbyUI); });
    }

    IEnumerator UpdateMatching() {
        GameAudio.SetBGMVolume("Abandoned", 0.3f, 5.0f);

        // 接続情報を取得
        var error            = default(string);
        var matchingResponse = default(WebAPI.MatchingResponse);
        using (var wait = UIWait<WebAPI.MatchingResponse>.RentFromPool()) {
            WebAPI.Matching(wait.Callback);
            yield return wait;
            error            = wait.error;
            matchingResponse = wait.Value1;
        }

        #if STANDALONE_MODE
        // スタンドアローンモード時
        if (GameManager.IsStandaloneMode) {
            GameSceneManager.ChangeScene(GameManager.ServerSceneName);
            yield break;
        }
        #endif

        // TODO
        // マッチングサーバに接続
        Debug.Log(error);
        Debug.Log(matchingResponse.matchingServerUrl);

        // TODO
        // ウェブソケットに接続する。
        // 切断前にイベント 0 が送信されるので、
        // 0 を受け取ったら処理して次のシーンへ。
        // 0 のメッセージがエラーか、0 を受け取る前に切断されたら前のシーンへ。
        // 0 で受け取れるのは MatchConnectData である。
        // MatchConnectData に含まれる serverToken は redis で作成する。
        // matchId to MatchConnectData (マッチ毎),
        // userId to matchId (ユーザ毎),
        // serverToken to userId (ユーザ毎) を作っておく。
        // サーバはマッチが終わったら matchId to MatchConnectData を消す。
        // ユーザは再参加時、userId から matchId を引いて、matchId から MatchConnectData を取得する。
        // この時 matchId が存在しないのであれば、再参加は行われない。

        // TODO
        //GameManager.SetServerInformation(address, port, token, sceneName);
        //matchingUI.ChangeScene(GameManager.ServerSceneName);

        // TODO
        // マッチングサーバへの接続とシーン切り替え
        yield return new WaitForSeconds(1.0f);
        matchingUI.ChangeScene("Logo");
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
