using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// タイトル
public class Title : GameScene {
    //-------------------------------------------------------------------------- 実装 (MonoBehaviour)
    IEnumerator Start() {
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
    }
}
