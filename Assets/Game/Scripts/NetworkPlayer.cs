using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

// ネットワークプレイヤー
public class NetworkPlayer : Player.NetworkPlayerBase {
    //-------------------------------------------------------------------------- 変数
    public GameObject playerPrefab = null; // プレイヤーのプレハブ

    // このネットワークプレイヤーが操作しているプレイヤーオブジェクト
    // (Player.NetworkPlayerBase の実装)
    public override Player player { get; protected set; }

    // 自分のインスタンス
    static NetworkPlayer instance = null;

    // 存在する全てのプレイヤーのインスタンス
    static List<NetworkPlayer> instances = new List<NetworkPlayer>();

    // 自分のインスタンスの取得
    public static NetworkPlayer Instance { get { return instance; }}

    // 自分のインスタンスの取得
    public static List<NetworkPlayer> Instances { get { return instances; }}

    //-------------------------------------------------------------------------- 実装 (MonoBehaviour)
    void Awake() {
        instances.Add(this);
    }

    public override void OnStartLocalPlayer() {
        instance = this;
    }

    void OnDestroy() {
        instances.Remove(this);
        if (instance == this) {
            instance = null;
        }

        // NOTE
        // プレイヤーが残っていたら消しておく
        if (player != null) {
            GameObject.Destroy(player.gameObject);
        }
    }

    void Start() {
        // NOTE
        // ネットワークプレイヤーの作成と同時に、必ずプレイヤーを作成する。
        // 初期化は Player の Start() 内で行われる。
        var playerObject = GameObject.Instantiate(playerPrefab);
        player = playerObject.GetComponent<Player>();
        Debug.Assert(player != null, "プレハブに Player スクリプトが含まれていない?");
    }

    //-------------------------------------------------------------------------- ユーティリティ
    // プレイヤーからインスタンスを探す
    public static NetworkPlayer FindByPlayer(Player player) {
        for (int i = 0; i < instances.Count; i++) {
            var networkPlayer = instances[i];
            if (networkPlayer.player == player) {
                return networkPlayer;
            }
        }
        return null;
    }

    // ネットID からインスタンスを探す
    public static NetworkPlayer FindByNetId(NetworkInstanceId netId) {
        for (int i = 0; i < instances.Count; i++) {
            var networkPlayer = instances[i];
            if (networkPlayer.netId == netId) {
                return networkPlayer;
            }
        }
        return null;
    }

    // プレイヤーコントローラID からインスタンスを探す
    public static NetworkPlayer FindByPlayerControllerId(short playerControllerId) {
        for (int i = 0; i < instances.Count; i++) {
            var networkPlayer = instances[i];
            if (networkPlayer.playerControllerId == playerControllerId) {
                return networkPlayer;
            }
        }
        return null;
    }
}

////-------------------------------------------------------------------------- スポーン
//// TODO
//// これは Player に移しておく。
//// [C 1-> S] スポーンをリクエスト
//public void Spawn() {
//    Debug.Assert(this.isLocalPlayer, "ローカル限定です");
//    CmdSpawn(); // TODO
//}
//
//// [S ->* C] スポーンをばらまき
//[Command]
//public void CmdSpawn() {
//    Debug.Assert(this.isServer, "サーバ限定です");
//    var position = Vector3.zero;
//    var rotation = Quaternion.identity;
//    SpawnPoint.GetRandomSpawnPoint(out position, out rotation);
//    RpcSpawn(position, rotation);
//}
//
//[ClientRpc]
//public void RpcSpawn(Vector3 position, Quaternion rotation) {
//    player.transform.position = position;
//    player.transform.rotation = rotation;
//    var rigidbody = player.GetComponent<Rigidbody>();
//    rigidbody.velocity        = new Vector3(0f,0f,0f);
//    rigidbody.angularVelocity = new Vector3(0f,0f,0f);
//    if (this.isLocalPlayer) {
//        GameCamera.Instance.SetBattleMode(player);
//    }
//}
