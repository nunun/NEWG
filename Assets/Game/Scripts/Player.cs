using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

// プレイヤーの実装
public partial class Player : MonoBehaviour {
    //-------------------------------------------------------------------------- 定義
    // ネットワークプレイヤー基本クラス
    public abstract partial class NetworkPlayerBase : NetworkBehaviour {
        public abstract Player player { get; protected set; }
    }

    //-------------------------------------------------------------------------- 変数
    public Gun        gun             = null;   // 銃の設定
    public GameObject head            = null;   // 頭の設定
    public GameObject aimPivot        = null;   // 射線の始点の設定 (右目とか頭とか)
    public GameObject throwPoint      = null;   // 投擲物発生位置
    public float      throwPower      = 350.0f; // 投擲力
    public float      throwCooldown   = 10.0f;  // 投擲間隔時間
    public GameObject grenadePrefab   = null;   // グレネードのプレハブ
    public GameObject explosionPrefab = null;   // 爆発のプレハブ   (死亡時)
    public int        hitPoint        = 100;    // ヒットポイント
    public Rigidbody  playerRididbody = null;   // このキャラのリジッドボディ
    public float      jumpPower       = 190.0f; // ジャンプ力
    public int        killPoint       = 0;      // キル数

    // 地面レイヤマスク
    int groundLayerMask = 0;

    // このプレイヤーが使用するネットワークプレイヤー
    NetworkPlayer networkPlayer = null;

    //-------------------------------------------------------------------------- 実装 (NetworkBehaviour)
    void Start() {
        Debug.Assert(gun             != null, "銃の設定なし");
        Debug.Assert(head            != null, "頭の設定なし");
        Debug.Assert(aimPivot        != null, "視線の始点の設定なし");
        Debug.Assert(explosionPrefab != null, "爆発プレハブの設定なし");

        networkPlayer = NetworkPlayer.FindByPlayer(this);

        InitializeMove();
        InitializeFire();
        InitializeThrow();
        InitializeAnimation();
        InitializeHitPoint();
        InitializeIK();

        // 地面レイヤを取得
        var groundLayerNo = LayerMask.NameToLayer("Ground");
        groundLayerMask = 1 << groundLayerNo;

        // NOTE
        // 即座にスポーンをリクエスト
        if (networkPlayer.isLocalPlayer) {
            Spawn();
        }
    }

    void Update() {
        if (networkPlayer.isLocalPlayer) {
            MoveLocal();
            FireLocal();
            ThrowLocal();
        } else {
            MoveNetwork();
            FireNetwork();
            ThrowNetwork();
        }
        UpdateAnimation();
        UpdateHitPoint();
    }

    void LateUpdate() {
        UpdateIK();
    }
}

// 位置と向きの同期
public partial class Player {
    //-------------------------------------------------------------------------- 定義
    public const float MOVE_SPEED              = 2.5f;  // 移動速度 (m/s)
    public const float SYNC_POSITION_LERP_RATE = 15.0f; // 位置補完レート

    //-------------------------------------------------------------------------- 変数
    float currentMoveSpeed = 0.0f; // 現在の移動速度

    //-------------------------------------------------------------------------- 制御
    // 移動の初期化
    void InitializeMove() {
        currentMoveSpeed = 0.0f;
    }

    // ローカル移動
    void MoveLocal() {
        var deltaTime  = Time.deltaTime;
        var gameCamera = GameCamera.Instance;

        // 移動入力
        var horizontal = GameInput.MoveHorizontal;
        var vertical   = GameInput.MoveVertical;
        var right      = (gameCamera == null)? Vector3.right   : gameCamera.transform.right;
        var forward    = (gameCamera == null)? Vector3.forward : gameCamera.transform.forward;
        var moveInput  = (right * horizontal) + (forward * vertical);
        moveInput.y = 0.0f; // NOTE 入力から Y 成分を消す
        moveInput = moveInput.normalized;

        // ジャンプ入力
        var jumpInput = GameInput.IsJump;

        // 視線を作成する
        var target = forward;
        if (gameCamera != null) {
            var hit = default(RaycastHit);
            if (Physics.Raycast(gameCamera.transform.position + (gameCamera.transform.forward * 2.5f), gameCamera.transform.forward, out hit)) {
                target = (hit.point - gun.transform.position).normalized;
            }
        }

        // 移動速度
        var move = moveInput * MOVE_SPEED;
        currentMoveSpeed = (move.sqrMagnitude >= 0.0001f)? MOVE_SPEED : 0.0f;

        // 位置と向きを更新
        var position  = transform.position + (move * deltaTime);
        var direction = (new Vector3(forward.x, 0.0f, forward.z)).normalized;
        var aim       = target;
        var look      = forward;
        transform.position      = position;
        transform.rotation      = Quaternion.LookRotation(direction);
        gun.transform.rotation  = Quaternion.LookRotation(aim);
        head.transform.rotation = Quaternion.LookRotation(look);

        // ジャンプを更新
        if (jumpInput) {
            var hit = default(RaycastHit);
            if (Physics.Raycast(transform.position + Vector3.up, -Vector3.up, out hit, 10.0f, groundLayerMask) && hit.distance < 1.001f) {
                playerRididbody.AddForce(transform.up * jumpPower);
            }
        }

        // NOTE
        // ローカルの位置と向きを同期
        networkPlayer.SyncMove(transform.position, aim, look);
    }

