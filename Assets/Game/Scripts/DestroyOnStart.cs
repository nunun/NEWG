using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyOnStart : MonoBehaviour {
    //-------------------------------------------------------------------------- 変数
    public GameObject targetObject = null;

    //-------------------------------------------------------------------------- 実装 (MonoBehaviour)
    void Start() {
        GameObject.Destroy(targetObject);
    }
}
