using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

// 音声マネージャ
public partial class AudioManager : MonoBehaviour {
    //-------------------------------------------------------------------------- 変数
    // インスタンス
    static AudioManager instance = null;

    //-------------------------------------------------------------------------- 実装 (MonoBehaviour);
    void Awake() {
        if (instance != null) {
            GameObject.Destroy(this.gameObject);
            return;
        }
        instance = this;
    }

    void OnDestroy() {
        if (instance != this) {
            return;
        }
        instance = null;
    }
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// BGM の対応
public partial class AudioManager {
    //-------------------------------------------------------------------------- 変数
    List<BGMPlayer> bgmPlayerList = new List<BGMPlayer>();

    //-------------------------------------------------------------------------- BGM
    public static void PlayBGM(string bgmName, float fadeTime = 0.5f) {
        var bgmPlayerList = instance.bgmPlayerList;
        var bgmPlayer     = GameObjectTag<BGMPlayer>.Find(bgmName);
        if (bgmPlayerList.Count >= 1) {
            for (int i = bgmPlayerList.Count - 1; i >= 0; i--) {
                var playingBGMPlayer = bgmPlayerList[i];
                if (playingBGMPlayer != bgmPlayer) {
                    bgmPlayerList.RemoveAt(i);
                    if (playingBGMPlayer != null) {
                        playingBGMPlayer.Stop(fadeTime);
                    }
                }
            }
        }
        if (bgmPlayerList.IndexOf(bgmPlayer) < 0) {
            bgmPlayerList.Add(bgmPlayer);
            if (bgmPlayer != null) {
                bgmPlayer.Play(fadeTime);
            }
        }
    }

    public static void MixBGM(string bgmName, float fadeTime = 0.5f) {
        var bgmPlayerList = instance.bgmPlayerList;
        Debug.Assert(bgmPlayerList.Count > 0, "BGM が再生されていない");
        var bgmPlayer    = GameObjectTag<BGMPlayer>.Find(bgmName);
        var bgmSource    = GameObjectTag<AudioSource>.Find(bgmName);
        var masterPlayer = bgmPlayerList[0];
        var masterSource = masterPlayer.gameObject.GetComponent<AudioSource>();
        bgmSource.time = masterSource.time;
        if (bgmPlayerList.IndexOf(bgmPlayer) < 0) {
            bgmPlayerList.Add(bgmPlayer);
            if (bgmPlayer != null) {
                bgmPlayer.Play(fadeTime);
            }
        }
        UpdatePlayingBGMVolumes(fadeTime);
    }

    public static void StopBGM(float fadeTime = 0.5f) {
        var bgmPlayerList = instance.bgmPlayerList;
        for (int i = bgmPlayerList.Count - 1; i >= 0; i--) {
            var bgmPlayer = bgmPlayerList[i];
            bgmPlayerList.RemoveAt(i);
            if (bgmPlayer != null) {
                bgmPlayer.Stop(fadeTime);
            }
        }
        bgmPlayerList.Clear();
    }

    public static void StopBGM(string bgmName, float fadeTime = 0.5f) {
        var bgmPlayerList = instance.bgmPlayerList;
        var bgmPlayer     = GameObjectTag<BGMPlayer>.Find(bgmName);
        bgmPlayerList.Remove(bgmPlayer);
        if (bgmPlayer != null) {
            bgmPlayer.Stop(fadeTime);
        }
        UpdatePlayingBGMVolumes(fadeTime);
    }

    //-------------------------------------------------------------------------- 共通ロジック
    static void UpdatePlayingBGMVolumes(float fadeTime = 0.5f) {
        var bgmPlayerList = instance.bgmPlayerList;
        var count         = bgmPlayerList.Count;
        var volume        = Mathf.Clamp(1.0f - (0.15f * (count - 1)), 0.70f, 1.0f);
        for (int i = bgmPlayerList.Count - 1; i >= 0; i--) {
            var playingBgmPlayer = bgmPlayerList[i];
            if (playingBgmPlayer != null) {
                playingBgmPlayer.SetVolume(volume, fadeTime);
            }
        }
    }
}
