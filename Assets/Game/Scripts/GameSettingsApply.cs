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
public partial class GameSettingsApply : MonoBehaviour {
    //-------------------------------------------------------------------------- 変数
    // ゲーム設定を適用するターゲットコンポーネント
    public Component targetComponent = null;

    //-------------------------------------------------------------------------- 操作
    // ゲーム設定の適用
    public void Apply() {
        var applied = false;
        if (targetComponent is WebAPIClient) {
            var webapiClient = (WebAPIClient)targetComponent;
            webapiClient.url = GameSettings.WebAPIURL;
            applied = true;
        } else if (targetComponent is MindlinkConnector) {
            var mindlinkConnector = (MindlinkConnector)targetComponent;
            mindlinkConnector.url = GameSettings.MindlinkURL;
            applied = true;
        } else if (targetComponent is GameNetworkManager) {
            var gameNetworkManager = (GameNetworkManager)targetComponent;
            gameNetworkManager.networkAddress    = GameSettings.ServerAddress;
            gameNetworkManager.networkPort       = GameSettings.ServerPort;
            gameNetworkManager.serverBindAddress = GameSettings.ServerAddress;
            gameNetworkManager.serverBindToIP    = GameSettings.BindIP;
            gameNetworkManager.useWebSockets     = GameSettings.UseWebSockets;
            gameNetworkManager.logLevel          = GameSettings.LogLevel;
            applied = true;
        } else if (targetComponent is WebSocketConnector) {
            var webSocketConnector = (WebSocketConnector)targetComponent;
            webSocketConnector.url = GameSettings.MatchingServerUrl;
            applied = true;
        }
        if (applied) {
            Debug.LogFormat("GameSettingsApply: コンポーネント '{0}' にゲーム設定を適用しました", targetComponent);
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
        GameSettings.AddUpdateEventListener(Apply);
        Apply();
    }

    void OnDestroy() {
        GameSettings.RemoveUpdateEventListener(Apply);
    }
}
