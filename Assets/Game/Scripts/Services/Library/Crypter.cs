using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

// クリプター
// 通信路の入出力をチェックし、暗号化と復号化を行います。
public class Crypter {
    //-------------------------------------------------------------------------- 初期化
    // TODO
    //string cryptSetting = null;

    //-------------------------------------------------------------------------- 初期化
    // コンストラクタ
    public Crypter(string cryptSetting) {
        Init(cryptSetting);
    }

    //-------------------------------------------------------------------------- 実装ポイント (初期化とクリア)
    // 初期化
    protected virtual void Init(string cryptSetting) {
        // TODO
        //this.cryptSetting = cryptSetting;
        Clear();
    }

    // クリア
    protected virtual void Clear() {
        // TODO
        // this.cryptSetting の値に応じて
        // エンクリプターを初期化
    }

    //-------------------------------------------------------------------------- 暗号化と復号化
    // 暗号化
    public string Encrypt(string message) {
        // TODO
        // this.cryptSetting の値に応じて
        // 暗号化ロジックの実装
        return message;
    }

    // 復号化
    public string Decrypt(string message) {
        // TODO
        // this.cryptSetting の値に応じて
        // 復号化ロジックの実装
        return message;
    }
}
