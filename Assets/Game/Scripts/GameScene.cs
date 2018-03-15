using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// ゲームシーンマネージャ
public class GameSceneManager : MonoBehaviour {
    //-------------------------------------------------------------------------- 変数
    // 現在のシーン
    static GameScene currentScene = null;

    // 現在走っているコルーチン
    static Coroutine currentCoroutine = null;

    // インスタンス
    static GameSceneManager instance = null;

    // 現在のシーンの取得
    public static GameScene CurrentScene { get { return currentScene; }}

    //-------------------------------------------------------------------------- 操作
    // シーンの切り替え
    public static ChangeScene(string sceneName, string uiEffectName = null) {
        Debug.Assert(currentCoroutine == null);
        currentCoroutine = instance.StartCoroutine(DoChange(sceneName, uiEffectName));
    }

    // シーンの切り替えコルーチン
    IEnumerator DoChange(string sceneName, string uiEffectName = null) {
        // エフェクト特定 (ゲームオブジェクトタグから)
        var uiEffect = GameObjectTag<UIEffect>.Find(uiEffectName);
        Debug.Assert(uiEffect != null, "切り替えエフェクトが見つからない");

        // エフェクト開始
        uiEffect.Effect();
        while (!uiEffect.IsEffected) {
            yield return null;
        }

        // ロード開始
        var asyncop = SceneManager.LoadSceneAsync(sceneName);
        while (!asyncop.isDone) {
            yield return null;
        }

        // シーンの切り替え
        currentScene = Object.FindObjectOfType(typeof(GameScene));
        Debug.Assert(currentScene == null, "シーンに GameScene コンポーネントがいない");

        // エフェクト解除
        uiEffect.Uneffect();
        while (!uiEffect.IsUneffected) {
            yield return null;
        }

        // シーン切り替えコルーチン解除
        currentCoroutine = null;
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
