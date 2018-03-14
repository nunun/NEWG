using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// UI エフェクト
// 全ての UI エフェクトの基礎クラス
public class CanvasGroupAlphaEffect : UIEffect {
    //-------------------------------------------------------------------------- 変数
    public const float DEFAULT_EFFECT_TIME = 0.2f;

    //-------------------------------------------------------------------------- 変数
    public CanvasGroup canvasGroup = null;
    public float       effectTime  = 0.0f;
    public bool        reverse     = false;

    float currentTime = 0.0f;

    //-------------------------------------------------------------------------- 実装 (UIEffect)
    // エフェクト再生状態の変更時
    protected override void OnEffectPlay(bool play) {
        enabled = play;
    }

    // エフェクト再生時間の変更時
    protected override void OnEffectPlayTime(float normalizedTime) {
        currentTime = normalizedTime * effectTime;
        UpdateAlpha();
    }

    //-------------------------------------------------------------------------- 実装 (MonoBehaviour)
    void Awake() {
        effectTime = (effectTime > 0.0f)? effectTime : DEFAULT_EFFECT_TIME;
        Stop();
    }

    void Update() {
        currentTime += Time.deltaTime;
        UpdateAlpha();
        if (currentTime >= effectTime ) {
            Done();
        }
    }

    //-------------------------------------------------------------------------- アルファ更新
    void UpdateAlpha() {
        if (canvasGroup != null) {
            var alpha = Mathf.Min(currentTime / effectTime, 1.0f);
            canvasGroup.alpha = (reverse)? 1.0f - alpha : alpha;
        }
    }
}
