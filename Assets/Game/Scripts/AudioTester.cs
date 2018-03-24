using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioTester : MonoBehaviour {
    //-------------------------------------------------------------------------- 定義
    static readonly int NITEM = 5;

    //-------------------------------------------------------------------------- 変数
    List<SEPlayer>  sePlayerList  = new List<SEPlayer>();
    int             sePage        = 0;
    int             sePageMax     = 0;
    string          seName        = null;

    List<BGMPlayer> bgmPlayerList = new List<BGMPlayer>();
    int             bgmPage       = 0;
    int             bgmPageMax    = 0;
    string          bgmName       = null;

    //-------------------------------------------------------------------------- 実装 (MonoBehaviour)
    void Start() {
        foreach (var t in this.gameObject.scene.GetRootGameObjects()) {
            var sePlayer = t.gameObject.GetComponent<SEPlayer>();
            if (sePlayer != null) {
                sePlayerList.Add(sePlayer);
            }
            var bgmPlayer = t.gameObject.GetComponent<BGMPlayer>();
            if (bgmPlayer != null) {
                bgmPlayerList.Add(bgmPlayer);
            }
        }
        bgmPage    = 0;
        bgmPageMax = bgmPlayerList.Count / NITEM;
    }

    void OnGUI() {
        GUILayout.BeginVertical();
        {
            GUILayout.BeginHorizontal("box");
            {
                GUILayout.BeginHorizontal("box");
                {
                    if (GUILayout.Button("Play")) {
                        GameAudio.Play(this.seName);
                    }
                    if (GUILayout.Button("Stop")) {
                        GameAudio.Stop(this.seName);
                    }
                }
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal("box");
                {
                    GUILayout.Label(string.Format("{0}/{1}", sePage + 1, sePageMax + 1));
                    GUI.enabled = (sePage > 0);
                    if (GUILayout.Button("<")) {
                        sePage = Mathf.Clamp(sePage - 1, 0, sePageMax);
                    }
                    GUI.enabled = (sePage < sePageMax);
                    if (GUILayout.Button(">")) {
                        sePage = Mathf.Clamp(sePage + 1, 0, sePageMax);
                    }
                    GUI.enabled = true;
                    for (int i = (sePage * NITEM); i < ((sePage * NITEM) + NITEM); i++) {
                        if (i >= sePlayerList.Count) {
                            continue;
                        }
                        var sePlayer = sePlayerList[i];
                        var seName   = sePlayer.name;
                        GUI.enabled = (this.seName != seName);
                        if (GUILayout.Button(seName)) {
                            this.seName = seName;
                        }
                        GUI.enabled = true;
                    }
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal("box");
            {
                GUILayout.BeginHorizontal("box");
                {
                    if (GUILayout.Button("PlayBGM")) {
                        GameAudio.PlayBGM(this.bgmName);
                    }
                    if (GUILayout.Button("MixBGM")) {
                        GameAudio.MixBGM(this.bgmName);
                    }
                    if (GUILayout.Button("StopBGM")) {
                        GameAudio.StopBGM(this.bgmName);
                    }
                    if (GUILayout.Button("StopBGM (All)")) {
                        GameAudio.StopBGM();
                    }
                }
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal("box");
                {
                    GUILayout.Label(string.Format("{0}/{1}", bgmPage + 1, bgmPageMax + 1));
                    GUI.enabled = (bgmPage > 0);
                    if (GUILayout.Button("<")) {
                        bgmPage = Mathf.Clamp(bgmPage - 1, 0, bgmPageMax);
                    }
                    GUI.enabled = (bgmPage < bgmPageMax);
                    if (GUILayout.Button(">")) {
                        bgmPage = Mathf.Clamp(bgmPage + 1, 0, bgmPageMax);
                    }
                    GUI.enabled = true;
                    for (int i = (bgmPage * NITEM); i < ((bgmPage * NITEM) + NITEM); i++) {
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
            }
            GUILayout.EndHorizontal();
        }
    }
}
