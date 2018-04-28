using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ゲームアセットマネージャ
// ゲームで使用するアセットを管理します。
[DefaultExecutionOrder(int.MinValue)]
public partial class GameAssetManager : MonoBehaviour {
    //-------------------------------------------------------------------------- 定義
    // ゲームアセット一覧
    public class GameAssets : List<GameAsset> {
        public bool isLoaded = false;
        public void Load() {
            GameAssetManager.Load(this);
        }
        public void Unload() {
            GameAssetManager.Unload(this);
        }
    }

    // ゲームアセット一覧 ロードオペレーション
    public class GameAssetsLoadOperation : CustomYieldInstruction {
        public GameAssets gameAssets = null;
        public GameAssetsLoadOperation(GameAssets gameAssets) { this.gameAssets = gameAssets; }
        public override bool keepWaiting { get { return (this.gameAssets != null && !this.gameAssets.isLoaded); }}
    }

    //-------------------------------------------------------------------------- 変数
    // アセット一覧 ロードオペレーションリスト
    protected List<GameAssetsLoadOperation> gameAssetsLoadOperationList = new List<GameAssetsLoadOperation>();

    //-------------------------------------------------------------------------- ロードとアンロード
    // アセットリストのロード
    protected static GameAssetsLoadOperation Load(GameAssets gameAssets) {
        var gameAssetsLoadOperation = new GameAssetsLoadOperation(gameAssets);
        gameAssetsLoadOperation.gameAssets = gameAssets;
        gameAssets.isLoaded = false;
        instance.gameAssetsLoadOperationList.Add(gameAssetsLoadOperation);
        instance.enabled = true; // NOTE 更新を有効に設定
        return gameAssetsLoadOperation;
    }

    // アセットリストのアンロード
    protected static void Unload(GameAssets gameAssets) {
        var gameAssetsLoadOperationList = instance.gameAssetsLoadOperationList;
        for (int i = gameAssetsLoadOperationList.Count - 1; i >= 0; i--) {
            var gameAssetsLoadOperation = gameAssetsLoadOperationList[i];
            if (gameAssetsLoadOperation.gameAssets == gameAssets) {
                gameAssetsLoadOperationList.RemoveAt(i);
            }
        }
        for (int i = gameAssets.Count - 1; i >= 0; i--) {
            gameAssets[i].Unload();
            gameAssets[i] = null;
        }
        gameAssets.Clear();
        gameAssets.isLoaded = true;
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
        // TODO
        // ここで共通のロードを走らせる

        // ロードオペレーションリストが空になった？
        if (gameAssetsLoadOperationList.Count <= 0) {
            enabled = false; // NOTE 更新を無効に設定
            return;
        }

        // TODO
        // ロードオペレーションリストを更新
    }
}
