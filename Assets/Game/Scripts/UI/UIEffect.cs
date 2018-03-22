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
    // 現在状態
    State currentState = State.Uneffected;

    // イベント
    [Serializable]
    public struct UIEffectEvents {
        public UnityEvent onEffect;        // 表示を開始したとき
        public UnityEvent onSetEffected;   // 初期化時などで即座に表示に設定されたとき
        public UnityEvent onEffected;      // 表示を完了したとき
        public UnityEvent onUneffect;      // 非表示を開始したとき
        public UnityEvent onSetUneffected; // 初期化時などで即座に非表示に設定されたとき
        public UnityEvent onUneffected;    // 非表示を完了したとき
    }
    [SerializeField] UIEffectEvents events;

    // 現在状態の取得
    public bool IsEffecting   { get { return currentState == State.Effecting;   }}
    public bool IsEffected    { get { return currentState == State.Effected;    }}
    public bool IsUneffecting { get { return currentState == State.Uneffecting; }}
    public bool IsUneffected  { get { return currentState == State.Uneffected;  }}

    // イベント
    public UnityEvent onEffect        { get { return events.onEffect;        }}
    public UnityEvent onSetEffected   { get { return events.onSetEffected;   }}
    public UnityEvent onEffected      { get { return events.onEffected;      }}
    public UnityEvent onUneffect      { get { return events.onUneffect;      }}
    public UnityEvent onSetUneffected { get { return events.onSetUneffected; }}
    public UnityEvent onUneffected    { get { return events.onUneffected;    }}

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
        Debug.AssertFormat(currentState == State.Uneffected, "アンエフェクト済ではない ({0}/{1})", name, currentState);
        currentState = State.Effecting;
        OnEffect(true, normalizedTime);
        onEffect.Invoke();
    }

    // エフェクト完了に設定
    public void SetEffected() {
        Debug.AssertFormat(currentState == State.Effected || currentState == State.Uneffected, "エフェクト中 ({0}/{1})", name, currentState);
        currentState = State.Effected;
        OnEffect(false, 1.0f);
        onSetEffected.Invoke();
    }

    // エフェクト完了にする
    public void Effected() {
        Debug.AssertFormat(currentState == State.Effecting, "エフェクト中ではない ({0}/{1})", name, currentState);
        currentState = State.Effected;
        OnEffect(false, 1.0f);
        onEffected.Invoke();
    }

    //-------------------------------------------------------------------------- エフェクト
    // アンエフェクト
    public void Uneffect(float normalizedTime = 0.0f) {
        Debug.AssertFormat(currentState == State.Effected, "エフェクト済ではない ({0}/{1})", name, currentState);
        currentState = State.Uneffecting;
        OnUneffect(true, normalizedTime);
        onUneffect.Invoke();
    }

    // アンエフェクト完了に設定
    public void SetUneffected() {
        Debug.AssertFormat(currentState == State.Effected || currentState == State.Uneffected, "エフェクト中 ({0}/{1})", name, currentState);
        currentState = State.Uneffected;
        OnUneffect(false, 1.0f);
        onSetUneffected.Invoke();
    }

    // アンエフェクト完了にする
    public void Uneffected() {
        Debug.AssertFormat(currentState == State.Uneffecting, "アンエフェクト中ではない ({0}/{1})", name, currentState);
        currentState = State.Uneffected;
        OnUneffect(false, 1.0f);
        onUneffected.Invoke();
    }
}
