using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Services.Protocols;

// サーバ起動
public class BootServer : GameScene {
    //-------------------------------------------------------------------------- 実装 (MonoBehaviour)
    IEnumerator Start() {
        // TODO
        // MindlinkConnectorTest を参考にして実装
        //var connector = MindlinkConnector.GetConnector();
        yield return null;
    }
}
