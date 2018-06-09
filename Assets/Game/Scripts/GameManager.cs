using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
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

// コマンドライン引数のインポート
public partial class GameManager {
    //-------------------------------------------------------------------------- コマンドライン引数
    public static bool ImportCommandLineStringArgument(string key, ref string v) {
        var s = default(string);
        if (ImportCommandLineArgument(key, ref s)) {
            v = s;
            return true;
        }
        return false;
    }

    public static bool ImportCommandLineIntegerArgument(string key, ref int v) {
        var s = default(string);
        if (ImportCommandLineArgument(key, ref s)) {
            v = int.Parse(s);
            return true;
        }
        return false;
    }

    public static bool ImportCommandLineFlagArgument(string key) {
        string[] args = System.Environment.GetCommandLineArgs();
        int index = System.Array.IndexOf(args, key);
        return (index >= 0);
    }

    public static bool ImportCommandLineArgument(string key, ref string v) {
        var args  = System.Environment.GetCommandLineArgs();
        var index = System.Array.IndexOf(args, key);
        if (index >= 0 && (index + 1) < args.Length) {
            v = args[index + 1];
            return true;
        }
        return false;
    }
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
#if UNITY_WEBGL

// ウェブブラウザ引数のインポート
public partial class GameManager {
    //-------------------------------------------------------------------------- ウェブブラウザクエリ引数
    public static void ImportWebBrowserQueryStringArgument(string key, ref string v) {
        var q = default(string);
        if (ImportWebBrowserQueryArgument(key, ref q)) {
            v = q;
            return true;
        }
        return false;
    }

    public static bool ImportWebBrowserQueryIntegerArgument(string key, ref int v) {
        var q = default(string);
        if (ImportWebBrowserQueryArgument(key, ref q)) {
            v = int.Parse(q);
            return true;
        }
        return false;
    }

    public static bool ImportWebBrowserQueryArgument(string key, ref string v) {
        var q = WebBrowser.GetLocationQuery(key);
        if (q != null) {
            v = q;
            return true;
        }
        return false;
    }

    //-------------------------------------------------------------------------- ウェブブラウザホスト名
    public static bool ImportWebBrowserHostName(ref v) {
        v = WebBrowser.GetLocationHostName();
        return true;
    }
}

#endif
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
