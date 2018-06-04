using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

#if UNITY_EDITOR
using System.IO;
using System.Text;
#endif

// ゲーム設定の適用
[DefaultExecutionOrder(int.MinValue)]
public partial class GameSettingsApply : MonoBehaviour {
    //-------------------------------------------------------------------------- 変数
    // ゲーム設定を適用するターゲットコンポーネント
    public Component targetComponent = null;

    //-------------------------------------------------------------------------- 操作
    // ゲーム設定の適用
    public void Apply() {
        var runtimeGameSettings = GameSettingsManager.GetRuntimeGameSettings();
        if (targetComponent is WebAPIClient) {
            var webapiClient = (WebAPIClient)targetComponent;
            webapiClient.url = runtimeGameSettings.webapiUrl;
        } else if (targetComponent is MindlinkConnector) {
            var mindlinkConnector = (MindlinkConnector)targetComponent;
            mindlinkConnector.url = runtimeGameSettings.mindlinkUrl;
        } else if (targetComponent is GameNetworkManager) {
            var gameNetworkManager = (GameNetworkManager)targetComponent;
            gameNetworkManager.networkAddress = runtimeGameSettings.serverAddress;
            gameNetworkManager.networkPort    = runtimeGameSettings.serverPort;
        }
    }
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// MonoBehaviour 実装
public partial class GameSettingsApply {
    //-------------------------------------------------------------------------- 実装 (MonoBehaviour)
    void Awake() {
        GameSettingsManager.AddUpdateEventListener(Apply);
        Apply();
    }

    void OnDestroy() {
        GameSettingsManager.RemoveUpdateEventListener(Apply);
    }
}
