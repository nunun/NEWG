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
    Action recycler = null; // 再利用関数の設定

    //------------------------------------------------------------------------- UI 結果関連
    // 再利用関数の設定
    protected void SetUIRecycle(Action recycler) {
        this.recycler = recycler;
    }

    //------------------------------------------------------------------------- 開く、閉じる
    // 開く
    public void Open() {
        // TODO
        // Appear
    }

    // 閉じる
    public void Close() {
        // TODO
        // Disappear
    }

    // 閉じられた
    public void Closed() {
        if (recycler != null) {
            recycler();
            return;
        }
        GameObject.Destroy(this.gameObject);// NOTE リサイクルしないなら削除
    }
}
