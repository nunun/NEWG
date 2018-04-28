using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// アセットインターフェイス
// アセットを共通で操作するためのインターフェイスを定義します。
public interface IAsset<T> where T : UnityEngine.Object {
    //-------------------------------------------------------------------------- 変数
    // アセットインスタンスの取得
    T asset { get; }

    // ロードが完了したかどうかの取得
    bool isLoaded { get; }

    //-------------------------------------------------------------------------- 操作
    // アセットのロード
    void LoadAsset(string path);

    // アセットのアンロード
    void UnloadAsset();
}
