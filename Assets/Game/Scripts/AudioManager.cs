using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

// 音声マネージャ
public class AudioManager : MonoBehaviour {
    // TODO
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// BGM の対応
public class AudioManager {
    //-------------------------------------------------------------------------- 変数
    List<BGMPlayer> bgmPlayerList = new List<BGMPlayer>();

    //-------------------------------------------------------------------------- BGM
    public static void PlayBGM(string bgmName, float fadeTime = 0.5f) {
        StopBGM(fadeTime);
        var bgmPlayer = GameObjectTag<BGMPlayer>(bgmName);
        bgmPlayer.Play(fadeTime);
        if (bgmPlayerList.IndexOf(bgmPlayer) <= 0) {
            bgmPlayerList.Add(bgmPlayer);
        }
    }

    public static void MixBGM(string bgmName, string masterBgmName, float fadeTime = 0.5f) {
        var masterSource = GameObjectTag<AudioSource>(masterBgmName);
        var bgmSource    = GameObjectTag<AudioSource>(bgmName);
        var bgmPlayer    = GameObjectTag<BGMPlayer>(bgmName);
        bgmSource.time = masterSource.time;
        bgmPlayer.Play(fadeTime);
        if (bgmPlayerList.IndexOf(bgmPlayer) <= 0) {
            bgmPlayerList.Add(bgmPlayer);
        }
    }

    public static void StopBGM(float fadeTime = 0.5f) {
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
        var bgmPlayer = GameObjectTag<BGMPlayer>(bgmName);
        bgmPlayer.Stop(fadeTime);
        bgmPlayerList.Remove(bgmPlayer);
    }
}
