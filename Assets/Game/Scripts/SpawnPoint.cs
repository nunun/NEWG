using System;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour {
    //-------------------------------------------------------------------------- 変数
    static List<SpawnPoint> spawnPoints = new List<SpawnPoint>();

    //-------------------------------------------------------------------------- 実装 (MonoBehaviour)
    void Awake() {
        spawnPoints.Add(this);
    }

    void OnDestroy() {
        spawnPoints.Remove(this);
    }

    //-------------------------------------------------------------------------- 操作
    public static void GetRandomSpawnPoint(out Vector3 position, out Quaternion rotation) {
        position = Vector3.zero;
        rotation = Quaternion.identity;
        if (spawnPoints.Count > 0) {
            var index = UnityEngine.Random.Range(0, spawnPoints.Count);
            var point = spawnPoints[index];
            position = point.transform.position;
            rotation = point.transform.rotation;
        }
    }
}
