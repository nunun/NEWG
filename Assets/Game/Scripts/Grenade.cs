using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

// グレネードの実装
public partial class Grenade : MonoBehaviour {
    //-------------------------------------------------------------------------- 変数
    public float      damage          = 100.0f; // 最大爆発ダメージ
    public float      range           = 2.4f;   // 爆発時のダメージ範囲
    public GameObject explosionPrefab = null;   // 爆発のプレハブ

    // 撃った人
    [NonSerialized] public Player thrower = null;

    // 爆発までの時間
    [NonSerialized] public float explosionTime = 3.0f;

    //-------------------------------------------------------------------------- 実装 (MonoBehaviour)
    void Start() {
        Debug.Assert(explosionPrefab != null, "爆発プレハブの設定なし");
    }

    void Update() {
        var deltaTime = Time.deltaTime;

        // 爆発まで飛ぶ
        explosionTime -= deltaTime;
        if (explosionTime >= 0.0f) {
            return;
        }

        // NOTE
        // 今の所は近距離ダメージ
        var networkPlayers = NetworkPlayer.Instances;
        for (int i = 0; i < networkPlayers.Count; i++) {
            var networkPlayer = networkPlayers[i];
            if (networkPlayer == null) {
                continue;
            }
            var player = networkPlayer.player;
            if (player == null) {
                continue;
            }

            var distance = (player.transform.position - transform.position).magnitude;
            if (distance <= range) {
                thrower.DealDamage(player, (int)(damage * (1.0f - (distance / range))));
            }
        }

        // 爆発
        // TODO
        // Bullet.cs を含めて、エフェクトの総数を管理する必要がある。
        GameObject.Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        GameObject.Destroy(this.gameObject);
    }
}
