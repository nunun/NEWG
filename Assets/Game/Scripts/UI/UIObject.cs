using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// UI オブジェクト
// 全ての UI の基礎クラス
public partial class UIObject : MonoBehaviour {
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
        InvokeOpenCallback();
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
        InvokeCloseCallback();
    }

    // 完了
    public void Done() {
        gameObject.SetActive(false);
        InvokeCloseCallback();
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
public partial class UIObject : MonoBehaviour {
    //------------------------------------------------------------------------- 変数
    Action recyclerBackup = null;

    //------------------------------------------------------------------------- ユーティリティ
    // Done() の呼び出しで閉じないように設定
    public void DontDestroyOnDone() {
        if (recycler == NothingToDo) {
            return;
        }
        recyclerBackup = recycler;
        recycler = NothingToDo;
    }

    // Done() の呼び出しで閉じるように設定
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

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// UI のコルーチン関連の実装
// UI が開いたときに Coroutine を実行できるようにします。
public partial class UIObject {
    //------------------------------------------------------------------------- 変数
    IEnumerator openCoroutine = null;
    Action      openCallback  = null;
    Action      closeCallback = null;

    //------------------------------------------------------------------------- 設定関数
    void InvokeOpenCallback() {
        if (openCallback != null) {
            StartCoroutine(openCoroutine);
        }
        if (openCallback != null) {
            openCallback();
        }
    }

    void InvokeCloseCallback() {
        if (openCallback != null) {
            StopCoroutine(openCoroutine);
        }
        if (closeCallback != null) {
            closeCallback();
        }
    }

    //------------------------------------------------------------------------- 設定関数
    // "開く" コールバックの設定
    protected void SetUIOpenCallback(IEnumerator coroutine) {
        Debug.Assert(openCallback != null, "開くコールバック設定済");
        openCallback  = null;
        openCoroutine = coroutine;
    }

    // "開く" コルーチンの設定
    protected void SetUIOpenCallback(Action callback) {
        Debug.Assert(openCoroutine != null, "開くコルーチン設定済");
        openCallback  = callback;
        openCoroutine = null;
    }

    // "閉じる" コルーチンの設定
    protected void SetUICloseCallback(Action callback) {
        closeCallback = callback;
    }
}
