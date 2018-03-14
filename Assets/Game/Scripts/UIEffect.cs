using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// [UI エフェクトの実装例]
//public class SampleUIEffect : UIEffect {
//    float currentTime = 0.0f;
//    float effectTime  = 3.0f;
//
//    public override void OnEffectPlay(bool play) {
//        enabled = play;
//    }
//
//    public override float OnEffectPlayTime(float normalizedTime) {
//        currentTime = normalizedTime * effectTime;
//        color.alpha = normalizedTime;
//    }
//
//    void Update() {
//        currentTime += Time.deltaTime;
//        color.a = Mathf.Min(currentTime / effectTime, 1.0f);
//        if (currentTime > 1.0f) {
//            Done();
//        }
//    }
//}

// UI エフェクト
// 全ての UI エフェクトの基礎クラス
public class UIEffect : MonoBehaviour {
    //-------------------------------------------------------------------------- 変数
    public UnityEvent onDone = new UnityEvent();

    //-------------------------------------------------------------------------- 実装ポイント
    // エフェクト再生状態の変更時
    protected virtual void OnEffectPlay(bool play) {
        // NOTE
        // 継承して実装
    }

    // エフェクト再生時間の変更時
    protected virtual void OnEffectPlayTime(float normalizedTime) {
        // NOTE
        // 継承して実装
    }

    //-------------------------------------------------------------------------- 操作
    // 再生
    public void Play() {
        OnEffectPlay(true);
    }

    // 時刻を指定して再生
    public void Play(float normalizedTime) {
        OnEffectPlayTime(normalizedTime);
        OnEffectPlay(true);
    }

    // 停止
    public void Stop() {
        OnEffectPlay(false);
    }

    // 時刻を指定して停止
    public void Stop(float normalizedTime) {
        OnEffectPlayTime(normalizedTime);
        OnEffectPlay(false);
    }

    // 完了
    public void Done() {
        Stop(1.0f);
        onDone.Invoke();
    }
}
