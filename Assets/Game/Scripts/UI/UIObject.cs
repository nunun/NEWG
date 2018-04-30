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
        SetActive();
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
        SetInactive();
    }

    // 完了
    public void Done() {
        SetInactive();
        if (recycler != null) {
            recycler();
            return;
        }
        GameObject.Destroy(this.gameObject);// NOTE リサイクルしないなら削除
    }

    //------------------------------------------------------------------------- 内部処理
    // ゲームオブジェクトをアクティブに設定
    void SetActive() {
        gameObject.SetActive(true);
        InvokeOpenCallback();
    }

    // ゲームオブジェクトを非アクティブに設定
    void SetInactive() {
        InvokeCloseCallback();
        gameObject.SetActive(false);
        //DoneWaitForClose(); // NOTE WaitForClose は使わないかもしれないので
    }
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// 完了 (Done()) で閉じないようにするユーティリティ
// 主に動作チェック (UITester) 等で使用。
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

// UI 処理でコルーチンを使えるようにする実装
// UI 処理を Start() を使って書かずに
// 任意のコールバックおよびコルーチンで実装できるようにします。
public partial class UIObject {
    //------------------------------------------------------------------------- 変数
    IEnumerator openCoroutine = null; // "開く" コールバック ("開く" コルーチンと排他)
    Action      openCallback  = null; // "開く" コルーチン ("開く" コールバックと排他)
    Action      closeCallback = null; // "閉じる" コールバック

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

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// NOTE
// WaitForClose は使わないかもしれないので。
//// UI 閉じ待ち実装
//// "UI が閉じるまで待つ" を yield return で出来るようにします。
//// UI のレスポンスを取ることはできません。
//public partial class UIObject {
//    //------------------------------------------------------------------------- 定義
//    // UI 閉じ待ちオペレーション
//    public class WaitForCloseOperation : CustomYieldInstruction {
//        public bool isDone = false;
//        public override bool keepWaiting { get { return !isDone; }}
//    }
//
//    //------------------------------------------------------------------------- 変数
//    WaitForCloseOperation waitForCloseOperation = null;
//
//    //------------------------------------------------------------------------- 操作
//    // 閉じるまで待つ
//    public WaitForCloseOperation WaitForClose() {
//        waitForCloseOperation = waitForCloseOperation ?? new WaitForCloseOperation();
//        return waitForCloseOperation;
//    }
//
//    //------------------------------------------------------------------------- 操作
//    // 閉じるまで待つ、を完了
//    void DoneWaitForClose() {
//        if (waitForCloseOperation != null) {
//            var operation = waitForCloseOperation;
//            waitForCloseOperation = null;
//            operation.isDone = true;
//        }
//    }
//}
