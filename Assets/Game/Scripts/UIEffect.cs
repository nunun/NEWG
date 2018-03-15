using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// [UI エフェクトの実装例]
//public class SampleUIEffect : UIEffect {
//    float currentTime = 0.0f;
//    float effectTime  = 3.0f;
//    bool  effect      = false;
//    public override void OnEffect(bool play, float normalizedTime) {
//        enabled = play;
//        currentTime = normalizedTime * effectTime;
//        color.alpha = normalizedTime;
//        effect = true;
//    }
//    public override void OnUneffect(bool play, float normalizedTime) {
//        enabled = play;
//        currentTime = normalizedTime * effectTime;
//        color.alpha = 1.0f - normalizedTime;
//        effect = false;
//    }
//    void Update() {
//        currentTime += Time.deltaTime;
//        var normalizedTime = Mathf.Min(currentTime / effectTime, 1.0f);
//        color.a = (effect)? normalizedTime : 1.0f - normalizedTime;
//        if (currentTime > 1.0f) {
//            if (effect) {
//                Effected();
//            } else {
//                Uneffected();
//            }
//        }
//    }
//}

// UI エフェクト
// 全ての UI エフェクトの基礎クラス
public class UIEffect : MonoBehaviour {
    //-------------------------------------------------------------------------- 変数
    public enum State { Effecting, Effected, Uneffecting, Uneffected };

    //-------------------------------------------------------------------------- 変数
    public UnityEvent onEffected   = new UnityEvent();
    public UnityEvent onUneffected = new UnityEvent();

    // 現在状態
    State currentState = State.Uneffected;

    // エフェクト済かどうか
    public bool IsEffected { get { return currentState == State.Effected; }}

    // アンエフェクト済かどうか
    public bool IsUneffected { get { return currentState == State.Uneffected; }}

    //-------------------------------------------------------------------------- 実装ポイント
    // エフェクト再生時間の変更時
    protected virtual void OnEffect(bool play, float normalizedTime) {
        // NOTE
        // 継承して実装
    }

    // アンエフェクト再生状態の変更時
    protected virtual void OnUneffect(bool play, float normalizedTime) {
        // NOTE
        // 継承して実装
    }

    //-------------------------------------------------------------------------- エフェクト
    // エフェクト
    public void Effect(float normalizedTime = 0.0f) {
        Debug.Assert(currentState == State.Uneffected);
        currentState = State.Effecting;
        OnEffect(true, normalizedTime);
    }

    // エフェクト完了に設定
    public void SetEffected() {
        Debug.Assert(currentState == State.Effected || currentState == State.Uneffected);
        currentState = State.Effected;
        OnEffect(false, 1.0f);
    }

    // エフェクト完了にする
    public void Effected() {
        Debug.Assert(currentState == State.Effecting);
        currentState = State.Effected;
        OnEffect(false, 1.0f);
        onEffected.Invoke();
    }

    //-------------------------------------------------------------------------- エフェクト
    // アンエフェクト
    public void Uneffect(float normalizedTime = 0.0f) {
        Debug.Assert(currentState == State.Effected);
        currentState = State.Uneffecting;
        OnUneffect(true, normalizedTime);
    }

    // アンエフェクト完了に設定
    public void SetUneffected() {
        Debug.Assert(currentState == State.Effected || currentState == State.Uneffected);
        currentState = State.Uneffected;
        OnUneffect(false, 1.0f);
    }

    // アンエフェクト完了にする
    public void Uneffected() {
        Debug.Assert(currentState == State.Uneffecting);
        currentState = State.Uneffected;
        OnUneffect(false, 1.0f);
        onUneffected.Invoke();
    }
}
