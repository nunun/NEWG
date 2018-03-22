﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ゲームのメイン処理
// ここからゲームが始まる
public class Logo : GameScene {
    //-------------------------------------------------------------------------- 実装 (MonoBehaviour)
    IEnumerator Start() {
        yield return new WaitForSeconds(2.0f);
        GameSceneManager.ChangeScene("Title");
    }
}