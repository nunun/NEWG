using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MessagePopup : UIComponent {
    //-------------------------------------------------------------------------- 実装 (MonoBehaviour)
    void Awake() {
        Hide();
    }

    void OnDestroy() {
        SetUIDone();
    }
}
