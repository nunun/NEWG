﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 設定オプションスクリーン
public class OptionsScreen : UIComponent {
    //-------------------------------------------------------------------------- 変数
    [SerializeField] Slider      mouseSensitivitySlider = null;
    [SerializeField] TextBuilder mouseSensitivityText   = null;
    [SerializeField] Toggle      invertMouseToggle      = null;
    [SerializeField] Button      closeButton            = null;

    //-------------------------------------------------------------------------- インスタンスの確保と解放
    public static OptionsScreen OpenScreen() {
        var component = GameObjectTag<OptionsScreen>.Find("OptionsScreen");
        Debug.Assert(component != null, "OptionsScreen がない");
        component.Open();
        return component;
    }

    //-------------------------------------------------------------------------- イベント
    protected void OnOpen() {
        OnMouseSensitivityChange(GameInputManager.MouseSensitivity);
        OnInvertMouseChange(GameInputManager.InvertMouse);
    }

    protected void OnMouseSensitivityChange(float value) {
        var sensitivity = Mathf.Floor(value * 10.0f) / 10.0f;
        mouseSensitivitySlider.value = value;
        mouseSensitivityText.Begin(sensitivity).Apply();
        GameInputManager.SetMouseSensitivity(sensitivity);
    }

    protected void OnInvertMouseChange(bool value) {
        invertMouseToggle.isOn = value;
        GameInputManager.SetInvertMouse(value);
    }

    protected void OnClickClose() {
        GameAudio.Play("Cancel");
        Close();
    }

    //-------------------------------------------------------------------------- 実装 (MonoBehaviour)
    void Awake() {
        mouseSensitivitySlider.onValueChanged.AddListener(OnMouseSensitivityChange);
        invertMouseToggle.onValueChanged.AddListener(OnInvertMouseChange);
        closeButton.onClick.AddListener(OnClickClose);
        SetUIVisibility(false);
    }

    void OnEnable() {
        Invoke("OnOpen", 0.01f);
    }
}