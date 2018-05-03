using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// UI オブジェクト
// 全ての UI の基礎クラス
public partial class UIObject : MonoBehaviour {
    //------------------------------------------------------------------------- 変数
    // イベント
    [Serializable]
    public struct UIObjectEvents {
        public UnityEvent onOpen;  // 表示を開始したとき
        public UnityEvent onClose; // 表示を完了したとき
    }

    //------------------------------------------------------------------------- 変数
    [SerializeField] protected UIObjectEvents events;          // イベント
    [SerializeField] protected UIEffect       uiEffect = null; // 出現と消失エフェクト

    // "開く" 中に実行される Hide() を無効化する。ワークフラグ。
    bool disableHideOnOpen = false;

    // 次の UI、または次のシーンへ
    UIObject nextUIObject        = null; // 次の UI
    string   nextSceneName       = null; // 次のシーン名
    string   nextSceneEffectName = null; // 次のシーン切り替えエフェクト名

    // 再利用関数の設定
    Action recycler = null;

    // 現在状態の取得
    public bool IsOpen    { get { return gameObject.activeSelf;                                          }}
    public bool IsOpening { get { return IsOpen && ((uiEffect == null)? false : uiEffect.IsEffecting);   }}
    public bool IsOpened  { get { return IsOpen && ((uiEffect == null)? true  : uiEffect.IsEffected);    }}
    public bool IsClosing { get { return IsOpen && ((uiEffect == null)? false : uiEffect.IsUneffecting); }}
    public bool IsClosed  { get { return !IsOpen;                                                        }}

    // イベントの取得
    public UnityEvent onOpen  { get { return events.onOpen  ?? (events.onOpen  = new UnityEvent()); }}
    public UnityEvent onClose { get { return events.onClose ?? (events.onClose = new UnityEvent()); }}

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
        disableHideOnOpen = true; // NOTE Hide 無効化, Awake などで UI が Hide されるのを無視
        SetActive();
        disableHideOnOpen = false; // NOTE Hide 無効化解除
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

    // UI を変更
    public void Change(UIObject nextUIObject) {
        this.nextUIObject = nextUIObject;
        Close();
    }

    // シーンを変更
    public void ChangeScene(string nextSceneName, string nextSceneEffectName = null) {
        this.nextSceneName       = nextSceneName;
        this.nextSceneEffectName = nextSceneEffectName;
        Close();
    }

    // 隠す
    public void Hide() {
        if (disableHideOnOpen) { // NOTE Hide 無効化, Awake などで UI が Hide されるのを無視
            return;
        }
        if (uiEffect != null) {
            uiEffect.SetUneffected();
        }
        SetHide();
    }

    // 完了
    public void Done() {
        SetInactive();
        var uiObject        = nextUIObject;
        var sceneName       = nextSceneName;
        var sceneEffectName = nextSceneEffectName;
        nextUIObject        = null;
        nextSceneName       = null;
        nextSceneEffectName = null;
        if (sceneName != null) {
            GameSceneManager.ChangeScene(sceneName, sceneEffectName);
        } else if (uiObject != null) {
            uiObject.Open();
        }
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
        events.onOpen.Invoke();
    }

    // ゲームオブジェクトを非アクティブに設定
    void SetInactive() {
        events.onClose.Invoke();
        //DoneWaitForClose(); // NOTE WaitForClose は使わないかもしれないので
        SetHide();
    }

    void SetHide() {
        gameObject.SetActive(false);
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
