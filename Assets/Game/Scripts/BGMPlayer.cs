using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using System.Collections.Generic;
using System.Text;

// BGM プレイヤー
public class BGMPlayer : MonoBehaviour {
    //---------------------------------------------------------------------- 変数
    [SerializeField, Range(0.0f, 1.0f)]
    public float volumeBase = 1.0f;

    AudioSource audioSource = null;
    bool        isPlaying   = false;
    float       volume      = 0.0f;
    float       volumeFrom  = 0.0f;
    float       volumeTo    = 0.0f;
    float       currentTime = 0.0f;
    float       fadeTime    = 0.0f;

    //---------------------------------------------------------------------- 操作
    public void Play(float fadeTime = 0.5f) {
        if (isPlaying) {
            return;//既に再生中ならやらない
        }
        this.enabled            = true;
        this.isPlaying          = true;
        //this.volume           = 0.0f;
        this.volumeFrom         = this.volume;
        this.volumeTo           = 1.0f;
        this.currentTime        = 0.0f;
        this.fadeTime           = fadeTime;
        this.audioSource.volume = 0.0f;
        this.audioSource.Play();
    }

    public void Stop(float fadeTime = 0.5f) {
        if (!isPlaying) {
            return;//既に停止中ならやらない
        }
        this.enabled            = true;
        this.isPlaying          = false;
        //this.volume           = 0.0f;
        this.volumeFrom         = this.volume;
        this.volumeTo           = 0.0f;
        this.currentTime        = 0.0f;
        this.fadeTime           = fadeTime;
        this.audioSource.volume = 1.0f;
    }

    public void SetVolume(float volume, float fadeTime = 0.5f) {
        this.enabled     = true;
        //this.volume    = 0.0f;
        this.volumeFrom  = this.volume;
        this.volumeTo    = volume;
        this.currentTime = 0.0f;
        this.fadeTime    = fadeTime;
    }

    //---------------------------------------------------------------------- 実装 (MonoBehaviour)
    void Awake() {
        audioSource = GetComponent<AudioSource>();
        Debug.Assert(audioSource != null, "オーディオソースなし");
        isPlaying          = false;
        volume             = 0.0f;
        volumeFrom         = 0.0f;
        volumeTo           = 0.0f;
        fadeTime           = 0.0f;
        currentTime        = 0.0f;
        fadeTime           = 0.0f;
        enabled            = false;
        audioSource.volume = 0.0f;
        audioSource.Stop();
    }

    void Update() {
        currentTime += Time.deltaTime;
        if (currentTime < fadeTime) {
            this.volume = this.volumeFrom + ((this.volumeTo - this.volumeFrom) * (this.currentTime / this.fadeTime));
            audioSource.volume = this.volume * volumeBase;
        } else {
            this.volume = this.volumeTo;
            audioSource.volume = this.volume * volumeBase;
            enabled = false;
        }
    }
}
