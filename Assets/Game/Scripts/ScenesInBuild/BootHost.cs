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
        // TODO
        // マインドリンクに接続して、自分のサーバアドレスとポートを
        // ready で広報し、クライアントがホストに接続できるようにする。
        yield return new WaitForSeconds(0.5f);
        GameSceneManager.ChangeScene("Logo");
    }
}
