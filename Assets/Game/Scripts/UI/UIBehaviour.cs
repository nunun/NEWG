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
public partial class UIBehaviour : MonoBehaviour {
    //------------------------------------------------------------------------- 変数
    // 出現と消失エフェクト
    [SerializeField] protected UIEffect uiEffect = null;

    // 再利用関数の設定
    Action recycler = null;

    // 現在状態の取得
    public bool IsOpen    { get { return gameObject.activeSelf;                                          }}
    public bool IsOpening { get { return IsOpen && ((uiEffect == null)? false : uiEffect.IsEffecting);   }}
    public bool IsOpened  { get { return IsOpen && ((uiEffect == null)? true  : uiEffect.IsEffected);    }}
    public bool IsClosing { get { return IsOpen && ((uiEffect == null)? false : uiEffect.IsUneffecting); }}
    public bool IsClosed  { get { return !IsOpen;                                                        }}

    //------------------------------------------------------------------------- UI 結果関連
    // 再利用関数の設定
    protected void SetUIRecycle(Action recycler) {
        this.recycler = recycler;
    }

    //------------------------------------------------------------------------- 開く、閉じる、完了
    // 開く
    public void Open() {
        if (IsOpen) {
            return; // 既に開いている
        }
        gameObject.SetActive(true);
        if (uiEffect != null) {
            uiEffect.Effect();
        }
    }

    // 閉じる
    public void Close() {
        if (IsClosing) {
            return; // 既に閉じている
        }
        if (uiEffect != null) {
            uiEffect.Uneffect();
        } else {
            Done();
        }
    }

    // 隠す
    public void Hide() {
        if (uiEffect != null) {
            uiEffect.SetUneffected();
        }
        gameObject.SetActive(false);
    }

    // 完了
    public void Done() {
        gameObject.SetActive(false);
        if (recycler != null) {
            recycler();
            return;
        }
        GameObject.Destroy(this.gameObject);// NOTE リサイクルしないなら削除
    }
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// 完了 (Done()) で閉じないようにするユーティリティ
// 主に動作チェック (UIComponentTester) 等で使用。
public partial class UIBehaviour : MonoBehaviour {
    //------------------------------------------------------------------------- 変数
    Action recyclerBackup = null;

    //------------------------------------------------------------------------- ユーティリティ
    public void DontDestroyOnDone() {
        if (recycler == NothingToDo) {
            return;
        }
        recyclerBackup = recycler;
        recycler = NothingToDo;
    }

    public void DestroyOnDone() {
        if (recycler != NothingToDo) {
            return;
        }
        recycler = recyclerBackup;
    }

    //------------------------------------------------------------------------- 内部静的メソッド
    static void NothingToDo() {
        // NOTE
        // 何もしない
    }
}
