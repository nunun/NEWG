using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Evnets;

// [UI エフェクトの実装例]
//public class SampleUIEffect : UIEffect {
//    float effectTime = 3.0f;
//    public override void OnEffectPlay(bool play) {
//        enabled = play;
//    }
//    public override float OnEffectPlayTime(float normalizedTime) {
//        return normalizedTime * effectTime;
//    }
//    public override void OnEffectUpdate(float time) {
//        color.a = Mathf.Min(time / effectTime, 1.0f);
//        if (time >= effectTime) {
//            Done();
//        }
//    }
//}

// UI エフェクト
// 全ての UI エフェクトの基礎クラス
public class UIEffect : MonoBehaviour {
    //-------------------------------------------------------------------------- 変数
    public UnityEvnet onDone      = new UnityEvent();
    public bool       autoCloseUI = false;

    float time = 0.0f; // 現在の再生時間

    //-------------------------------------------------------------------------- 実装ポイント
    // エフェクト再生状態の変更時
    protected virtual void OnEffectPlay(bool play) {
        // NOTE
        // 継承して実装
    }

    // エフェクト再生時間の変更時
    protected virtual float OnEffectPlayTime(float normalizedTime) {
        // NOTE
        // 継承して実装
        return -1.0f;
    }

    // エフェクトの更新
    protected virtual float OnEffectUpdate(float time) {
        // NOTE
        // 継承して実装
    }

    //-------------------------------------------------------------------------- 操作
    // 再生
    public void Play() {
        OnEffectPlay(true);
        enabled = true;
    }

    // 時刻を指定して再生
    public void Play(float normalizedTime) {
        var time = OnEffectPlayTime(normalizedTime);
        if (time >= 0.0f) {
            this.time = time;
        }
        OnEffectUpdate(this.time);
        OnEffectPlay(true);
        enabled = true;
    }

    // 停止
    public void Stop() {
        OnEffectPlay(false);
        enabled = false;
    }

    // 時刻を指定して停止
    public void Stop(float normalizedTime) {
        var time = OnEffectPlayTime(normalizedTime);
        if (time >= 0.0f) {
            this.time = time;
        }
        OnEffectUpdate(this.time);
        OnEffectPlay(false);
        enabled = false;
    }

    // 完了
    public void Done() {
        // 1.0f で停止
        Stop(1.0f);

        // UnityEvent 呼び出し
        onDone.Invoke();

        // UI を自動的に閉じる
        if (autoCloseUI) {
            var uiComponent = gameObject.GetComponent<UIComponent>();
            if (uiComponent != null) {
                uiComponent.Closed();
            }
        }
    }

    //-------------------------------------------------------------------------- 実装 (MonoBehaviour)
    public void Update() {
        time += Time.deltaTime;
        OnEffectUpdate(time);
    }
}
