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
        var sceneName = "Logo";

        // とりま待ちを入れておく
        yield return new WaitForSeconds(0.5f);

        // シーン切り替え
        Debug.Log("シーンを切り替え (" + sceneName + ") ...");
        GameSceneManager.ChangeScene(sceneName);
    }
}
