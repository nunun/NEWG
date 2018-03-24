using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// タイトル
public class Title : GameScene {
    //-------------------------------------------------------------------------- 実装 (MonoBehaviour)
    IEnumerator Start() {
        GameAudio.PlayBGM("Revenge1");
        GameAudio.SetBGMVolume("Revenge1", 1.0f);

        using (var subject = StatusLine.Observe()) {
            subject.message = "ログイン中 ...";
            yield return new WaitForSeconds(1.0f);
            subject.message = "ロード中 ...";
            yield return new WaitForSeconds(1.0f);
        }

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
