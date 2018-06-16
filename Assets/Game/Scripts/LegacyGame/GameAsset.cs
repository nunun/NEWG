using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ゲームアセット
// GameAsset<NetworkEmulator> gameAsset = new GameAsset<NetworkEmulator>("DataTables/NetworkEmulator");
// yield return gameAsset.Load();
// var networkEmulator = gameAsset.Loaded;
// gameAsset.Unload();
public partial class GameAsset<T> : GameAsset where T : class {
    //-------------------------------------------------------------------------- 変数
    string         name           = null; // 名前
    string         path           = null; // ロードするパス
    T              asset          = null; // ロード済アセット
    AsyncOperation asyncOperation = null; // 非同期オペレーション

    // ロードしたアセットの取得
    public T Loaded {
        get {
            return GetLoaded() as T;
        }
    }

    //-------------------------------------------------------------------------- コンストラクタ
    public GameAsset(string name, string path) {
        this.name = name;
        this.path = path;
    }

    public GameAsset(string path) : this() {
        this.name = typeof(T).Name;
        this.path = path;
    }

    protected GameAsset() {
        this.name           = null;
        this.path           = null;
        this.asset          = null;
        this.asyncOperation = null;
    }

    //-------------------------------------------------------------------------- 実装 (GameAsset)
    // ゲームアセットのロード
    public override AsyncOperation Load() {
        Unload();
        asset          = default(T);
        asyncOperation = Resources.LoadAsync(path, typeof(T));
        return asyncOperation;
    }

    // ゲームアセットの即時ロード
    public override void LoadImmediately() {
        asset          = Resources.Load(path, typeof(T)) as T;
        asyncOperation = null;
    }

    // ゲームアセットのアンロード
    public override void Unload() {
        if (asset != default(T)) {
            Resources.UnloadAsset(asset as UnityEngine.Object);
        }
        var resourceRequest = asyncOperation as ResourceRequest;
        if (resourceRequest != null && resourceRequest.asset != null) {
            Resources.UnloadAsset(resourceRequest.asset);
        }
        asset          = default(T);
        asyncOperation = null;
    }

    // ロード済ゲームアセットの取得
    protected override object GetLoaded() {
        if (IsLoaded) {
            if (asset != default(T)) {
                return asset;
            }
            var resourceRequest = asyncOperation as ResourceRequest;
            if (resourceRequest != null && resourceRequest.isDone) {
                asset = resourceRequest.asset as T;
                return asset;
            }
        }
        return null;
    }

    // ロード中かどうか
    protected override bool GetIsLoading() {
        return (asset != default(T) || asyncOperation != null);
    }

    // ロード完了かどうか
    protected override bool GetIsLoaded() {
        return (asset != default(T) || (asyncOperation != null && asyncOperation.isDone));
    }

    // 名前の取得
    protected override string GetName() {
        return this.name;
    }
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// ゲームアセット
public abstract class GameAsset {
    //-------------------------------------------------------------------------- 変数
    public string Name      { get { return GetName();      }}
    public bool   IsLoading { get { return GetIsLoading(); }}
    public bool   IsLoaded  { get { return GetIsLoaded();  }}

    //-------------------------------------------------------------------------- 実装ポイント
    public    abstract AsyncOperation Load();
    public    abstract void           LoadImmediately();
    public    abstract void           Unload();
    protected abstract object         GetLoaded();
    protected abstract bool           GetIsLoading();
    protected abstract bool           GetIsLoaded();
    protected abstract string         GetName();
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
//
// ゲームアセットのキャッシュ
// yield return GameAsset<NetworkEmulator>.Cache("DataTables/NetworkEmulator");
// var networkEmulator = GameAsset<NetworkEmulator>.Cached;
// GameAsset<NetworkEmulator>.Uncache();
//public partial class GameAsset<T> {
//    //-------------------------------------------------------------------------- 変数
//    // キャッシュしたアセットアセット
//    static GameAsset<T> cached = null;
//
//    // キャッシュしたゲームアセットの取得
//    public static T Cached {
//        get {
//            if (cached != null) {
//                return cached.Loaded;
//            }
//            return default(T);
//        }
//    }
//
//    //-------------------------------------------------------------------------- キャッシュとアンキャッシュ
//    // ゲームアセットのキャッシュ
//    public static AsyncOperation Cache(string path) {
//        Uncache();
//        cached = new GameAsset<T>(path);
//        return cached.Load();
//    }
//
//    // ゲームアセットのアンキャッシュ
//    public static void Uncache() {
//        if (cached != null) {
//            cached.Unload();
//        }
//        cached = null;
//    }
//}
//
//
