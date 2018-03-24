﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ゲームのメイン処理
// ここからゲームが始まる
public class Logo : GameScene {
    //-------------------------------------------------------------------------- 実装 (MonoBehaviour)
    IEnumerator Start() {
        GameAudio.PlayBGM("Revenge1");
        GameAudio.SetBGMVolume("Revenge1", 0.25f);
        yield return new WaitForSeconds(2.0f);
        GameSceneManager.ChangeScene("Title");
    }
}
