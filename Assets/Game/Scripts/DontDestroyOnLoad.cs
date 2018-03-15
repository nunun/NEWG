using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DontDestroyOnLoad : MonoBehaviour {
    //-------------------------------------------------------------------------- 実装 (MonoBehaviour)
    void Awake() {
        while (transform.childCount > 0) {
            var child = transform.GetChild(0);
            child.SetParent(null);
            child.gameObject.SetActive(true);
            DontDestroyOnLoad(child.gameObject);
        }
        GameObject.Destroy(this.gameObject);
    }
}
