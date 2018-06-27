using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// テキストビルダー
public class TextBuilder : MonoBehaviour {
    //------------------------------------------------------------------------- 変数
    public Text  text = null; // ビルド対象のテキスト

    float time = 0.0f; // 表示時間

    StringBuilder stringBuilder     = new StringBuilder(64, 2048);
    StringBuilder lastStringBuilder = new StringBuilder(64, 2048);

    //------------------------------------------------------------------------- 操作
    public TextBuilder Begin(string s) {
        stringBuilder.Length = 0;
        stringBuilder.Append(s);
        return this;
    }

    public TextBuilder Begin(int i) {
        stringBuilder.Length = 0;
        stringBuilder.Append(i);
        return this;
    }

    public TextBuilder Begin(float f) {
        stringBuilder.Length = 0;
        stringBuilder.Append(f);
        return this;
    }

    public TextBuilder Append(string s) {
        stringBuilder.Append(s);
        return this;
    }

    public TextBuilder Append(int i) {
        stringBuilder.Append(i);
        return this;
    }

    public TextBuilder Append(float f) {
        stringBuilder.Append(f);
        return this;
    }

    public TextBuilder Apply(bool forceUpdate = false) {
        // 更新が必要か？
        if (!forceUpdate && stringBuilder.Equals(lastStringBuilder)) {
            return this;
        }

        // テキスト更新
        var text = stringBuilder.ToString();
        this.text.text = text;

        // キャパシティが違うと比較できないので調整
        if (lastStringBuilder.Capacity != stringBuilder.Capacity) {
            lastStringBuilder.Capacity = stringBuilder.Capacity;
        }

        // 最後の文字列を更新
        lastStringBuilder.Length   = 0;
        lastStringBuilder.Append(text);
        return this;
    }

    public TextBuilder For(float time) {
        this.time    = time;
        this.enabled = true;
        return this;
    }

    //------------------------------------------------------------------------- 実装 (MonoBehaviour)
    void Awake() {
        Debug.Assert(text != null, "テキストの設定なし");
    }

    void Start() {
        this.enabled = (time > 0.0f);
    }

    void Update() {
        time -= Time.deltaTime;
        if (time <= 0.0f) {
            Begin("").Apply(true);
            enabled = false;
            return;
        }
    }
}
