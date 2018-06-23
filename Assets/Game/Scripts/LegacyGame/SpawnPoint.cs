using System;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour {
    //-------------------------------------------------------------------------- 変数
    static List<SpawnPoint> spawnPoints = new List<SpawnPoint>();

    static List<SpawnPoint> shuffledSpawnPoints = new List<SpawnPoint>();

    //-------------------------------------------------------------------------- 実装 (MonoBehaviour)
    void Awake() {
        spawnPoints.Add(this);
    }

    void OnDestroy() {
        spawnPoints.Remove(this);
    }

    //-------------------------------------------------------------------------- 操作
    public static void GetRandomSpawnPoint(out Vector3 position, out Quaternion rotation, int sequenceIndex = -1) {
        position = Vector3.zero;
        rotation = Quaternion.identity;
        if (sequenceIndex < 0) {
            if (spawnPoints.Count > 0) {
                var spwanPoint = spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Count)];
                position = spwanPoint.transform.position;
                rotation = spwanPoint.transform.rotation;
            }
        } else {
            if (sequenceIndex == 0) {
                shuffledSpawnPoints.Clear();
            }
            if (shuffledSpawnPoints.Count <= 0) {
                shuffledSpawnPoints.AddRange(spawnPoints);
                for (int i = 0; i < shuffledSpawnPoints.Count; i++) {
                    var swapIndex = UnityEngine.Random.Range(i, spawnPoints.Count);
                    var swapPoint = shuffledSpawnPoints[swapIndex];
                    shuffledSpawnPoints[swapIndex] = shuffledSpawnPoints[i];
                    shuffledSpawnPoints[i]         = swapPoint;
                }
            }
            if (shuffledSpawnPoints.Count > 0) {
                var spwanPoint = shuffledSpawnPoints[0];
                shuffledSpawnPoints.RemoveAt(0);
                position = spwanPoint.transform.position;
                rotation = spwanPoint.transform.rotation;
            }
        }
    }
}
