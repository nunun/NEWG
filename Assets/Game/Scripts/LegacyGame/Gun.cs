﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

// 銃の実装
public partial class Gun : MonoBehaviour {
    //-------------------------------------------------------------------------- 変数
    public GameObject     muzzle       = null;  // 銃口の設定
    public ParticleSystem muzzleFlash  = null;  // マズルフラッシュ
    public GameObject     bulletPrefab = null;  // 弾の設定
    public GameObject     leftHandle   = null;  // 左手の位置
    public GameObject     rightHandle  = null;  // 右ての位置
    public float          fireRate     = 10.0f; // 秒間発射
    public AudioSource    fireAudio    = null;  // 発射時の音

    float fireInterval = 0.0f; // 発射間隔

    //-------------------------------------------------------------------------- 実装 (MonoBehaviour)
    void Start() {
        Debug.Assert(muzzle       != null, "銃にマズルが設定されていない");
        //Debug.Assert(muzzleFlash  != null, "銃にマズルフラッシュが設定されていない");
        //Debug.Assert(bulletPrefab != null, "銃に弾が設定されていない");
        //Debug.Assert(leftHandle   != null, "銃にレフトハンドルが設定されていない");
        //Debug.Assert(rightHandle  != null, "銃にライトハンドルが設定されていない");
    }

    //-------------------------------------------------------------------------- 操作
    // 発砲
    public void Fire(Player shooter) {
        float deltaTime = Time.deltaTime;
        fireInterval -= deltaTime;
        if (fireInterval <= 0.0f) {
            fireInterval = 1.0f / fireRate;
            var position = muzzle.transform.position;
            var rotation = muzzle.transform.rotation;
            var bulletObject = GameObject.Instantiate(bulletPrefab, position, rotation);
            var bullet = bulletObject.GetComponent<Bullet>();
            if (bullet != null) {
                bullet.shooter = shooter;
            }
            if (fireAudio != null) {
                fireAudio.Play();
            }
        }
    }

    // マズルフラッシュの有効・無効
    public void MuzzleFlash(bool isFlash) {
        if (isFlash) {
            if (!muzzleFlash.isPlaying) {
                muzzleFlash.Play();
            }
        } else {
            if (muzzleFlash.isPlaying) {
                muzzleFlash.Stop();
            }
        }
    }
}
