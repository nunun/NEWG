using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Networking;

// ゲームのカメラ処理
public partial class GameCamera : MonoBehaviour {
    //-------------------------------------------------------------------------- 変数
    public enum Mode { None, Menu, Battle, Result }

    //-------------------------------------------------------------------------- 変数
    Mode      mode      = Mode.None; // カメラモード
    TPSCamera tpsCamera = null;      // サードパーソンカメラ

    // ゲームカメラのインスタンス
    static GameCamera instance = null;

    // ゲームカメラのインスタンスの取得
    public static GameCamera Instance { get { return instance; }}

    //-------------------------------------------------------------------------- 実装 (MonoBehaviour)
    void Awake() {
        // インスタンス特定
        if (instance != null) {
            GameObject.Destroy(this.gameObject);
            return;
        }
        instance = this;

        // このオブジェクトから各種コンポーネントの取得
        tpsCamera = this.gameObject.GetComponent<TPSCamera>();
        Debug.Assert(tpsCamera != null, "フリールックカメラなし");

        // とりあえずメニューモード
        SetMenuMode();
    }

    void OnDestroy() {
        // インスタンス解除
        if (instance == this) {
            instance = null;
        }
    }

    void Update() {
        switch (mode) {
        case Mode.Menu:
            UpdateMenuMode();
            return;
        case Mode.Battle:
            UpdateBattleMode();
            return;
        case Mode.Result:
            UpdateResultMode();
            return;
        case Mode.None:
        default:
            return;
        }
    }
}

// メニューモードに移行
public partial class GameCamera {
    //-------------------------------------------------------------------------- 操作
    public void SetMenuMode() {
        mode = Mode.Menu;
        transform.position = new Vector3(0.0f, 1.0f, -10.0f);
        transform.rotation = Quaternion.identity;
        tpsCamera.enabled = false;
        GameInput.IsEnabled = false;
        Cursor.lockState = CursorLockMode.None;
    }

    void UpdateMenuMode() {
        // NOTE
        // 今のところ処理なし
    }
}

// バトルモードに移行
public partial class GameCamera {
    //-------------------------------------------------------------------------- 操作
    public void SetBattleMode(Player player) {
        mode = Mode.Battle;
        tpsCamera.target  = player.aimPivot;
        tpsCamera.enabled = true;
        GameInput.IsEnabled = true;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void UpdateBattleMode() {
        if (Cursor.lockState == CursorLockMode.Locked) {
            if (!GameInput.IsEnabled) {
                GameInput.IsEnabled = true;
            }
        } else {
            if (GameInput.IsEnabled) {
                GameInput.IsEnabled = false;
            }
            var refocusInput = Input.GetButton("Fire1");
            if (refocusInput) {
                Cursor.lockState = CursorLockMode.Locked;
            }
        }
    }
}

// 結果モードに移行
public partial class GameCamera {
    //-------------------------------------------------------------------------- 操作
    public void SetResultMode() {
        mode = Mode.Menu;
        tpsCamera.enabled = false;
        GameInput.IsEnabled = false;
        Cursor.lockState = CursorLockMode.None;
    }

    void UpdateResultMode() {
        // NOTE
        // 今のところ処理なし
    }
}
