using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Services.Protocols.Models;

// ゲームデータマネージャ
// ゲームで使用するデータを保持します。
[DefaultExecutionOrder(int.MinValue)]
public partial class GameDataManager : MonoBehaviour {
    //-------------------------------------------------------------------------- 変数
    public static PlayerData     PlayerData     = new PlayerData();
    public static SessionData    SessionData    = new SessionData();
    public static CredentialData CredentialData = new CredentialData();
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// 自動インポート対応
public partial class GameDataManager {
    //-------------------------------------------------------------------------- 定義
    // 自動インポート用データ構造
    // 自動インポートするフィールドを増やす場合はここに追記。
    // 取り込みフィールド名とデータへの参照をここに書いておくと、
    // WebAPI などを受けたときに、自動的に上書きしてくれる。
    [Serializable]
    public class ImportData {
        [Serializable]
        public class ActiveData {
            // TODO
            // リファクタ
            // 更新のあったデータはコールバックが呼ばれるようにする。
            [Serializable]
            public class ActiveDataContainer<TData> : ISerializationCallbackReceiver {
                public bool  active  = false;
                public TData content = default(TData);
                public void OnBeforeSerialize() {
                    active = false;
                }
                public void OnAfterDeserialize() {
                    if (active) {
                        Debug.Log("Active!");
                    }
                }
            }
            [Serializable] public class PlayerDataContainer : ActiveDataContainer<PlayerData> {};
            public PlayerDataContainer player_data = new PlayerDataContainer() { content = GameDataManager.PlayerData };
        }
        public ActiveData active_data = new ActiveData();
    }

    //-------------------------------------------------------------------------- 変数
    // インポート用データインスタンス
    ImportData importData = new ImportData();

    //-------------------------------------------------------------------------- インポート
    public static void Import(string message) {
        Debug.Assert(instance != null, "GameDataManager がいない");
        JsonUtility.FromJsonOverwrite(message, instance.importData);
    }
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// MonoBehaviour 側の実装
public partial class GameDataManager {
    //-------------------------------------------------------------------------- 変数
    // インスタンス
    static GameDataManager instance = null;

    //-------------------------------------------------------------------------- 実装 (MonoBehaviour)
    void Awake() {
        if (instance != null) {
            GameObject.Destroy(this.gameObject);
            return;
        }
        instance = this;
    }

    void OnDestroy() {
        if (instance != this) {
            return;
        }
        instance = null;
    }
}
