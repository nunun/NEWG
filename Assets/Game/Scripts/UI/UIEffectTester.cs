using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIEffectTester : MonoBehaviour {
    //-------------------------------------------------------------------------- 実装 (MonoBehaviour)
    public UIEffect uiEffect = null;

    //-------------------------------------------------------------------------- 実装 (MonoBehaviour)
    void OnGUI() {
        GUILayout.BeginVertical();
        {
            GUILayout.BeginHorizontal("box");
            {
                if (GUILayout.Button("Effect")) {
                    uiEffect.Effect();
                }
                if (GUILayout.Button("SetEffected")) {
                    uiEffect.SetEffected();
                }
                if (GUILayout.Button("Effected")) {
                    uiEffect.Effected();
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal("box");
            {
                if (GUILayout.Button("Uneffect")) {
                    uiEffect.Uneffect();
                }
                if (GUILayout.Button("SetUneffected")) {
                    uiEffect.SetUneffected();
                }
                if (GUILayout.Button("Uneffected")) {
                    uiEffect.Uneffected();
                }
            }
            GUILayout.EndHorizontal();
        }
        GUILayout.EndVertical();
    }
}
