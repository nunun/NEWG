using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Services.Protocols;

// クライアント起動
public class BootClient : GameScene {
    //-------------------------------------------------------------------------- 実装 (MonoBehaviour)
    IEnumerator Start() {
        yield return new WaitForSeconds(0.5f);
        GameSceneManager.ChangeScene("Logo");
    }
}
