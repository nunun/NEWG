using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

// バトル UI の実装
public class BattleUI : MonoBehaviour {
    //-------------------------------------------------------------------------- 変数
    public Text hitPointText    = null;
    public Text killPointText   = null;
    public Text sensitivityText = null;

    //-------------------------------------------------------------------------- 実装 (MonoBehaviour)
    void Awake() {
        Debug.Assert(hitPointText    != null, "ヒットポイントテキスト未設定");
        Debug.Assert(killPointText   != null, "キルポイントテキスト未設定");
        Debug.Assert(sensitivityText != null, "マウス感度テキスト未設定");
        hitPointText.text    = "";
        sensitivityText.text = "";
    }

    void OnEnable() {
        // NOTE
        // 有効化時に再設定
        // 表示されない場合があるので。
        SetSensitivity(GameInputManager.Sensitivity);
    }

    //-------------------------------------------------------------------------- 操作
    // ヒットポイントの表示を変更
    public void SetHitPoint(int hitPoint) {
        hitPointText.text = hitPoint.ToString();
    }

    // キルポイントの表示を変更
    public void SetKillPoint(int killPoint) {
        killPointText.text = killPoint.ToString();
    }

    // マウス感度の表示を変更
    public void SetSensitivity(int sensitivity) {
        sensitivityText.text = string.Format("Sensitivity: {0}", sensitivity);
    }
}