    // ネットワーク移動
    void MoveNetwork() {
        var deltaTime = Time.deltaTime;

        // 移動速度
        var move = transform.position - networkPlayer.syncPosition;
        currentMoveSpeed = (move.sqrMagnitude >= 0.0001f)? MOVE_SPEED : 0.0f;

        // 位置と向きを更新
        var position  = Vector3.Lerp(transform.position, networkPlayer.syncPosition, deltaTime * SYNC_POSITION_LERP_RATE);
        var direction = (new Vector3(networkPlayer.syncAim.x, 0.0f, networkPlayer.syncAim.z)).normalized;
        var aim       = networkPlayer.syncAim;
        var look      = networkPlayer.syncLook;
        transform.position      = position;
        transform.rotation      = Quaternion.LookRotation(direction);
        gun.transform.rotation  = Quaternion.LookRotation(aim);
        head.transform.rotation = Quaternion.LookRotation(look);
    }

    //------------------------------------------------------------------------- 同期
    public partial class NetworkPlayerBase {
        // [C 1-> S] 位置と向きの同期を発行
        public void SyncMove(Vector3 position, Vector3 aim, Vector3 look) {
            Debug.Assert(this.isLocalPlayer, "ローカル限定です");
            CmdSyncMove(position, aim, look);
        }

        // [S ->* C] 位置と向きをバラマキ
        [Command]
        public void CmdSyncMove(Vector3 position, Vector3 aim, Vector3 look) {
            Debug.Assert(this.isServer, "サーバ限定です");
            syncPosition = position;
            syncAim      = aim;
            syncLook     = look;
        }

        [HideInInspector, SyncVar] public Vector3 syncPosition = Vector3.zero;
        [HideInInspector, SyncVar] public Vector3 syncAim      = Vector3.forward;
        [HideInInspector, SyncVar] public Vector3 syncLook     = Vector3.forward;
    }
}

// 発砲の同期
public partial class Player {
    //-------------------------------------------------------------------------- 制御
    bool isFire = false; // 現在発射中かどうか

    //-------------------------------------------------------------------------- 制御
    // 銃の初期化
    void InitializeFire() {
        isFire = false;
    }

    // ローカル発砲
    void FireLocal() {
        var isFireInput = GameInput.IsFire;
        if (isFireInput) {
            gun.Fire(this);
        }

        // NOTE
        // 状態が変わったら同期
        if (isFire != isFireInput) {
            isFire = isFireInput;
            networkPlayer.SyncFire(isFire);
        }
    }

    // ネットワーク発砲
    void FireNetwork() {
        isFire = networkPlayer.syncFire;
        if (isFire) {
            gun.Fire(this);
        }
    }

    //------------------------------------------------------------------------- 同期
    public partial class NetworkPlayerBase {
        // [C 1-> S] 発砲の同期を発行
        public void SyncFire(bool isFire) {
            Debug.Assert(this.isLocalPlayer, "ローカル限定です");
            CmdSyncFire(isFire);
        }

        // [S ->* C] 発砲状態をバラマキ
        [Command]
        public void CmdSyncFire(bool isFire) {
            Debug.Assert(this.isServer, "サーバ限定です");
            syncFire = isFire;
        }

        [HideInInspector, SyncVar] public bool syncFire = false;
    }
}

// 投擲の同期
public partial class Player {
    //-------------------------------------------------------------------------- 制御
    float throwInterval = 0.0f; // 投擲間隔

    //-------------------------------------------------------------------------- 制御
    // 投擲の初期化
    void InitializeThrow() {
        throwInterval = 0.0f;
    }

