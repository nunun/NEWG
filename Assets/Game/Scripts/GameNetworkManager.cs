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
    public override void OnStartServer() {
        base.OnStartServer();
        CheckActiveServer();
    }

    //-------------------------------------------------------------------------- 制御
    // サーバ起動チェック
    void CheckActiveServer() {
        if (!this.isNetworkActive) {
            Invoke("CheckActiveServer", 0.1f);// NOTE 繰り返し
            return;
        }
        if (serverSpawnInfo.autoCreateServer) {
            if (server == null) {
                Debug.Log("GameNetworkManager: サーバ生成");
                server = GameObject.Instantiate(serverSpawnInfo.serverPrefab);
                UnityEngine.Networking.NetworkServer.Spawn(server);
            }
        }
    }
}
