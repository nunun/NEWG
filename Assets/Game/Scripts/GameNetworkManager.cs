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

    //-------------------------------------------------------------------------- 実装 (UnityEngine.Networking.NetworkManager)
    public override void OnStartServer() {
        Debug.Log("GameNetworkManager: サーバ開始");
        if (serverSpawnInfo.autoCreateServer) {
            var server = GameObject.Instantiate(serverSpawnInfo.serverPrefab);
            UnityEngine.Networking.NetworkServer.Spawn(server);
        }
        base.OnStartServer();
    }
}
