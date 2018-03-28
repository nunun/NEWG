using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Services.Protocols.Models;

// TODO
// GameDataManager.active_data を作った方がいいかもしれない。
// model は全て active_data に入れて置き、
// import_data.active_data にリンクする


// ゲームデータマネージャ
// ゲームで使用するデータを保持します。
[DefaultExecutionOrder(int.MinValue)]
public partial class GameDataManager : MonoBehaviour {
    //-------------------------------------------------------------------------- 変数
    public static PlayerData     PlayerData     = new PlayerData();     // プレイヤー情報
    public static SessionData    SessionData    = new SessionData();    // セッション情報
    public static CredentialData CredentialData = new CredentialData(); // 認証情報

    //-------------------------------------------------------------------------- 操作
    // データ初期化
    void InitializeData() {
        // 認証情報は事前ロード
        GameDataManager.SessionData.Load();
        GameDataManager.CredentialData.Load();

        #if DEBUG
        Debug.LogFormat("session_token: {0}", GameDataManager.SessionData.session_token);
        Debug.LogFormat("signin_token: {0}",  GameDataManager.CredentialData.signin_token);
        #endif

        // 自動インポートイベントリスナをセットアップ
        import_data.active_data.session_data.AddImportEventListener(() => {
            #if DEBUG
            Debug.LogFormat("[import] session_token: {0}", GameDataManager.SessionData.session_token);
            #endif
            // TODO
            // WebAPIClient 側のセッション情報を更新
            GameDataManager.SessionData.Save();
        });
        import_data.active_data.credential_data.AddImportEventListener(() => {
            #if DEBUG
            Debug.LogFormat("[import] signin_token: {0}", GameDataManager.CredentialData.signin_token);
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
    // 自動インポートするデータを増やす場合は ActiveData クラスに追記。
    [Serializable]
    public class ImportData {
        [Serializable]
        public class ActiveData {
            [Serializable]
            public class PlayerDataImporter : ActiveDataImporter<PlayerData> {};
            public PlayerDataImporter player_data = new PlayerDataImporter() { data = GameDataManager.PlayerData };

            [Serializable]
            public class SessionDataImporter : ActiveDataImporter<SessionData> {};
            public SessionDataImporter session_data = new SessionDataImporter() { data = GameDataManager.SessionData };

            [Serializable]
            public class CredentialDataImporter : ActiveDataImporter<CredentialData> {};
            public CredentialDataImporter credential_data = new CredentialDataImporter() { data = GameDataManager.CredentialData };
        }
        public ActiveData active_data = new ActiveData();
    }

    //-------------------------------------------------------------------------- 変数
    // インポート用データ
    ImportData importData = new ImportData();

    // インポート用データの取得 (インポートイベントリスナ設定用)
    public static ImportData import_data { get { return instance.importData; }}

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
