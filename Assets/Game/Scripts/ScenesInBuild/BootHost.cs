using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Services.Protocols;

// ホスト起動
public class BootHost : GameScene {
    //-------------------------------------------------------------------------- 実装 (MonoBehaviour)
    IEnumerator Start() {
        var sceneName = "Logo";

        #if STANDALONE_MODE
        // ネットワークエミュレーションモード時
        if (GameManager.IsStandaloneMode) {
            GameSceneManager.ChangeSceneImmediately(sceneName);
            yield break;
        }
        #endif

        // マインドリンク接続開始
        GameMindlinkManager.StartConnect();
        while (!GameMindlinkManager.IsDone) {
            yield return null;
        }

        // NOTE
        // ホストモードではセットアップリクエストに従わないので
        // ここでは待たない。
        //while (GameMindlinkManager.SetupRequest != null) {
        //    yield return null;
        //}
        //sceneName = GameSceneManager.SetupRequest.sceneName;

        // シーン切り替え
        Debug.Log("シーンを切り替え (" + sceneName + ") ...");
        GameSceneManager.ChangeScene(sceneName);
    }
}
