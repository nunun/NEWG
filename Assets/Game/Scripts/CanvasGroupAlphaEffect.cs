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

    float currentTime = 0.0f;
    bool  reverse     = false;

    //-------------------------------------------------------------------------- 実装 (UIEffect)
    // エフェクト再生状態の変更時
    protected override void OnEffect(bool play, float normalizedTime) {
        enabled = play;
        currentTime = normalizedTime * effectTime;
        reverse = false;
        UpdateAlpha();
    }

    // エフェクト再生時間の変更時
    protected override void OnUneffect(bool play, float normalizedTime) {
        enabled = play;
        currentTime = normalizedTime * effectTime;
        reverse = true;
        UpdateAlpha();
    }

    //-------------------------------------------------------------------------- 実装 (MonoBehaviour)
    void Awake() {
        effectTime = (effectTime > 0.0f)? effectTime : DEFAULT_EFFECT_TIME;
        SetEffectVisibility(false);
    }

    void Update() {
        currentTime += Time.deltaTime;
        UpdateAlpha();
        if (currentTime >= effectTime ) {
            if (reverse) {
                Uneffected();
            } else {
                Effected();
            }
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