    // ローカル投擲
    void ThrowLocal() {
        // 投擲間隔待ち
        if (throwInterval > 0.0f) {
            var deltaTime = Time.deltaTime;
            throwInterval -= deltaTime;
            return;
        }

        // 投擲入力をチェック
        var isThrow = GameInput.IsThrow;
        if (isThrow) {
            var gameCamera = GameCamera.Instance;
            var forward = (gameCamera == null)? Vector3.forward : gameCamera.transform.forward;
            var up      = (gameCamera == null)? Vector3.up      : gameCamera.transform.up;
            var throwPosition = throwPoint.transform.position;
            var throwVector   = (forward + up).normalized;
            var grenadeObject = GameObject.Instantiate(grenadePrefab, throwPosition, Quaternion.LookRotation(throwVector));
            var grenade = grenadeObject.GetComponent<Grenade>();
            if (grenade != null) {
                grenade.thrower = this;
            }
            var grenadeRigidbody = grenadeObject.GetComponent<Rigidbody>();
            if (grenadeRigidbody != null) {
                grenadeRigidbody.AddForce(throwVector * throwPower);
            }
            throwInterval = throwCooldown;

            // NOTE
            // 投擲を同期
            networkPlayer.SyncThrow(throwPosition, throwVector);
        }
    }

    // ネットワーク投擲
    void ThrowNetwork() {
        // NOTE
        // 今の所処理なし
        // イベントで処理するので。
    }

    // ネットワーク投擲
    void OnThrowNetwork(Vector3 throwPosition, Vector3 throwVector) {
        var grenadeObject = GameObject.Instantiate(grenadePrefab, throwPosition, Quaternion.LookRotation(throwVector));
        var grenade = grenadeObject.GetComponent<Grenade>();
        if (grenade != null) {
            grenade.thrower = this;
        }
        var grenadeRigidbody = grenadeObject.GetComponent<Rigidbody>();
        if (grenadeRigidbody != null) {
            grenadeRigidbody.AddForce(throwVector * throwPower);
        }
    }

    //------------------------------------------------------------------------- 同期
    public partial class NetworkPlayerBase {
        // [C 1-> S] 投擲をリクエスト
        public void SyncThrow(Vector3 throwPosition, Vector3 throwVector) {
            Debug.Assert(this.isLocalPlayer, "ローカル限定です");
            CmdThrow(throwPosition, throwVector, this.netId);
        }

        // [S ->* C] 投げをばらまき
        [Command]
        public void CmdThrow(Vector3 throwPosition, Vector3 throwVector, NetworkInstanceId throwerNetId) {
            Debug.Assert(this.isServer, "サーバ限定です");
            RpcThrow(throwPosition, throwVector, throwerNetId);
        }

        [ClientRpc]
        public void RpcThrow(Vector3 throwPosition, Vector3 throwVector, NetworkInstanceId throwerNetId) {
            var networkPlayer = NetworkPlayer.FindByNetId(throwerNetId);
            if (networkPlayer != null) {
                if (networkPlayer == NetworkPlayer.Instance) {
                    return; // NOTE 自分の投げた分はローカルで反映済なので、自分で受け取らない...
                }
                var player = networkPlayer.player;
                if (player != null) {
                    player.OnThrowNetwork(throwPosition, throwVector);
                }
            }
        }
    }
}

// アニメーション
public partial class Player {
    //-------------------------------------------------------------------------- 定義
    public const float MOVE_ANIMATION_SPEED = 0.8f; // アニメーション速度倍率 (勘)

    //-------------------------------------------------------------------------- 変数
    Animator animator = null;

    //-------------------------------------------------------------------------- 制御
    void InitializeAnimation() {
        animator = GetComponent<Animator>();
        Debug.Assert(animator != null, "アニメーターなし");
    }

    void UpdateAnimation() {
        // アニメータに速度を通知 (止まる、歩く、走るの自動切換え)
        animator.SetFloat("MoveSpeed", currentMoveSpeed);

        // アニメーション速度も変えておく
        var animationSpeed = (currentMoveSpeed >= 0.0001f)? currentMoveSpeed * MOVE_ANIMATION_SPEED : 1.0f;
        animator.speed = animationSpeed;
    }
}

// ヒットポイント
public partial class Player {
    //-------------------------------------------------------------------------- 制御
    void InitializeHitPoint() {
        // NOTE
        // 今のところ処理なし
    }

    void UpdateHitPoint() {
        // TODO
        // 将来的にここで無敵状態とかやる
    }

    // このプレイヤーがダメージを与える
    public void DealDamage(Player target, int damage) {
        if (networkPlayer.isLocalPlayer) {
            networkPlayer.SyncDealDamage(target.networkPlayer.netId, damage, networkPlayer.netId);
        }
    }

    void OnSyncHitPoint(int value) {
        hitPoint = value;
        if (networkPlayer.isLocalPlayer) {
            GameMain.Instance.battleUI.SetHitPoint(hitPoint);
        }
    }

    //------------------------------------------------------------------------- 同期
    public partial class NetworkPlayerBase {
        // [C 1-> S] ダメージを発行
        public void SyncDealDamage(NetworkInstanceId targetNetId, int damage, NetworkInstanceId shooterNetId) {
            Debug.Assert(this.isLocalPlayer, "ローカル限定です");
            CmdSyncDealDamage(targetNetId, damage, shooterNetId);
        }

