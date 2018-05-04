using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// メッセージのみでボタンがないポップアップ
public class MessagePopup : UIComponent {
    //-------------------------------------------------------------------------- インスタンスの確保と解放
    [SerializeField] Text messageText = null;

    //-------------------------------------------------------------------------- インスタンスの確保と解放
    public static MessagePopup Open(string message, Action<string> callback = null) {
        var component = GameObjectTag<MessagePopup>.RentObject();
        component.messageText.text = message;
        component.SetUICallback(callback);
        component.Open();
        return component;
    }

    void ReturnToPool() {
        SetUIDone();
        GameObjectTag<MessagePopup>.ReturnObject(this);
    }

    //-------------------------------------------------------------------------- 実装 (MonoBehaviour)
    void Awake() {
        SetUIRecycle(ReturnToPool);
        SetUIVisibility(false);
    }

    void OnDestroy() {
        SetUIDone();
    }
}
