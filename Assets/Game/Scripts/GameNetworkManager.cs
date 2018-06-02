using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

// ゲームネットワークマネージャ
[DefaultExecutionOrder(int.MinValue)]
public class GameNetworkManager : NetworkManager {
    //-------------------------------------------------------------------------- 実装 (UnityEngine.Networking.NetworkManager)
    public override void OnStartServer() {
        // TODO
        Debug.Log("GameNetworkManager: OnStartServer(): サーバ開始");
        base.OnStartServer();
    }
}
