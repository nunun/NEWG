using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestPopup : UIComponent {
    //-------------------------------------------------------------------------- 実装 (MonoBehaviour)
    void Awake() {
        Hide();
    }

    void OnDestroy() {
        SetUIDone();
    }
}
