using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UITester : MonoBehaviour {
    //-------------------------------------------------------------------------- 変数
    UIBehaviour uiBehaviour   = null;
    bool        destroyOnDone = false;

    //-------------------------------------------------------------------------- 実装 (MonoBehaviour)
    void Start() {
        foreach (var t in this.gameObject.scene.GetRootGameObjects()) {
            var component = t.gameObject.GetComponent<UIBehaviour>();
            if (component != null) {
                uiBehaviour = component;
                break;
            }
        }
        Debug.Assert(uiBehaviour != null, "テスト対象の UI 無し");
        if (destroyOnDone) {
            uiBehaviour.DestroyOnDone();
        } else {
            uiBehaviour.DontDestroyOnDone();
        }
    }

    void OnGUI() {
        GUILayout.BeginHorizontal();
        {
            GUILayout.BeginHorizontal("box");
            {
                if (GUILayout.Button("Hide")) {
                    uiBehaviour.Hide();
                }
                if (GUILayout.Button("Open")) {
                    uiBehaviour.Open();
                }
                if (GUILayout.Button("Close")) {
                    uiBehaviour.Close();
                }
                if (GUILayout.Button("Done")) {
                    uiBehaviour.Done();
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal("box");
            {
                if (destroyOnDone) {
                    if (GUILayout.Button("DontDestroyOnDone")) {
                        uiBehaviour.DontDestroyOnDone();
                        destroyOnDone = false;
                    }
                } else {
                    if (GUILayout.Button("DestroyOnDone")) {
                        uiBehaviour.DestroyOnDone();
                        destroyOnDone = true;
                    }
                }
            }
            GUILayout.EndHorizontal();
        }
        GUILayout.EndHorizontal();
    }
}
