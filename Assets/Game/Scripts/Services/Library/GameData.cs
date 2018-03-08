using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

// ゲームデータ
[Serializable]
public class GameData {
    //-------------------------------------------------------------------------- 変数
    [NonSerialized]
    protected string currentPrefsKey = null;

    // クリプタ
    protected static Crypter crypter = new Crypter("GameData");

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

    //-------------------------------------------------------------------------- 実装ポイント
    // ロード
    public bool Load(string prefsKey = null) {
        prefsKey = prefsKey ?? this.GetType().Name;
        if (!PlayerPrefs.HasKey(prefsKey)) {
            return false;
        }
        var jsonText = PlayerPrefs.GetString(prefsKey, null);
        if (jsonText == null) {
            return false;
        }
        try {
            JsonUtility.FromJsonOverwrite(crypter.Decrypt(jsonText), this);
        } catch (Exception e) {
            Debug.LogError(e.ToString());
            return false;
        }
        return true;
    }

    // セーブ
    public bool Save(string prefsKey = null) {
        var jsonText = default(string);
        try {
            jsonText = JsonUtility.ToJson(this);
        } catch (Exception e) {
            Debug.LogError(e.ToString());
            return false;
        }
        prefsKey = prefsKey ?? this.GetType().Name;
        PlayerPrefs.SetString(prefsKey, crypter.Encrypt(jsonText));
        PlayerPrefs.Save();
        return true;
    }
}
