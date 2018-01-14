using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

// ゲーム入力の実装
public partial class GameInput : MonoBehaviour {
    //-------------------------------------------------------------------------- 定数
    public const string SENSITIVITY_KEY    = "Sensitivity";
    public const int    MAX_SENSITIVIY     = 100;
    public const int    DEFAULT_SENSITIVIY = MAX_SENSITIVIY / 2;

    //-------------------------------------------------------------------------- 変数
    static bool  isEnabled      = false;
    static float moveHorizontal = 0.0f;
    static float moveVertical   = 0.0f;
    static float lookHorizontal = 0.0f;
    static float lookVertical   = 0.0f;
    static bool  isFire         = false;
    static bool  isThrow        = false;
    static bool  isJump         = false;
    static int   sensitivity    = DEFAULT_SENSITIVIY;

    public static bool  IsEnabled      { get { return isEnabled;      } set { isEnabled = value; }}
    public static float MoveHorizontal { get { return moveHorizontal; }}
    public static float MoveVertical   { get { return moveVertical;   }}
    public static float LookHorizontal { get { return lookHorizontal; }}
    public static float LookVertical   { get { return lookVertical;   }}
    public static bool  IsFire         { get { return isFire;         }}
    public static bool  IsThrow        { get { return isThrow;        }}
    public static bool  IsJump         { get { return isJump;         }}
    public static int   Sensitivity    { get { return sensitivity;    }}

    //-------------------------------------------------------------------------- 実装 (MonoBehaviour)
    void Start() {
        // マウス感度復活
        sensitivity = PlayerPrefs.GetInt(SENSITIVITY_KEY, DEFAULT_SENSITIVIY);
        GameMain.Instance.battleUI.SetSensitivity(sensitivity);
    }

    void Update() {
        // 入力の設定
        if (isEnabled) {
            moveHorizontal = Input.GetAxis("Horizontal");
            moveVertical   = Input.GetAxis("Vertical");
            lookHorizontal = Input.GetAxis("Mouse X") * (sensitivity / 100.0f);
            lookVertical   = Input.GetAxis("Mouse Y") * (sensitivity / 100.0f);
            isFire         = Input.GetButton("Fire1");
            isThrow        = Input.GetKeyDown(KeyCode.Q);
            isJump         = Input.GetKeyDown(KeyCode.Space);

            // マウス感度の設定
            if (Input.GetKeyDown(KeyCode.LeftBracket)) {
                sensitivity = Mathf.Min(sensitivity + 1, 100);
                GameMain.Instance.battleUI.SetSensitivity(sensitivity);
                PlayerPrefs.SetInt(SENSITIVITY_KEY, sensitivity);
            }
            if (Input.GetKeyDown(KeyCode.RightBracket)) {
                sensitivity = Mathf.Max(sensitivity - 1, 0);
                GameMain.Instance.battleUI.SetSensitivity(sensitivity);
                PlayerPrefs.SetInt(SENSITIVITY_KEY, sensitivity);
            }
        } else {
            moveHorizontal = 0.0f;
            moveVertical   = 0.0f;
            lookHorizontal = 0.0f;
            lookVertical   = 0.0f;
            isFire         = false;
            isThrow        = false;
            isJump         = false;
        }
    }
}
