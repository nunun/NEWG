using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

// ネットワークプレイヤー
// プレイヤーが接続するとすべてのクライアントで生成されるネットワークオブジェクトです。
// ネットワークプレイヤーはプレイヤーオブジェクトを生成して保持します。
public class NetworkPlayer : Player.NetworkPlayerBehaviour {
    //-------------------------------------------------------------------------- 変数
    public GameObject playerPrefab = null; // プレイヤーのプレハブ

    // このネットワークプレイヤーが操作しているプレイヤーオブジェクト
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
        if (player != null) {
            GameObject.Destroy(player.gameObject);//NOTE プレイヤーが残っていたら消しておく
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
        var playerObject = GameObject.Instantiate(playerPrefab);
        player = playerObject.GetComponent<Player>();
        Debug.Assert(player != null, "プレハブに Player コンポーネントが含まれていない?");
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
