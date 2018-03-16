using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DontDestroyOnLoad : MonoBehaviour {
    //-------------------------------------------------------------------------- 実装 (MonoBehaviour)
    void Awake() {
        while (transform.childCount > 0) {
            var child = transform.GetChild(0);

            // 親を自分から外す
            child.SetParent(null);

            // アクティブに設定
            child.gameObject.SetActive(true);

            // DontDestroyOnLoad を設定
            DontDestroyOnLoad(child.gameObject);
        }

        // 自分自身は消える
        GameObject.Destroy(this.gameObject);
    }
}
