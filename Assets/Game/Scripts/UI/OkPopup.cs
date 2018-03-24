using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// "OK" ボタンがあるポップアップの実装
public class OkPopup : UIComponent {
    //-------------------------------------------------------------------------- インスタンスの確保と解放
    [SerializeField] Text   messageText  = null;
    [SerializeField] Button okButton     = null;
    [SerializeField] Text   okButtonText = null;

    //-------------------------------------------------------------------------- インスタンスの確保と解放
    public static OkPopup Open(string message, Action<string> callback = null, string ok = null) {
        var component = GameObjectTag<OkPopup>.RentObject();
        component.messageText.text = message;
        if (ok != null) {
            component.okButtonText.text = ok;
        }
        component.SetUICallback(callback);
        component.Open();
        return component;
    }

    void ReturnToPool() {
        SetUIDone();
        GameObjectTag<OkPopup>.ReturnObject(this);
    }

    //-------------------------------------------------------------------------- イベント
    protected void OnClickOk() {
        GameAudio.Play("OK");
        Close();
    }

    //-------------------------------------------------------------------------- 実装 (MonoBehaviour)
    void Awake() {
        SetUIRecycle(ReturnToPool);
        okButton.onClick.AddListener(OnClickOk);
        Hide();
    }

    void OnDestroy() {
        SetUIDone();
    }
}
