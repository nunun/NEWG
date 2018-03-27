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
    [SerializeField] protected PlayerData     player_data     = new PlayerData();
    [SerializeField] protected SessionData    session_data    = new SessionData();
    [SerializeField] protected CredentialData credential_data = new CredentialData();

    public static PlayerData     PlayerData     { get { return instance.player_data;     }}
    public static SessionData    SessionData    { get { return instance.session_data;    }}
    public static CredentialData CredentialData { get { return instance.credential_data; }}
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// TODO
// FromJsonOverwrite による取り込みは MonoBehaviour には無効なので
// GameDataManager.ServerData.server_data を調整する必要がある。
// 一段階プレーンなクラスでラップして、JsonFromOverwrite で
// 正しく上書きされるようにする必要がある。
// なお、ISerializationCallbackReceiver の OnAfterDeserialize はコールされた。
// カウンターなどを使って、Model が IsDirty かどうかの判定に利用できる。

// 自動更新対応
// WebAPI などで Json のレスポンスを受けたとき、
// ゲームデータマネージャのシリアライズメンバを自動的に上書き更新。
public partial class GameDataManager {
    //-------------------------------------------------------------------------- 定義
    // インポート用サーバデータ
    [Serializable]
    public class ServerData {
        public GameDataManager server_data;
    }

    //-------------------------------------------------------------------------- 変数
    ServerData serverData = new ServerData();

    // 更新イベントリスナ
    Action updateEventListener = null;

    //-------------------------------------------------------------------------- インポート
    public static void FromJsonOverwrite(string message) {
        Debug.Assert(instance != null, "GameDataManager がいない");
        instance.serverData.server_data = instance;
        JsonUtility.FromJsonOverwrite(message, instance.serverData);
        if (instance.updateEventListener != null) {
            instance.updateEventListener();
        }
    }

    //-------------------------------------------------------------------------- イベントリスナ
    public static void AddUpdateEventListener(Action eventListener) {
        Debug.Assert(instance != null, "GameDataManager がいない");
        instance.updateEventListener += eventListener;
    }

    public static void RemoveUpdateEventListener(Action eventListener) {
        Debug.Assert(instance != null, "GameDataManager がいない");
        instance.updateEventListener -= eventListener;
    }

    public static void ClearUpdateEventListener() {
        Debug.Assert(instance != null, "GameDataManager がいない");
        instance.updateEventListener = null;
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
