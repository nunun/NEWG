using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIComponentTester : MonoBehaviour {
    //-------------------------------------------------------------------------- 変数
    public UIComponent uiComponent   = null;

    bool destroyOnDone = false;

    //-------------------------------------------------------------------------- 実装 (MonoBehaviour)
    void Awake() {
        if (destroyOnDone) {
            uiComponent.DestroyOnDone();
        } else {
            uiComponent.DontDestroyOnDone();
        }
    }

    void OnGUI() {
        GUILayout.BeginHorizontal();
        {
            GUILayout.BeginHorizontal("box");
            {
                if (GUILayout.Button("Hide")) {
                    uiComponent.Hide();
                }
                if (GUILayout.Button("Open")) {
                    uiComponent.Open();
                }
                if (GUILayout.Button("Close")) {
                    uiComponent.Close();
                }
                if (GUILayout.Button("Done")) {
                    uiComponent.Done();
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal("box");
            {
                if (destroyOnDone) {
                    if (GUILayout.Button("DontDestroyOnDone")) {
                        uiComponent.DontDestroyOnDone();
                        destroyOnDone = false;
                    }
                } else {
                    if (GUILayout.Button("DestroyOnDone")) {
                        uiComponent.DestroyOnDone();
                        destroyOnDone = true;
                    }
                }
            }
            GUILayout.EndHorizontal();
        }
        GUILayout.EndHorizontal();
    }
}
