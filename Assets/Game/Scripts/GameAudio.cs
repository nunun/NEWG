using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

// ゲームオーディオ
public partial class GameAudio {
    // NOTE
    // パーシャルクラスを参照
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// SE 対応
// 指定した名前の SEPlayer を持つゲームオブジェクトタグを探し出し
// その音声を再生したり停止します。
public partial class GameAudio {
    //-------------------------------------------------------------------------- SE
    public static void Play(string seName, GameObject parent = null, bool forceParent = false) {
        var sePlayer = GameObjectTag<SEPlayer>.Find(seName, parent, forceParent);
        if (sePlayer == null) {
            #if !SERVER_CODE
            Debug.LogErrorFormat("SEなし ({0})", seName);
            #endif
            return;
        }
        sePlayer.Play();
    }

    public static void Play(string seName, ulong delay, GameObject parent = null, bool forceParent = false) {
        var sePlayer = GameObjectTag<SEPlayer>.Find(seName, parent, forceParent);
        if (sePlayer == null) {
            #if !SERVER_CODE
            Debug.LogErrorFormat("SEなし ({0})", seName);
            #endif
            return;
        }
        sePlayer.Play(delay);
    }

    public static void Stop(string seName, GameObject parent = null, bool forceParent = false) {
        var sePlayer = GameObjectTag<SEPlayer>.Find(seName, parent, forceParent);
        if (sePlayer == null) {
            #if !SERVER_CODE
            Debug.LogErrorFormat("SEなし ({0})", seName);
            #endif
            return;
        }
        sePlayer.Stop();
    }
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// BGM 対応
// 指定した名前の BGMPlayer を持つゲームオブジェクトタグを探し出し
// その音声を再生したり停止します。
public partial class GameAudio {
    //-------------------------------------------------------------------------- 変数
    // 再生中の bgm リスト
    static List<BGMPlayer> bgmPlayerList = new List<BGMPlayer>();

    //-------------------------------------------------------------------------- BGM
    public static void PlayBGM(string bgmName, float fadeTime = 0.5f, float volume = 1.0f) {
        var bgmPlayer = GameObjectTag<BGMPlayer>.Find(bgmName);
        if (bgmPlayer == null) {
            #if !SERVER_CODE
            Debug.LogErrorFormat("BGMなし ({0})", bgmName);
            #endif
            return;
        }
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
            bgmPlayer.Play(fadeTime);
        }
    }

    public static void MixBGM(string bgmName, float fadeTime = 0.5f) {
        Debug.Assert(bgmPlayerList.Count > 0, "BGM が再生されていない");
        var bgmPlayer = GameObjectTag<BGMPlayer>.Find(bgmName);
        if (bgmPlayer == null) {
            #if !SERVER_CODE
            Debug.LogErrorFormat("BGMなし ({0})", bgmName);
            #endif
            return;
        }
        var bgmSource    = GameObjectTag<AudioSource>.Find(bgmName);
        var masterPlayer = bgmPlayerList[0];
        var masterSource = masterPlayer.gameObject.GetComponent<AudioSource>();
        bgmSource.time = masterSource.time;
        if (bgmPlayerList.IndexOf(bgmPlayer) < 0) {
            bgmPlayerList.Add(bgmPlayer);
            bgmPlayer.Play(fadeTime);
        }
        UpdatePlayingBGMVolumes(fadeTime);
    }

    public static void StopBGM(string bgmName, float fadeTime = 0.5f) {
        var bgmPlayer = GameObjectTag<BGMPlayer>.Find(bgmName);
        if (bgmPlayer == null) {
            #if !SERVER_CODE
            Debug.LogErrorFormat("BGMなし ({0})", bgmName);
            #endif
            return;
        }
        bgmPlayerList.Remove(bgmPlayer);
        bgmPlayer.Stop(fadeTime);
        UpdatePlayingBGMVolumes(fadeTime);
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

    public static void SetBGMVolume(string bgmName, float volume, float fadeTime = 0.5f) {
        var bgmPlayer = GameObjectTag<BGMPlayer>.Find(bgmName);
        if (bgmPlayer == null) {
            #if !SERVER_CODE
            Debug.LogErrorFormat("BGMなし ({0})", bgmName);
            #endif
            return;
        }
        bgmPlayer.SetVolume(volume, fadeTime);
    }

    //-------------------------------------------------------------------------- 共通ロジック
    static void UpdatePlayingBGMVolumes(float fadeTime = 0.5f) {
        var count  = bgmPlayerList.Count;
        var volume = Mathf.Clamp(1.0f - (0.15f * (count - 1)), 0.70f, 1.0f);
        for (int i = bgmPlayerList.Count - 1; i >= 0; i--) {
            var playingBgmPlayer = bgmPlayerList[i];
            if (playingBgmPlayer != null) {
                playingBgmPlayer.SetVolume(volume, fadeTime);
            }
        }
    }
}
