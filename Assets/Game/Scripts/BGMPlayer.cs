using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using System.Collections.Generic;
using System.Text;

// BGM プレイヤー
public class BGMPlayer : MonoBehaviour {
    //---------------------------------------------------------------------- 変数
    AudioSource audioSource = null;
    bool        isPlaying   = false;
    float       currentTime = 0.0f;
    float       fadeTime    = 0.0f;

    //---------------------------------------------------------------------- 操作
    public void Play(float fadeTime = 0.5f) {
        if (isPlaying) {
            return;//既に再生中ならやらない
        }
        this.isPlaying          = true;
        this.currentTime        = 0.0f;
        this.fadeTime           = fadeTime;
        this.enabled            = true;
        this.audioSource.volume = 0.0f;
        this.audioSource.Play();
    }

    public void Stop(float fadeTime = 0.5f) {
        if (!isPlaying) {
            return;//既に停止中ならやらない
        }
        this.isPlaying          = false;
        this.currentTime        = 0.0f;
        this.fadeTime           = fadeTime;
        this.enabled            = true;
        this.audioSource.volume = 1.0f;
    }

    //---------------------------------------------------------------------- 実装 (MonoBehaviour)
    void Awake() {
        audioSource = GetComponent<AudioSource>();
        Debug.Assert(audioSource != null, "オーディオソースなし");
        isPlaying          = false;
        currentTime        = 0.0f;
        fadeTime           = 0.0f;
        enabled            = false;
        audioSource.volume = 0.0f;
        audioSource.Stop();
    }

    void Update() {
        currentTime += Time.deltaTime;
        if (isPlaying) {
            if (currentTime < fadeTime) {
                audioSource.volume = currentTime / fadeTime;
            } else {
                audioSource.volume = 1.0f;
                enabled = false;
            }
        } else {
            if (currentTime < fadeTime) {
                audioSource.volume = 1.0f - (currentTime / fadeTime);
            } else {
                audioSource.volume = 0.0f;
                audioSource.Stop();
                enabled = false;
            }
        }
    }
}
