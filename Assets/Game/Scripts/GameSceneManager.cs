using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// ゲームシーンマネージャ
public class GameSceneManager : MonoBehaviour {
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
    public static ChangeSchene(string sceneName, string uiEffectName = null) {
        Debug.Assert(currentCoroutine == null);
        currentCoroutine = instance.StartCoroutine(DoChangeScene(sceneName, uiEffectName));
    }

    // シーン変更の実行
    IEnumerator DoChangeScene(string sceneName, string uiEffectName = null) {
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
        try {
            if (uiEffect != null) {
                var asyncop = SceneManager.LoadSceneAsync(sceneName);
                while (!asyncop.isDone) {
                    yield return null;
                }
            } else {
                SceneManager.LoadScene(sceneName);
            }
        } catch (Exception e) {
            GameApplication.Abort(e.ToString());
            yield break;
        }

        // カレントシーン取得
        currentScene = Object.FindObjectOfType(typeof(GameScene));
        Debug.Assert(currentScene == null, "シーンに GameScene コンポーネントがいない");

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
            SceneManagement.LoadScene(sceneName);
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
