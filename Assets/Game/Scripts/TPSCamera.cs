using System;
using UnityEngine;

public class TPSCamera : MonoBehaviour {
    //-------------------------------------------------------------------------- 変数
    public GameObject target      = null;
    public float      orbitSize   = 0.5f;
    public float      height      = 0.5f;
    public float      distance    = 1.0f;
    public float      rotateSpeed = 10.0f;
    public float      minVertical = -15.0f;
    public float      maxVertical = 85.0f;

    Vector3 euler = Vector3.zero;

    //-------------------------------------------------------------------------- 実装 (MonoBehaviour)
    void LateUpdate() {
        // ターゲットなし?
        if (target == null) {
            return;
        }

        var horizontal = GameInputManager.LookHorizontal * rotateSpeed;
        var vertical   = GameInputManager.LookVertical   * -rotateSpeed;

        var x = Mathf.Clamp(euler.x + vertical, minVertical, maxVertical);
        var y = (euler.y + horizontal) % 360.0f;
        euler = new Vector3(x, y, 0.0f);

        transform.position = target.transform.position;
        transform.rotation = Quaternion.identity;

        // Y 軸の回転
        transform.Rotate(0.0f, euler.y, 0.0f);

        // オービット
        transform.position = transform.position + (transform.right * orbitSize) + new Vector3(0.0f, height, 0.0f);
        var position = transform.position;

        // X 軸の回転
        transform.Rotate(euler.x, 0.0f, 0.0f);

        // 引き
        var hit = default(RaycastHit);
        if (Physics.Raycast(transform.position, -transform.forward, out hit, distance)) {
            transform.position = hit.point;
        } else {
            transform.position = transform.position - (transform.forward * distance);
        }

        // 見直す
        transform.LookAt(position);
    }
}
