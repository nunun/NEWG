using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

// 結果画面 UI の実装
public class ResultUI : MonoBehaviour {
    //-------------------------------------------------------------------------- 変数
    public Text killPointText = null;

    //-------------------------------------------------------------------------- 実装 (MonoBehaviour)
    void Awake() {
        Debug.Assert(killPointText != null, "キルポイントテキスト未設定");
    }

    //-------------------------------------------------------------------------- 操作
    // キルポイントの表示を変更
    public void SetKillPoint(int killPoint) {
        killPointText.text = killPoint.ToString();
    }
}
