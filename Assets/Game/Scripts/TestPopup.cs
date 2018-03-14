using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestPopup : UIComponent {
    //-------------------------------------------------------------------------- 実装 (MonoBehaviour)
    void Awake() {
        disappearEffect.Stop(1.0f);
    }

    void Start() {
        Invoke("OpenPopup",  3.0f);
        Invoke("ClosePopup", 6.0f);
    }

    void OnDestroy() {
        SetUIDone();
    }

    //-------------------------------------------------------------------------- イベント等
    void OpenPopup() {
        Open();
    }

    void ClosePopup() {
        Close();
    }
}
