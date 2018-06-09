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
    PlayerData     playerData     = new PlayerData();     // 自分のプレイヤー情報
    SessionData    sessionData    = new SessionData();    // セッション情報
    CredentialData credentialData = new CredentialData(); // 認証情報

    public static PlayerData     PlayerData     { get { return instance.playerData;     }}
    public static SessionData    SessionData    { get { return instance.sessionData;    }}
    public static CredentialData CredentialData { get { return instance.credentialData; }}

    //-------------------------------------------------------------------------- 初期化
    // ゲームデータの初期化
    void InitializeGameData() {
        // NOTE ゲーム共通のデータ処理はここに追記して下さい。

        // セーブデータのクリアフラグ
        if (GameSettings.ImportCommandLineFlagArgument("-clearSaveData")) {
            Debug.Log("セーブデータをクリア");
            PlayerPrefs.DeleteAll();
        }

        // 認証情報は事前ロード
        GameDataManager.SessionData.Load();
        GameDataManager.CredentialData.Load();

        #if DEBUG
        Debug.LogFormat("GameDataManager: InitializeGameData(): sessionToken = {0}", GameDataManager.SessionData.sessionToken);
        Debug.LogFormat("GameDataManager: InitializeGameData(): signinToken = {0}",  GameDataManager.CredentialData.signinToken);
        #endif
    }
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// 自動インポートの実装
// WebAPI などの JSON レスポンスで、
// "activeData" プロパティ以下のデータを自動インポートするための仕組み。
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
            public PlayerDataImporter playerData = new PlayerDataImporter();

            [Serializable]
            public class SessionDataImporter : ActiveDataImporter<SessionData> {};
            public SessionDataImporter sessionData = new SessionDataImporter();

            [Serializable]
            public class CredentialDataImporter : ActiveDataImporter<CredentialData> {};
            public CredentialDataImporter credentialData = new CredentialDataImporter();
        }
        public ActiveData activeData = new ActiveData();
    }

    //-------------------------------------------------------------------------- 変数
    // インポートデータ
    ImportData importData = new ImportData();

    // 各種インポーターの取得
    // NOTE 自動インポートするデータを増やす場合はここに追記。
    public static ImportData.ActiveData.PlayerDataImporter     PlayerDataImporter     { get { return instance.importData.activeData.playerData; }}
    public static ImportData.ActiveData.SessionDataImporter    SessionDataImporter    { get { return instance.importData.activeData.sessionData; }}
    public static ImportData.ActiveData.CredentialDataImporter CredentialDataImporter { get { return instance.importData.activeData.credentialData; }}

    //-------------------------------------------------------------------------- 初期化
    // インポートデータの初期化
    void InitializeImportData() {
        // 自動インポート設定
        // NOTE 自動インポートするデータを増やす場合はここに追記。
        GameDataManager.PlayerDataImporter.data     = playerData;
        GameDataManager.SessionDataImporter.data    = sessionData;
        GameDataManager.CredentialDataImporter.data = credentialData;

        // 自動インポートのイベントリスナ設定
        // NOTE 自動インポートするデータを増やす場合はここに追記。
        GameDataManager.SessionDataImporter.AddImportEventListener(() => {
            #if DEBUG
            Debug.LogFormat("GameDataManager: InitializeImportData(): [import] sessionToken = {0}", GameDataManager.SessionData.sessionToken);
            #endif

            // WebAPI アクセスのセッショントークン更新
            var client = WebAPIClient.GetClient();
            client.SessionParameters.SetHeader("SessionToken", GameDataManager.SessionData.sessionToken);

            // セーブ
            GameDataManager.SessionData.Save();
        });
        GameDataManager.CredentialDataImporter.AddImportEventListener(() => {
            #if DEBUG
            Debug.LogFormat("GameDataManager: InitializeImportData(): [import] signinToken = {0}", GameDataManager.CredentialData.signinToken);
            #endif

            // セーブ
            GameDataManager.CredentialData.Save();
        });
    }

    //-------------------------------------------------------------------------- 操作
    // インポート
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
        InitializeGameData();
        InitializeImportData();
    }

    void OnDestroy() {
        if (instance != this) {
            return;
        }
        instance = null;
    }
}
