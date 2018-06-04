using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using System.IO;
using System.Text;
using UnityEditor;
#endif

// ゲームマネージャ
// ゲーム自体の制御と設定保持を行います。
[DefaultExecutionOrder(int.MinValue)]
public partial class GameManager : MonoBehaviour {
    // NOTE
    // パーシャルクラスを参照
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// スタンドアローンモード情報
public partial class GameManager {
    //-------------------------------------------------------------------------- 変数
    public StandaloneSimulatorProfile standaloneSimulatorProfile = null; // スタンドアローンシミュレータ プロファイル

    // スタンドアローンシミュレータの取得
    public static StandaloneSimulatorProfile StandaloneSimulatorProfile { get { return instance.standaloneSimulatorProfile; }}

    // スタンドアローンモードかどうか
    // このフラグの if 文を置くことによって Unreachable コードの警告を回避
    public static bool IsStandaloneMode {
        get {
            #if STANDALONE_MODE
            return true;
            #else
            return false;
            #endif
        }
    }
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// ゲームの中断と終了
public partial class GameManager {
    //-------------------------------------------------------------------------- 変数
    static string abortSceneName = "Abort";
    static string lastAbortError = null;

    //-------------------------------------------------------------------------- 中断
    public static void Abort(string error = null) {
        error = error ?? "Game Aborted. Fatal Error occurred.";
        Debug.LogErrorFormat("GameManager.Abort: error='{0}'", error);
        SetLastAbortErorr(error);
        if (!GameSceneManager.ChangeSceneImmediately(abortSceneName)) {
            Quit();
            return;
        }
    }

    static void SetAbortSceneName(string sceneName) {
        abortSceneName = sceneName;
    }

    static void SetLastAbortErorr(string error) {
        lastAbortError = error;
    }

    public static string GetLastAbortError() {
        return lastAbortError;
    }

    public static void ClearLastAbortError() {
        lastAbortError = null;
    }

    //-------------------------------------------------------------------------- 終了
    public static void Quit() {
        Application.Quit();
        #if UNITY_EDITOR
        EditorApplication.isPlaying = false;
        #endif
    }
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// MonoBehaviour 実装
public partial class GameManager {
    //-------------------------------------------------------------------------- 変数
    // ゲームサービスインスタンス
    static GameManager instance = null;

    //-------------------------------------------------------------------------- 実装 (MonoBehaviour)
    void Awake() {
        if (instance != null) {
            GameObject.Destroy(this.gameObject);
            return;
        }
        instance = this;
    }

    void OnDestroy() {
        if (instance != this) {
            return;
        }
        instance = null;
    }
}
