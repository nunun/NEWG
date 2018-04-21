using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 複数行で適切なフォントサイズを決める
public class MultilineBestFit : MonoBehaviour {
    //-------------------------------------------------------------------------- インスタンスの確保と解放
    [SerializeField] float rows = 2.0f;

    RectTransform rectTransform = null;
    Text          text          = null;

    //-------------------------------------------------------------------------- 実装 (MonoBehaviour)
    void Awake() {
        rectTransform = GetComponent<RectTransform>();
        text          = GetComponent<Text>();
        Debug.Assert(rectTransform != null, "RectTransform なし");
        Debug.Assert(text          != null, "Text なし");
    }

    void Update() {
        text.fontSize = (int)(((rectTransform.rect.height / rows) - text.lineSpacing) / 1.7f);
        enabled = false;
    }
}
