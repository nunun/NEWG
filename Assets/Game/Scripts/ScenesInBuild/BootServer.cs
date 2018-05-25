using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Services.Protocols.Models;

// サーバ起動
public class BootServer : GameScene {
    //-------------------------------------------------------------------------- 実装 (MonoBehaviour)
    IEnumerator Start() {
        var sceneName = GameManager.ServerSceneName;

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
        // 外部からセットアップリクエストがあるまで眠る。
        // 何もないシーンで停滞することによりサーバリソースを節約する。
        while (GameMindlinkManager.SetupRequest != null) {
            yield return null;
        }
        sceneName = GameMindlinkManager.SetupRequest.sceneName;

        // シーン切り替え
        Debug.Log("シーンを切り替え (" + sceneName + ") ...");
        GameSceneManager.ChangeSceneImmediately(sceneName);
    }
}
