using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// シーンで使われる UI の実装
// UI の内部実装はありません。
// シーンの実装が、この UI に対して Open() と Close() するために使います。
public class SceneUI : UIComponent {
    //-------------------------------------------------------------------------- 定義
    public enum Visibility { Show, Hide, SceneSetting };

    //-------------------------------------------------------------------------- 変数
    public Visibility initialVisibility = Visibility.Hide;

    //-------------------------------------------------------------------------- 実装 (MonoBehaviour)
    void Awake() {
        SetUIRecycle(SetUIDone);
        switch (initialVisibility) {
        case Visibility.Show:
            Show();
            break;
        case Visibility.Hide:
            Hide();
            break;
        case Visibility.SceneSetting:
        default:
            break;
        }
    }

    void OnDestroy() {
        SetUIDone();
    }
}
