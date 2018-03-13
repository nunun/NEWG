using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO
// [UI エフェクトの実装例]
//public class SampleUI : UIComponent<string> {
//    public static SampleUI RentFromPool(Action<string> callback) {
//        var component = GameObjectPool<SampleUI>.RentGameObject();
//        component.SetUICallback(callback);
//        component.Open();
//    }
//    public void ReturnToPool() {
//        GameObjectPool<SampleUI>.ReturnObject(this);
//    }
//    void Awake() {
//        SetUIRecycle(ReturnToPool);
//    }
//}
//var ui = SampleUI.RentFromPool(callback);
//ui.Close();
//GameObject.Destroy(ui.gameObject);


// UI エフェクト
// 全ての UI エフェクトの基礎クラス
public class UIEffect : MonoBehaviour {
    //-------------------------------------------------------------------------- 変数
    // TODO

    //-------------------------------------------------------------------------- エフェクト終了
    // エフェクト開始
    public void Effect() {
        // TODO
    }

    // エフェクト終了
    public void Effected() {
        // TODO
    }

    // アンエフェクト開始
    public void Uneffect() {
        // TODO
    }

    // アンエフェクト終了
    public void Uneffected() {
        var uiComponent = gameObject.GetComponent<UIComponent>();
        if (uiComponent != null) {
            uiComponent.Closed();
        }
    }
}
