using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using System.Collections.Generic;
using System.Text;

// SE プレイヤー
public class SEPlayer : MonoBehaviour {
    //---------------------------------------------------------------------- 変数
    AudioSource audioSource = null;

    //---------------------------------------------------------------------- 操作
    public void Play() {
        audioSource.Play();
    }

    public void Play(ulong delay) {
        audioSource.Play(delay);
    }

    public void Stop() {
        audioSource.Stop();
    }

    //---------------------------------------------------------------------- 実装 (MonoBehaviour)
    void Awake() {
        audioSource = GetComponent<AudioSource>();
        Debug.Assert(audioSource != null, "オーディオソースなし");
    }
}
