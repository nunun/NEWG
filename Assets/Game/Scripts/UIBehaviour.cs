using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// [UI 挙動の実装例]
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


// UI 挙動
// 全ての UI の基礎クラス
public class UIBehaviour : MonoBehaviour {
    //------------------------------------------------------------------------- 変数
    public UIEffect appearEffect    = null; // 出現エフェクト
    public UIEffect disappearEffect = null; // 消失エフェクト

    Action recycler = null; // 再利用関数の設定

    //------------------------------------------------------------------------- UI 結果関連
    // 再利用関数の設定
    protected void SetUIRecycle(Action recycler) {
        this.recycler = recycler;
    }

    //------------------------------------------------------------------------- 開く、閉じる、完了
    // 開く
    public void Open() {
        if (appearEffect != null) {
            appearEffect.Play(0.0f);
        }
    }

    // 閉じる
    public void Close() {
        if (disappearEffect != null) {
            disappearEffect.Play(0.0f);
        } else {
            Done();
        }
    }

    // 完了
    public void Done() {
        if (recycler != null) {
            recycler();
            return;
        }
        GameObject.Destroy(this.gameObject);// NOTE リサイクルしないなら削除
    }
}
