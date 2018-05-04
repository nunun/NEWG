using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ゲームアセットマネージャ
// ゲームで使用するアセット (プレハブ、スクリプタブルオブジェクト) を管理します。
[DefaultExecutionOrder(int.MinValue)]
public partial class GameAssetManager : MonoBehaviour {
    // NOTE
    // パーシャルクラスを参照
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// ロードの追加と削除とロード処理
public partial class GameAssetManager {
    //-------------------------------------------------------------------------- 変数
    // ロード中のリスト
    protected List<GameAssetListLoadOperation> loadingList = new List<GameAssetListLoadOperation>();

    //-------------------------------------------------------------------------- ロードとアンロード
    // アセットリストのロード
    protected static GameAssetListLoadOperation Load(GameAssetList gameAssetList) {
        var loading = new GameAssetListLoadOperation(gameAssetList);
        instance.loadingList.Add(loading);
        instance.enabled = true; // NOTE 更新を有効に設定
        return loading;
    }

    // アセットリストのアンロード
    protected static void Unload(GameAssetList gameAssetList) {
        for (int i = instance.loadingList.Count - 1; i >= 0; i--) {
            var loading = instance.loadingList[i];
            if (loading.gameAssetList == gameAssetList) {
                instance.loadingList.RemoveAt(i);
                loading.CancelLoad();
            }
        }
        for (int i = gameAssetList.Count - 1; i >= 0; i--) {
            gameAssetList[i].Unload();
        }
    }
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// ゲームアセットリスト
public partial class GameAssetManager {
    public class GameAssetList : List<GameAsset> {
        //---------------------------------------------------------------------- ロードとアンロード
        // ロード
        public void Load() {
            GameAssetManager.Load(this);
        }

        // アンロード
        public void Unload() {
            GameAssetManager.Unload(this);
        }
    }
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// ゲームアセットリスト ロードオペレーション
public partial class GameAssetManager {
    public class GameAssetListLoadOperation : CustomYieldInstruction {
        //---------------------------------------------------------------------- 変数
        int loadedCount = 0; // ロード済カウント

        // ゲームアセットリストの取得
        public GameAssetList gameAssetList { get; protected set; }

        // ロードが完了したかどうかの取得
        public bool isDone { get { return (this.gameAssetList != null && (this.loadedCount >= this.gameAssetList.Count)); }}

        // エラーの取得
        public string error { get; protected set; }

        // CustomYieldInstruction の実装
        public override bool keepWaiting { get { return !isDone; }}

        //---------------------------------------------------------------------- コンストラクタ
        public GameAssetListLoadOperation(GameAssetList gameAssetList) {
            this.gameAssetList = gameAssetList;
            this.loadedCount   = 0;
            this.error         = null;
        }

        //---------------------------------------------------------------------- 操作
        // ロード更新
        public void UpdateLoad() {
            // 完了？
            if (isDone) {
                return;
            }

            // ロード対象取得
            var gameAsset = gameAssetList[loadedCount];
            Debug.Assert(gameAsset != null, "ロード対象が null");

            // ロードしていないならロード開始
            if (!gameAsset.IsLoading) {
                gameAsset.Load();
            }

            // ロードが終わったら次
            if (gameAsset.IsLoaded) {
                loadedCount++;
            }
        }

        // ロードキャンセル
        public void CancelLoad() {
            var isDone = this.isDone;

            // ロードカウントを吹き飛ばして強制ロード済にする
            this.loadedCount = int.MaxValue;

            // 完了していないならエラー設定
            this.error = (isDone)? null : "cancelled.";
        }
    }
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// MonoBehaviour 側の実装
public partial class GameAssetManager {
    //-------------------------------------------------------------------------- 変数
    // インスタンス
    static GameAssetManager instance = null;

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

    void Update() {
        // 常に先頭のロードだけをチェック
        var loading = default(GameAssetListLoadOperation);
        do {
            // ロードオペレーションリストが空になった？
            if (loadingList.Count <= 0) {
                enabled = false; // NOTE 更新を無効に設定
                return;
            }

            // ロードが終わったかチェックする
            loading = loadingList[0];
            if (loading.isDone) {
                loadingList.RemoveAt(0);
            }
        } while(loading.isDone);

        // ロード更新
        loading.UpdateLoad();
    }
}
