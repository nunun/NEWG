using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

// プレイヤーコライダーの実装
public class PlayerCollider : MonoBehaviour {
    //-------------------------------------------------------------------------- 変数
    public Player player     = null; // プレイヤーの設定
    public float  damageBuff = 1.0f; // ヒット時のダメージ倍率
}
