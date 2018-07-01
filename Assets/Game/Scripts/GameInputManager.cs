using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

// ゲーム入力の実装
[DefaultExecutionOrder(int.MinValue)]
public partial class GameInputManager : MonoBehaviour {
    //-------------------------------------------------------------------------- 変数
    static float moveHorizontal = 0.0f;
    static float moveVertical   = 0.0f;
    static float lookHorizontal = 0.0f;
    static float lookVertical   = 0.0f;
    static bool  isFire         = false;
    static bool  isGangsta      = false;
    static bool  isThrow        = false;
    static bool  isSprint       = false;
    static bool  isJump         = false;
    static bool  isFocus        = false;

    public static float MoveHorizontal { get { return moveHorizontal; }}
    public static float MoveVertical   { get { return moveVertical;   }}
    public static float LookHorizontal { get { return lookHorizontal; }}
    public static float LookVertical   { get { return lookVertical;   }}
    public static bool  IsFire         { get { return isFire;         }}
    public static bool  IsGangsta      { get { return isGangsta;      }}
    public static bool  IsThrow        { get { return isThrow;        }}
    public static bool  IsSprint       { get { return isSprint;       }}
    public static bool  IsJump         { get { return isJump;         }}
    public static bool  IsFocus        { get { return isFocus;        }}

    // NOTE
    // 既読フラグ (FixedUpdate用)
    static bool markAsRead = false;

    //-------------------------------------------------------------------------- 制御
    public static void MarkAsRead() {
        markAsRead = true;
    }

    //-------------------------------------------------------------------------- 実装 (MonoBehaviour)
    void Start() {
        InitEnabled();
        InitMouseSensitivity();
        InitInvertMouse();
    }

    void Update() {
        if (isEnabled) {
            var isJumpDown = Input.GetKeyDown(KeyCode.Space);
            moveHorizontal = Input.GetAxis("Horizontal");
            moveVertical   = Input.GetAxis("Vertical");
            lookHorizontal = Input.GetAxis("Mouse X") * (mouseSensitivity / 100.0f);
            lookVertical   = Input.GetAxis("Mouse Y") * (mouseSensitivity / 100.0f) * ((invertMouse)? -1 : 1);
            isFire         = Input.GetButton("Fire1");
            isGangsta      = Input.GetKeyDown(KeyCode.E);
            isThrow        = Input.GetKeyDown(KeyCode.Q);
            isSprint       = Input.GetKey(KeyCode.LeftShift);
            isJump         = (markAsRead)? isJumpDown : (isJump | isJumpDown);
            isFocus        = false;
        } else {
            moveHorizontal = 0.0f;
            moveVertical   = 0.0f;
            lookHorizontal = 0.0f;
            lookVertical   = 0.0f;
            isFire         = false;
            isGangsta      = false;
            isThrow        = false;
            isSprint       = false;
            isJump         = false;
            isFocus        = Input.GetButton("Fire1");
        }
        markAsRead = false;
    }
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// 入力の有効化と無効化
public partial class GameInputManager {
    //-------------------------------------------------------------------------- 変数
    static bool isEnabled = false;

    public static bool IsEnabled { get { return isEnabled; } set { isEnabled = value; }}

    //-------------------------------------------------------------------------- 操作
    public static void SetEnabled(bool isEnabled) {
        GameInputManager.isEnabled = isEnabled;
    }

    //-------------------------------------------------------------------------- 初期化等
    void InitEnabled() {
        isEnabled = false;
    }
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// マウス感度関連
public partial class GameInputManager {
    //-------------------------------------------------------------------------- 定義
    public const string MOUSE_SENSITIVITY_KEY     = "Sensitivity";
    public const float  MAX_MOUSE_SENSITIVITY     = 100;
    public const float  DEFAULT_MOUSE_SENSITIVITY = MAX_MOUSE_SENSITIVITY / 2;

    //-------------------------------------------------------------------------- 変数
    static float mouseSensitivity = DEFAULT_MOUSE_SENSITIVITY;

    public static float MouseSensitivity { get { return mouseSensitivity; }}

    //-------------------------------------------------------------------------- 操作
    public static void SetMouseSensitivity(float mouseSensitivity) {
        GameInputManager.mouseSensitivity = mouseSensitivity;
        PlayerPrefs.SetFloat(MOUSE_SENSITIVITY_KEY, mouseSensitivity);
        PlayerPrefs.Save();
    }

    //-------------------------------------------------------------------------- 初期化等
    void InitMouseSensitivity() {
        mouseSensitivity = PlayerPrefs.GetFloat(MOUSE_SENSITIVITY_KEY, DEFAULT_MOUSE_SENSITIVITY);
    }
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// マウス反転
public partial class GameInputManager {
    //-------------------------------------------------------------------------- 定義
    public const string INVERT_MOUSE_KEY     = "InvertMouse";
    public const bool   DEFAULT_INVERT_MOUSE = false;

    //-------------------------------------------------------------------------- 変数
    static bool invertMouse = DEFAULT_INVERT_MOUSE;

    public static bool InvertMouse { get { return invertMouse; }}

    //-------------------------------------------------------------------------- 操作
    public static void SetInvertMouse(bool invertMouse) {
        GameInputManager.invertMouse = invertMouse;
        PlayerPrefs.SetInt(INVERT_MOUSE_KEY, ((invertMouse)? 1 :0));
        PlayerPrefs.Save();
    }

    //-------------------------------------------------------------------------- 初期化等
    void InitInvertMouse() {
        invertMouse = (PlayerPrefs.GetInt(INVERT_MOUSE_KEY, ((DEFAULT_INVERT_MOUSE)? 1 : 0)) != 0)? true : false;
    }
}
