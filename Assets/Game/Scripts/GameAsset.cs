using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ゲームアセット
public abstract class GameAsset {
    //-------------------------------------------------------------------------- 実装ポイント
    public abstract AsyncOperation Load();
    public abstract void           Unload();
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// 通常のロード
// GameAsset<NetworkEmulator> asset = new GameAsset<NetworkEmulator>("DataTables/NetworkEmulator");
// yield return asset.Load();
// var networkEmulator = asset.Loaded;
// asset.Unload();
public partial class GameAsset<T> : GameAsset where T : UnityEngine.Object {
    //-------------------------------------------------------------------------- 変数
    string         path           = null; // ロードするパス
    AsyncOperation asyncOperation = null; // 非同期オペレーション

    // ロードしたアセット
    T loaded = default(T);

    // ロードしたアセットの取得
    public T Loaded {
        get {
            if (loaded != default(T)) {
                return loaded;
            }
            if (asyncOperation != null) {
                var resourceRequest = asyncOperation as ResourceRequest;
                if (resourceRequest != null && resourceRequest.isDone) {
                    loaded = resourceRequest.asset as T;
                }
            }
            return default(T);
        }
    }

    //-------------------------------------------------------------------------- 生成と破棄
    public GameAsset(string path) {
        this.path           = path;
        this.asyncOperation = null;
        this.loaded         = default(T);
    }

    //-------------------------------------------------------------------------- ロードとアンロード
    // アセットのロード
    public override AsyncOperation Load() {
        Unload();
        asyncOperation = Resources.LoadAsync(path, typeof(T));
        return asyncOperation;
    }

    // アセットのアンロード
    public override void Unload() {
        var resourceRequest = asyncOperation as ResourceRequest;
        if (resourceRequest != null && resourceRequest.asset != null) {
            Resources.UnloadAsset(resourceRequest.asset);
        }
        asyncOperation = null;
        loaded         = default(T);
    }
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// スタティックなアセットのロード
// yield return GameAsset<NetworkEmulator>.Cache("DataTables/NetworkEmulator");
// var networkEmulator = GameAsset<NetworkEmulator>.Cached;
// GameAsset<NetworkEmulator>.Uncache();
public partial class GameAsset<T> {
    //-------------------------------------------------------------------------- 変数
    static GameAsset<T> asset = null; // キャッシュアセット

    // キャッシュアセットの取得
    public static T Cached {
        get {
            if (asset != null) {
                return asset.Loaded;
            }
            return default(T);
        }
    }

    //-------------------------------------------------------------------------- キャッシュとアンキャッシュ
    // アセットのキャッシュ
    public static AsyncOperation Cache(string path) {
        Uncache();
        asset = new GameAsset<T>(path);
        return asset.Load();
    }

    // アセットのアンキャッシュ
    public static void Uncache() {
        if (asset != null) {
            asset.Unload();
        }
        asset = null;
    }
}
