using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

// ゲームネットワークマネージャ
[DefaultExecutionOrder(int.MinValue)]
public class GameNetworkManager : NetworkManager {
    //-------------------------------------------------------------------------- 変数
    [Serializable]
    public class ServerSpawnInfo {
        public GameObject serverPrefab     = null;
        public bool       autoCreateServer = true;
    }

    //-------------------------------------------------------------------------- 変数
    public ServerSpawnInfo serverSpawnInfo = new ServerSpawnInfo(); // サーバスポーン情報

    // サーバインスタンス (初期化確認用)
    GameObject server = null;

    //-------------------------------------------------------------------------- 実装 (UnityEngine.Networking.NetworkManager)
    public override void OnServerReady(NetworkConnection conn) {
        base.OnServerReady(conn);

        // NOTE
        // 以下は OnStartServer においていたが
        // OnStartServer では Spawn できないのでここに書く。
        if (serverSpawnInfo.autoCreateServer) {
            if (server == null) {
                Debug.Log("GameNetworkManager: サーバ生成");
                server = GameObject.Instantiate(serverSpawnInfo.serverPrefab);
                UnityEngine.Networking.NetworkServer.Spawn(server);
            }
        }
    }
}
