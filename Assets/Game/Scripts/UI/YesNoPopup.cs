using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// "Yes", "No" ボタンがあるポップアップの実装
public class YesNoPopup : UIComponent<bool> {
    //-------------------------------------------------------------------------- インスタンスの確保と解放
    [SerializeField] Text   messageText   = null;
    [SerializeField] Button yesButton     = null;
    [SerializeField] Text   yesButtonText = null;
    [SerializeField] Button noButton      = null;
    [SerializeField] Text   noButtonText  = null;

    //-------------------------------------------------------------------------- インスタンスの確保と解放
    public static YesNoPopup Open(string message, Action<string,bool> callback = null, string yes = null, string no = null) {
        var component = GameObjectTag<YesNoPopup>.RentObject();
        component.messageText.text = message;
        if (yes != null) {
            component.yesButtonText.text = yes;
        }
        if (no != null) {
            component.noButtonText.text = no;
        }
        component.SetUICallback(callback);
        component.Open();
        return component;
    }

    void ReturnToPool() {
        SetUIDone();
        GameObjectTag<YesNoPopup>.ReturnObject(this);
    }

    //-------------------------------------------------------------------------- イベント
    protected void OnClickYes() {
        GameAudio.Play("OK");
        SetUIResult(null, true);
        Close();
    }

    protected void OnClickNo() {
        GameAudio.Play("Cancel");
        SetUIResult(null, false);
        Close();
    }

    //-------------------------------------------------------------------------- 実装 (MonoBehaviour)
    void Awake() {
        SetUIRecycle(ReturnToPool);
        yesButton.onClick.AddListener(OnClickYes);
        noButton.onClick.AddListener(OnClickNo);
        Hide();
    }

    void OnDestroy() {
        SetUIDone();
    }
}
