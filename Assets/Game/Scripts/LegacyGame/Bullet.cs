using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

// 弾の実装
public partial class Bullet : MonoBehaviour {
    //-------------------------------------------------------------------------- 変数
    public int        damage       = 10;     // 弾のダメージ
    public float      speed        = 715.0f; // 弾速 (m/s)
    public float      flyTime      = 0.5f;   // 飛翔時間
    public GameObject hitPrefab    = null;   // ヒット時のエフェクトのプレハブ
    public GameObject impactPrefab = null;   // 床に当たったときのプレハブ

    // 撃った人
    [NonSerialized] public Player shooter = null;

    // ヒットエフェクト
    static List<GameObject> hitEffects = new List<GameObject>();

    //-------------------------------------------------------------------------- 実装 (MonoBehaviour)
    void Start() {
        Debug.Assert(hitPrefab,    "ヒットプレハブなし");
        Debug.Assert(impactPrefab, "インパクトプレハブなし");
    }

    void Update() {
        Fly();
    }

    //-------------------------------------------------------------------------- 処理
    // 飛翔
    void Fly() {
        var deltaTime = Time.deltaTime;
        var distance  = speed * deltaTime;
        var hit       = default(RaycastHit);

        // 衝突判定
        if (Physics.Raycast(transform.position, transform.forward, out hit, distance)) {
            Impact(hit);
            return;
        }

        // 移動
        transform.position += transform.forward * distance;

        // 飛翔時間満了？
        flyTime -= deltaTime;
        if (flyTime <= 0.0f) {
            Disappear();
            return;
        }
    }

    // 当たった
    void Impact(RaycastHit hit) {
        var playerCollider = hit.collider.GetComponent<PlayerCollider>();
        var hitEffect = default(GameObject);
        if (playerCollider != null) {
            shooter.DealDamage(playerCollider.player, damage);
            hitEffect = GameObject.Instantiate(hitPrefab, hit.point, Quaternion.LookRotation(hit.normal));
        } else {
            hitEffect = GameObject.Instantiate(impactPrefab, hit.point, Quaternion.LookRotation(hit.normal));
        }

        // ヒットエフェクトは総数を設けておく
        hitEffects.Add(hitEffect);
        if (hitEffects.Count >= 10) {
            hitEffect = hitEffects[0];
            if (hitEffect != null) {
                GameObject.Destroy(hitEffect);
            }
            hitEffects.RemoveAt(0);
        }

        GameObject.Destroy(this.gameObject);
    }

    // 射程外消失
    void Disappear() {
        GameObject.Destroy(this.gameObject);
    }
}
