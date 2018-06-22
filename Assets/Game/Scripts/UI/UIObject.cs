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

    // 表示状態が初期化済かどうか
    // Show, Hide 後の Awake で二重初期化を防止するためのワークフラグ。
    bool isUIVisibilityInitialized = false;

    // アクティブに設定されたかどうか
    // イベント呼び出し制御用のワークフラグ。
    bool isSetActive = false;

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

    // 表示の初期状態を設定
    protected void SetUIVisibility(bool isShow) {
        if (isUIVisibilityInitialized) {
            return;
        }
        if (isShow) {
            this.Show();
        } else {
            this.SetHide();
        }
    }

    //------------------------------------------------------------------------- 開く、閉じる、完了
    // 開く
    public void Open() {
        if (IsOpening || IsOpened) {
            return; // 既に開いている
        }
        if (IsClosing) {
            Debug.LogError("閉じ中に開いた");
            Hide();
        }
        SetActive(true);
        if (uiEffect != null) {
            uiEffect.Effect();
        }
    }

    // 閉じる
    public void Close() {
        if (IsClosing || IsClosed) {
            return; // 既に閉じている
        }
        if (IsOpening) {
            Debug.LogError("閉き中に閉じた");
            Show();
        }
        if (uiEffect != null) {
            if (IsOpening) {
                uiEffect.Effected();
            }
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

    // 表示
    public void Show() {
        SetActive(true);
        if (uiEffect != null) {
            uiEffect.SetEffected();
        }
    }

    // 非表示
    public void Hide() {
        if (uiEffect != null) {
            uiEffect.SetUneffected();
        }
        SetInactive(true);
    }

    // 非表示に設定 (イベントコールなし)
    protected void SetHide() {
        if (uiEffect != null) {
            uiEffect.SetUneffected();
        }
        SetInactive(false);
    }

    // 完了
    public void Done() {
        SetInactive(true);
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
    // ゲームオブジェクトを表示に設定
    void SetActive(bool invokeEvent) {
        isUIVisibilityInitialized = true;
        gameObject.SetActive(true);
        if (!isSetActive) {
            isSetActive = true;
            if (invokeEvent) {
                events.onOpen.Invoke();
            }
        }
    }

    // ゲームオブジェクトを表示に設定
    void SetInactive(bool invokeEvent) {
        isUIVisibilityInitialized = true;
        if (isSetActive) {
            isSetActive = false;
            if (invokeEvent) {
                events.onClose.Invoke();
            }
        }
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

//DoneWaitForClose(); // NOTE WaitForClose は使わないかもしれないので
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
