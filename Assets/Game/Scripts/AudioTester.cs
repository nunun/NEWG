using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioTester : MonoBehaviour {
    //-------------------------------------------------------------------------- 定義
    static readonly int NITEM = 5;

    //-------------------------------------------------------------------------- 変数
    List<BGMPlayer> bgmPlayerList = new List<BGMPlayer>();
    int             page          = 0;
    int             pageMax       = 0;
    string          bgmName       = null;

    //-------------------------------------------------------------------------- 実装 (MonoBehaviour)
    void Start() {
        foreach (var t in this.gameObject.scene.GetRootGameObjects()) {
            var component = t.gameObject.GetComponent<BGMPlayer>();
            if (component != null) {
                bgmPlayerList.Add(component);
            }
        }
        page    = 0;
        pageMax = bgmPlayerList.Count / NITEM;
    }

    void OnGUI() {
        GUILayout.BeginVertical();
        {
            GUILayout.BeginHorizontal("box");
            {
                GUILayout.Label(string.Format("{0}/{1}", page + 1, pageMax + 1));
                GUI.enabled = (page > 0);
                if (GUILayout.Button("<")) {
                    page = Mathf.Clamp(page - 1, 0, pageMax);
                }
                GUI.enabled = (page < pageMax);
                if (GUILayout.Button(">")) {
                    page = Mathf.Clamp(page + 1, 0, pageMax);
                }
                GUI.enabled = true;
                for (int i = (page * NITEM); i < ((page * NITEM) + NITEM); i++) {
                    if (i >= bgmPlayerList.Count) {
                        continue;
                    }
                    var bgmPlayer = bgmPlayerList[i];
                    var bgmName   = bgmPlayer.name;
                    GUI.enabled = (this.bgmName != bgmName);
                    if (GUILayout.Button(bgmName)) {
                        this.bgmName = bgmName;
                    }
                    GUI.enabled = true;
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal("box");
            {
                if (GUILayout.Button("PlayBGM")) {
                    AudioManager.PlayBGM(this.bgmName);
                }
                if (GUILayout.Button("MixBGM")) {
                    AudioManager.MixBGM(this.bgmName);
                }
                if (GUILayout.Button("StopBGM")) {
                    AudioManager.StopBGM(this.bgmName);
                }
                if (GUILayout.Button("StopBGM (All)")) {
                    AudioManager.StopBGM();
                }
            }
            GUILayout.EndHorizontal();
        }
        GUILayout.EndHorizontal();
    }
}
