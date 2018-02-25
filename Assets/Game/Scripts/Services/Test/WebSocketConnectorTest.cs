using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;

public class WebSocketConnectorTest : WebSocketConnector, IMonoBehaviourTest {
    //-------------------------------------------------------------------------- 変数
    public bool IsTestFinished { get; private set; }

    //-------------------------------------------------------------------------- 実装 (MonoBehaviour)
    void Start() {
        AddConnectEventListner(() => {
            Disconnect();
        });
        AddDisconnectEventListner((error) => {
            if (error != null) {
                Debug.LogError(error);
            }
            IsTestFinished = true;
        });

        // Start Connection
        url = "ws://localhost:7766";
        Connect();
    }
}
