using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// テキストに設定します。
// 複数行で適切なフォントサイズを決定して自動設定します。
// TODO UnityEditor 上での自動設定
[ExecuteInEditMode]
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
        var fontSize = (int)(((rectTransform.rect.height / rows) - text.lineSpacing) / 1.7f);
        if (text.fontSize != fontSize) {
            text.fontSize = fontSize;
        }
    }
}
