using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// [使い方]
// using (var subject = StatusLine.Observe()) {
//     subject.message = "ログイン中 ...";
//     yield return new WaitForSeconds(1.0f);
//     subject.message = "ロード中 ...";
//     yield return new WaitForSeconds(1.0f);
// }

// 現在の処理状況を表示するラインUI
public class StatusLine : UIComponent<bool> {
    //-------------------------------------------------------------------------- インスタンスの確保と解放
    [SerializeField] Text messageText  = null; // 状態メッセージ
    [SerializeField] Text progressText = null; // 進捗テキスト

    // ステータスオブザーバ
    Observer observer = new Observer();

    //-------------------------------------------------------------------------- インスタンスの確保と解放
    public static Observer.Subject Observe(string name = null) {
        var component = GameObjectTag<StatusLine>.Find();
        if (!component.IsOpen) {
            component.messageText.text  = null;
            component.progressText.text = null;
            component.observer.Clear();
            component.Open();
        }
        return component.observer.Observe(name);
    }

   //-------------------------------------------------------------------------- 実装 (MonoBehaviour)
    void Awake() {
        SetUIRecycle(() => {});
        SetUIVisibility(false);
    }

    void OnDestroy() {
        SetUIDone();
    }

    void Update() {
        // 閉じ中は更新しない
        if (IsClosing) {
            return;
        }

        // 更新
        if (observer.isDirtyMessage) {
            messageText.text = observer.message;
        }
        if (observer.isDirtyProgress) {
            progressText.text = observer.progress.ToString();
        }

        // 撒いたサブジェクトを監視
        observer.Update();
        if (observer.isDone) {
            Close();
        }
    }
}
