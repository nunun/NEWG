using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

// ネットワークサーバ
// サーバが起動するとすべてのクライアントで生成されるネットワークオブジェクトです。
// ネットワークサーバはサーバオブジェクトを生成して保持します。
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

        // NOTE
        // サーバオブジェクト作成
        var serverObject = GameObject.Instantiate(serverPrefab);
        server = serverObject.GetComponent<Server>();
        this.Link(server);
    }

    void OnDestroy() {
        if (server != null) {
            GameObject.Destroy(server.gameObject);//NOTE サーバが残っていたら消しておく
        }
        if (instance != this) {
            return;
        }
        instance = null;
    }
}
