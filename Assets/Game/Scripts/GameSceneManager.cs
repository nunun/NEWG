using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// ゲームシーンマネージャ
[DefaultExecutionOrder(int.MinValue)]
public class GameSceneManager : MonoBehaviour {
    //-------------------------------------------------------------------------- 定義
    public static readonly string DEFAULT_UI_EFFECT_NAME = "ScreenFadeEffect";

    //-------------------------------------------------------------------------- 変数
    // 現在走っているコルーチン
    static Coroutine currentCoroutine = null;

    // 現在のシーン
    static GameScene currentScene = null;

    // 現在のシーンの取得
    public static GameScene CurrentScene { get { return currentScene; }}

    // インスタンス
    static GameSceneManager instance = null;

    //-------------------------------------------------------------------------- 切り替え
    // シーン変更
    public static void ChangeScene(string sceneName, string uiEffectName = null) {
        Debug.Assert(currentCoroutine == null);
        currentCoroutine = instance.StartCoroutine(instance.DoChangeScene(sceneName, uiEffectName));
    }

    // シーン変更の実行
    IEnumerator DoChangeScene(string sceneName, string uiEffectName = null) {
        uiEffectName = uiEffectName ?? DEFAULT_UI_EFFECT_NAME;

        // エフェクト特定 (ゲームオブジェクトタグから)
        var uiEffect = GameObjectTag<UIEffect>.Find(uiEffectName);
        if (uiEffect == null) {
            Debug.LogError("切り替えエフェクトが見つからない");
        }

        // エフェクト開始
        if (uiEffect != null) {
            uiEffect.Effect();
            while (!uiEffect.IsEffected) {
                yield return null;
            }
        }

        // シーンをロード
        if (uiEffect != null) {
            var asyncop = default(AsyncOperation);
            try {
                asyncop = SceneManager.LoadSceneAsync(sceneName);
            } catch (Exception e) {
                GameApplication.Abort(e.ToString());
                yield break;
            }
            while (!asyncop.isDone) {
                yield return null;
            }
        } else {
            try {
                SceneManager.LoadScene(sceneName);
            } catch (Exception e) {
                GameApplication.Abort(e.ToString());
                yield break;
            }
        }

        // カレントシーン取得
        currentScene = UnityEngine.Object.FindObjectOfType(typeof(GameScene)) as GameScene;
        Debug.Assert(currentScene != null, "シーンに GameScene コンポーネントがいない");

        // TODO
        #if UNITY_EDITOR && (UNITY_5_0 || UNITY_5_1 || UNITY_5_2_0)
        if (UnityEditor.Lightmapping.giWorkflowMode == UnityEditor.Lightmapping.GIWorkflowMode.Iterative) {
            DynamicGI.UpdateEnvironment();
        }
        #endif

        // エフェクト解除
        if (uiEffect != null) {
            uiEffect.Uneffect();
            while (!uiEffect.IsUneffected) {
                yield return null;
            }
        }

        // シーン切り替えコルーチン解除
        currentCoroutine = null;
    }

    //-------------------------------------------------------------------------- 切り替え
    // 即座にシーン変更
    // エラーなどで利用
    public static bool ChangeSceneImmediately(string sceneName) {
        try {
            SceneManager.LoadScene(sceneName);
        } catch (Exception e) {
            Debug.LogError(e.ToString());
            return false;
        }
        return true;
    }

    //-------------------------------------------------------------------------- 実装 (MonoBehaviour);
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
