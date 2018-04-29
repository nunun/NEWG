using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ゲームアセットリスト
// ゲームアセットをまとめてロードするために使用。
// GameAssetList gameAssetList = new GameAssetList() {
//      new GameAsset<TestPrefab>("Prefabs/TestPrefab"),
// };
// yield return gameAssetList.Load();
// var testPrefab = gameAssetList.GetAsset<TestPrefab>();
// gameAssetList.Unload();
public class GameAssetList : GameAssetManager.GameAssetList {
    //-------------------------------------------------------------------------- 操作
    // ゲームアセットを探す
    public T GetAsset<T>(string name = null) where T : class {
        name = name ?? typeof(T).Name;
        for (int i = 0; i < this.Count; i++) {
            var gameAsset = this[i];
            if (gameAsset.Name == name) {
                var typedGameAsset = gameAsset as GameAsset<T>;
                return typedGameAsset.Loaded;
            }
        }
        return default(T);
    }
}
