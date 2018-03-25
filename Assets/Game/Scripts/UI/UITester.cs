using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UITester : MonoBehaviour {
    //-------------------------------------------------------------------------- 変数
    UIObject uiObject      = null;
    bool     destroyOnDone = false;

    //-------------------------------------------------------------------------- 実装 (MonoBehaviour)
    void Start() {
        foreach (var t in this.gameObject.scene.GetRootGameObjects()) {
            var component = t.gameObject.GetComponent<UIObject>();
            if (component != null) {
                uiObject = component;
                break;
            }
        }
        Debug.Assert(uiObject != null, "テスト対象の UI 無し");
        if (destroyOnDone) {
            uiObject.DestroyOnDone();
        } else {
            uiObject.DontDestroyOnDone();
        }
    }

    void OnGUI() {
        GUILayout.BeginHorizontal();
        {
            GUILayout.BeginHorizontal("box");
            {
                if (GUILayout.Button("Hide")) {
                    uiObject.Hide();
                }
                if (GUILayout.Button("Open")) {
                    uiObject.Open();
                }
                if (GUILayout.Button("Close")) {
                    uiObject.Close();
                }
                if (GUILayout.Button("Done")) {
                    uiObject.Done();
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal("box");
            {
                if (destroyOnDone) {
                    if (GUILayout.Button("DontDestroyOnDone")) {
                        uiObject.DontDestroyOnDone();
                        destroyOnDone = false;
                    }
                } else {
                    if (GUILayout.Button("DestroyOnDone")) {
                        uiObject.DestroyOnDone();
                        destroyOnDone = true;
                    }
                }
            }
            GUILayout.EndHorizontal();
        }
        GUILayout.EndHorizontal();
    }
}
