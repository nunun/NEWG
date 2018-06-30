using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// シーンで使われる UI の実装
public class ConfigScreen : UIComponent {
    //-------------------------------------------------------------------------- 変数
    [SerializeField] Slider      mouseSensitivitySlider = null;
    [SerializeField] TextBuilder mouseSensitivityText   = null;
    [SerializeField] Toggle      invertMouseToggle      = null;
    [SerializeField] Button      closeButton            = null;

    //-------------------------------------------------------------------------- インスタンスの確保と解放
    public static ConfigScreen OpenScreen() {
        var component = GameObjectTag<ConfigScreen>.Find("ConfigScreen");
        Debug.Assert(component != null, "ConfigScreen がない");
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