        // [S ->* C] 残り HP をバラマキ
        [Command]
        public void CmdSyncDealDamage(NetworkInstanceId targetNetId, int damage, NetworkInstanceId shooterNetId) {
            Debug.Assert(this.isServer, "サーバ限定です");
            var networkPlayer = NetworkPlayer.FindByNetId(targetNetId);
            if (networkPlayer != null) {
                networkPlayer.syncHitPoint -= damage;
                if (networkPlayer.syncHitPoint <= 0) {
                    networkPlayer.RpcDeath(shooterNetId);
                }
            }
        }

        // 残り HP を受信
        public void OnSyncHitPoint(int value) {
            if (player != null) {
                player.OnSyncHitPoint(value);
            }
        }

        [HideInInspector, SyncVar(hook="OnSyncHitPoint")] public int syncHitPoint = 0;
    }
}

// スポーン
public partial class Player {
    //-------------------------------------------------------------------------- 制御
    public void Spawn() {
        enabled = false;
        networkPlayer.SyncSpawn();
    }

    //-------------------------------------------------------------------------- 同期
    public partial class NetworkPlayerBase {
        // [C 1-> S] スポーンをリクエスト
        public void SyncSpawn() {
            Debug.Assert(this.isLocalPlayer, "ローカル限定です");
            CmdSpawn();
        }

        // [S ->* C] スポーンをばらまき
        [Command]
        public void CmdSpawn() {
            Debug.Assert(this.isServer, "サーバ限定です");
            var position = Vector3.zero;
            var rotation = Quaternion.identity;
            SpawnPoint.GetRandomSpawnPoint(out position, out rotation);
            syncHitPoint = 100; // NOTE サーバヒットポイントをリセット
            RpcSpawn(position, rotation);
        }

        [ClientRpc]
        public void RpcSpawn(Vector3 position, Quaternion rotation) {
            var playerRididbody = player.playerRididbody;
            playerRididbody.velocity        = new Vector3(0f,0f,0f);
            playerRididbody.angularVelocity = new Vector3(0f,0f,0f);
            playerRididbody.useGravity      = true;
            player.transform.position = position;
            player.transform.rotation = rotation;
            syncPosition = position;
            syncAim      = Vector3.forward;
            player.killPoint = 0;
            player.enabled = true;
            if (this.isLocalPlayer) {
                GameCamera.Instance.SetBattleMode(player);
                GameMain.Instance.battleUI.SetKillPoint(player.killPoint);
            }
        }
    }
}

// 死
public partial class Player {
    //-------------------------------------------------------------------------- 変数
    // サーバからの死の宣告を受けたとき
    void OnDeath(NetworkInstanceId shooterNetId) {
        GameObject.Instantiate(explosionPrefab, transform.position, transform.rotation);
        this.transform.position = new Vector3(0.0f, -1000.0f, 0.0f);

        // 得点追加
        var shooterNetworkPlayer = NetworkPlayer.FindByNetId(shooterNetId);
        if (shooterNetworkPlayer != null) {
            var player = shooterNetworkPlayer.player;
            if (player != null) {
                player.killPoint++;

                // キルポイント表示変更
                if (shooterNetworkPlayer.isLocalPlayer) {
                    GameMain.Instance.battleUI.SetKillPoint(player.killPoint);
                }
            }
        }

        // 自分がやられたら結果表示
        if (networkPlayer.isLocalPlayer) {
            var killPoint = 0;
            if (networkPlayer.player != null) {
                killPoint = networkPlayer.player.killPoint;
            }
            GameMain.Instance.StartResult(killPoint);
        }
    }

    //-------------------------------------------------------------------------- 同期
    public partial class NetworkPlayerBase {
        // [S ->* C] 死を宣告
        [ClientRpc]
        public void RpcDeath(NetworkInstanceId shooterNetId) {
            if (player != null) {
                player.OnDeath(shooterNetId);
            }
        }
    }
}

// IK
public partial class Player {
    //-------------------------------------------------------------------------- 制御
    void InitializeIK() {
        // NOTE
        // 今のところ処理なし
    }

    void UpdateIK() {
        // NOTE
        // 将来的にここで IK する
        //gun.transform.rotation  = Quaternion.LookRotation(currentAim);
        //head.transform.rotation = Quaternion.LookRotation(currentLook);
    }
}

//float time = 5.0f;
//void UpdateHitPoint() {
//    // TODO
//    // 将来的にここで無敵状態とかやる
//    time -= Time.deltaTime;
//    if (time <= 0) {
//        DealDamage(this, 10);
//    }
//}
