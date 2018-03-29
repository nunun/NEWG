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
    static PlayerData     playerData     = new PlayerData();
    static SessionData    sessionData    = new SessionData();
    static CredentialData credentialData = new CredentialData();

    public static PlayerData     PlayerData     { get { return playerData;     }}
    public static SessionData    SessionData    { get { return sessionData;    }}
    public static CredentialData CredentialData { get { return credentialData; }}

    //-------------------------------------------------------------------------- 操作
    // データ初期化
    void InitializeData() {
        // 認証情報は事前ロード
        GameDataManager.SessionData.Load();
        GameDataManager.CredentialData.Load();

        #if DEBUG
        Debug.LogFormat("sessionToken: {0}", GameDataManager.SessionData.sessionToken);
        Debug.LogFormat("signinToken: {0}",  GameDataManager.CredentialData.signinToken);
        #endif

        // 自動インポートイベントリスナをセットアップ
        GameDataManager.SessionDataImporter.AddImportEventListener(() => {
            #if DEBUG
            Debug.LogFormat("[import] sessionToken: {0}", GameDataManager.SessionData.sessionToken);
            #endif
            // TODO
            // WebAPIClient 側のセッション情報を更新
            GameDataManager.SessionData.Save();
        });
        GameDataManager.CredentialDataImporter.AddImportEventListener(() => {
            #if DEBUG
            Debug.LogFormat("[import] signinToken: {0}", GameDataManager.CredentialData.signinToken);
            #endif
            GameDataManager.CredentialData.Save();
        });
    }
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// 自動インポートの実装
public partial class GameDataManager {
    //-------------------------------------------------------------------------- 定義
    // インポート用データクラス
    // NOTE 自動インポートするデータを増やす場合はここに追記。
    [Serializable]
    public class ImportData {
        [Serializable]
        public class ActiveData {
            [Serializable]
            public class PlayerDataImporter : ActiveDataImporter<PlayerData> {};
            public PlayerDataImporter playerData = new PlayerDataImporter() { data = GameDataManager.PlayerData };

            [Serializable]
            public class SessionDataImporter : ActiveDataImporter<SessionData> {};
            public SessionDataImporter sessionData = new SessionDataImporter() { data = GameDataManager.SessionData };

            [Serializable]
            public class CredentialDataImporter : ActiveDataImporter<CredentialData> {};
            public CredentialDataImporter credentialData = new CredentialDataImporter() { data = GameDataManager.CredentialData };
        }
        public ActiveData activeData = new ActiveData();
    }

    //-------------------------------------------------------------------------- 変数
    // インポート用データ
    ImportData importData = new ImportData();

    // 各種データインポータの取得
    // NOTE 自動インポートするデータを増やす場合はここに追記。
    public static ImportData.ActiveData.PlayerDataImporter     PlayerDataImporter     { get { return instance.importData.activeData.playerData; }}
    public static ImportData.ActiveData.SessionDataImporter    SessionDataImporter    { get { return instance.importData.activeData.sessionData; }}
    public static ImportData.ActiveData.CredentialDataImporter CredentialDataImporter { get { return instance.importData.activeData.credentialData; }}

    //-------------------------------------------------------------------------- インポート
    public static void Import(string message) {
        Debug.Assert(instance != null, "GameDataManager がいない");
        JsonUtility.FromJsonOverwrite(message, instance.importData);
    }
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// アクティブデータインポーターの実装
// FromJsonOverwrite などでシリアライズされたとき
// オブジェクトの書き換えを検知するためのデータラッパークラス。
public partial class GameDataManager {
    [Serializable]
    public class ActiveDataImporter<TData> : ISerializationCallbackReceiver {
        //---------------------------------------------------------------------- 変数
        public bool  active = false;          // アクティブフラグ
        public TData data   = default(TData); // データ本体

        // インポートイベントリスナ
        Action importEventListener = null;

        //---------------------------------------------------------------------- 実装 (ISerializationCallbackReceiver)
        // シリアライズ前
        public void OnBeforeSerialize() {
            // NOTE
            // シリアライズ前にフラグを落とす
            active = false;
        }

        // デシリアライズ後
        public void OnAfterDeserialize() {
            // NOTE
            // シリアライズ後にアクティブフラグなら
            // シリアライズを検知。
            if (active) {
                if (importEventListener != null) {
                    importEventListener();
                }
            }
        }

        //---------------------------------------------------------------------- イベントリスナ操作
        // イベントリスナの追加
        public void AddImportEventListener(Action eventListener) {
            importEventListener += eventListener;
        }

        // イベントリスナの削除
        public void RemoveImportEventListener(Action eventListener) {
            importEventListener -= eventListener;
        }
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

        // NOTE
        // 各種データをセットアップ
        InitializeData();
    }

    void OnDestroy() {
        if (instance != this) {
            return;
        }
        instance = null;
    }
}
