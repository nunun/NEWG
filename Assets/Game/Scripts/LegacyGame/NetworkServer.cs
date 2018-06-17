using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

// ネットワークサーバ
public class NetworkServer : Server.NetworkServerBehaviour {
    //-------------------------------------------------------------------------- 変数
    public GameObject serverPrefab = null; // サーバのプレハブ

    // このネットワークサーバが操作しているサーバオブジェクト
    public override Server server { get; protected set; }

    // サーバインスタンス
    static NetworkServer instance = null;

    // サーバインスタンスの取得
    public static NetworkServer Instance { get { return instance; }}

    //-------------------------------------------------------------------------- 実装 (MonoBehaviour)
    void Awake() {
        if (instance != null) {
            return;
        }
        instance = this;
    }

    void OnDestroy() {
        if (server != null) {
            GameObject.Destroy(server.gameObject);//NOTE プレイヤーが残っていたら消しておく
        }
        if (instance != this) {
            return;
        }
        instance = null;
    }

    void Start() {
        // NOTE
        // ネットワークプレイヤーの作成と同時に、必ずプレイヤーを作成する。
        // 初期化は Player の Start() 内で行われる。
        var serverObject = GameObject.Instantiate(serverPrefab);
        server = serverObject.GetComponent<Server>();
        Debug.Assert(server != null, "プレハブに Server コンポーネントが含まれていない?");
    }
}
