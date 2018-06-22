using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkObjectTester : MonoBehaviour {
    //-------------------------------------------------------------------------- 実装 (MonoBehaviour)
    void OnGUI() {
        GUILayout.BeginHorizontal("box");
        {
            if (GUILayout.Button("GameCamera.SetMenuMode")) {
                var gameCamera = GameCamera.Instance;
                gameCamera.SetMenuMode();
            }
            if (GUILayout.Button("GameCamera.SetBattleMode")) {
                var gameCamera = GameCamera.Instance;
                gameCamera.SetBattleMode(NetworkPlayer.Instance.player);
                NetworkPlayer.Instance.player.Spawn();
            }
        }
        GUILayout.EndHorizontal();
    }
}
