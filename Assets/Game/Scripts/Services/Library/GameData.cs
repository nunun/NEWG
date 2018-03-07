using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

// ゲームデータ
public class GameData {
    //-------------------------------------------------------------------------- 初期化
    // コンストラクタ
    public GameData() {
        Init();
    }

    //-------------------------------------------------------------------------- 実装ポイント
    // 初期化
    protected virtual void Init() {
        Clear();
    }

    // クリア
    protected virtual void Clear() {
        // NOTE
        // 継承して実装
    }
}
