using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

// プレイヤー
// プレイヤーの入力や更新を制御します。
public partial class Player : MonoBehaviour {
    //-------------------------------------------------------------------------- 定義
    // ネットワークプレイヤー挙動クラス
    public abstract partial class NetworkPlayerBehaviour : NetworkBehaviour {
        public abstract Player player { get; protected set; }
        protected void Link(Player player) { player.networkPlayer = this as NetworkPlayer; }
    }

    // 最大ヒットポイント
    public static readonly int MAX_HITPOINT = 100;

    //-------------------------------------------------------------------------- 変数
    public Rigidbody   playerRididbody = null;   // このプレイヤーのリジッドボディ
    public Gun         gun             = null;   // 銃の設定
    public GameObject  head            = null;   // 頭の設定
    public GameObject  aimPivot        = null;   // 射線の始点の設定 (右目とか頭とか)
    public GameObject  throwPoint      = null;   // 投擲物発生位置
    public float       throwPower      = 350.0f; // 投擲力
    public float       throwCooldown   = 10.0f;  // 投擲間隔時間
    public GameObject  grenadePrefab   = null;   // グレネードのプレハブ
    public GameObject  explosionPrefab = null;   // 爆発のプレハブ   (死亡時)
    public AudioSource throwAudio      = null;   // 投げの音
    public int         hitPoint        = 100;    // ヒットポイント
    public float       jumpPower       = 190.0f; // ジャンプ力
    public int         killPoint       = 0;      // キル数
    public float       gunDistance     = 0.52f;  // 銃までの距離
    public string      playerId        = "";     // プレイヤーID
    public string      playerName      = "";     // プレイヤー名

    // 各種 UI
    TextBuilder hitPointText  = null; // ヒットポイントテキスト
    Slider      hitPointGauge = null; // ヒットポイントゲージ
    TextBuilder killPointText = null; // キルポイントテキスト
    TextBuilder messageText   = null; // メッセージテキスト
    SceneUI     exitUI        = null; // ExitUI

    // 地面レイヤマスク
    int groundLayerMask = 0;

    // このプレイヤーが使用するネットワークプレイヤー
    NetworkPlayer networkPlayer = null;

    //-------------------------------------------------------------------------- 実装 (NetworkBehaviour)
    void Start() {
        Debug.Assert(gun             != null, "銃の設定なし");
        Debug.Assert(head            != null, "頭の設定なし");
        Debug.Assert(aimPivot        != null, "視線の始点の設定なし");
        Debug.Assert(grenadePrefab   != null, "グレネードプレハブの設定なし");
        Debug.Assert(explosionPrefab != null, "爆発プレハブの設定なし");
        Debug.Assert(throwAudio      != null, "投げの設定なし");

        // 各種UI取得
        hitPointText  = GameObjectTag<TextBuilder>.Find("HitPointText");
        hitPointGauge = GameObjectTag<Slider>.Find("HitPointGauge");
        killPointText = GameObjectTag<TextBuilder>.Find("KillPointText");
        messageText   = GameObjectTag<TextBuilder>.Find("MessageText");
        exitUI        = GameObjectTag<SceneUI>.Find("ExitUI");
        Debug.Assert(hitPointText  != null, "ヒットポイントテキストがシーンにない");
        Debug.Assert(hitPointGauge != null, "ヒットポイントゲージがシーンにない");
        Debug.Assert(killPointText != null, "キルポイントテキストがシーンにない");
        Debug.Assert(messageText   != null, "メッセージテキストがシーンにない");
        Debug.Assert(exitUI        != null, "ExitUI がシーンにない");

        // 初期化
        InitMove();
        InitLook();
        InitFire();
        InitThrow();
        InitAnimation();
        InitHitPoint();
        InitPlayerInfo();

        // 地面レイヤを取得
        var groundLayerNo = LayerMask.NameToLayer("Ground");
        groundLayerMask = 1 << groundLayerNo;

        // NOTE
        // 即座に自分の情報とスポーンをリクエスト
        // プレイヤー情報を送った後にスポーンしてもいいかもしれない。
        if (networkPlayer.isLocalPlayer) {
            SendPlayerInfo();
            Spawn();
        }

        #if SERVER_CODE
        Server.AddPlayer(this);
        #endif
    }

    void OnDestroy() {
        #if SERVER_CODE
        Server.RemovePlayer(this);
        #endif
    }

    void Update() {
        if (networkPlayer.isLocalPlayer) {
            LookLocal();
            FireLocal();
            ThrowLocal();
        } else {
            LookNetwork();
            FireNetwork();
            ThrowNetwork();
        }
        UpdateAnimation();
        UpdateHitPoint();
    }

    void FixedUpdate() {
        if (networkPlayer.isLocalPlayer) {
            MoveLocal();
        } else {
            MoveNetwork();
        }
    }

    void OnAnimatorIK(int layerIndex) {
        UpdateIK();
    }
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// 位置と向きの同期
public partial class Player {
    //-------------------------------------------------------------------------- 定義
    public const float MOVE_SPEED              = 2.5f;  // 移動速度 (m/s)
    public const float SYNC_POSITION_LERP_RATE = 15.0f; // 位置補完レート

    //-------------------------------------------------------------------------- 変数
    float currentMoveSpeed = 0.0f; // 現在の移動速度

    //-------------------------------------------------------------------------- 制御 (移動)
    // 移動の初期化
    void InitMove() {
        currentMoveSpeed = 0.0f;
    }

    // ローカル移動
    void MoveLocal() {
        var deltaTime  = Time.deltaTime;
        var gameCamera = GameCamera.Instance;

        // 移動入力
        var horizontal = GameInputManager.MoveHorizontal;
        var vertical   = GameInputManager.MoveVertical;
        var right      = (gameCamera == null)? Vector3.right   : gameCamera.transform.right;
        var forward    = (gameCamera == null)? Vector3.forward : gameCamera.transform.forward;
        var moveInput  = (right * horizontal) + (forward * vertical);
        moveInput.y = 0.0f; // NOTE 入力から Y 成分を消す
        moveInput = moveInput.normalized;

        // ジャンプ入力
        var jumpInput = GameInputManager.IsJump;

        // 視線を作成する
        var look = forward;
        var aim  = look;
        if (gameCamera != null) {
           var hit = default(RaycastHit);
           if (Physics.Raycast(gameCamera.transform.position + (gameCamera.transform.forward * 6.5f), gameCamera.transform.forward, out hit)) {
               aim = (hit.point - aimPivot.transform.position).normalized;
           }
        }

        // 移動速度
        var move = moveInput * MOVE_SPEED;
        currentMoveSpeed = (move.sqrMagnitude >= 0.0001f)? MOVE_SPEED : 0.0f;

        // 位置を更新
        var position  = transform.position + (move * deltaTime);
        playerRididbody.MovePosition(position);

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
        transform.position = position;
    }

    //-------------------------------------------------------------------------- 制御 (視線)
    // 視線の初期化
    void InitLook() {
        // NOTE
        // 今のところ特になし
    }

    // ローカル視線
    void LookLocal() {
        var gameCamera = GameCamera.Instance;

        // 前方を取得
        var forward = (gameCamera == null)? Vector3.forward : gameCamera.transform.forward;

        // 視線を作成する
        var look = forward;
        var aim  = look;
        if (gameCamera != null) {
            var hit = default(RaycastHit);
            if (Physics.Raycast(gameCamera.transform.position + (gameCamera.transform.forward * 6.5f), gameCamera.transform.forward, out hit)) {
                aim = (hit.point - aimPivot.transform.position).normalized;
            }
        }

        // 向き調整
        var direction = (new Vector3(forward.x, 0.0f, forward.z)).normalized;
        transform.rotation = Quaternion.LookRotation(direction);

        // 銃の位置と向きを調整
        var gunPosition  = aimPivot.transform.position + (aim * gunDistance);
        var gunDirection = aim;
        gun.transform.position = gunPosition;
        gun.transform.rotation = Quaternion.LookRotation(gunDirection);

        // NOTE
        // 同期送信は MoveLocal で行う
    }

    // ネットワーク視線
    void LookNetwork() {
        // 向きを更新
        var direction = (new Vector3(networkPlayer.syncAim.x, 0.0f, networkPlayer.syncAim.z)).normalized;
        transform.rotation = Quaternion.LookRotation(direction);

        // 銃の位置と向きを調整
        var aim = networkPlayer.syncAim;
        var gunPosition  = aimPivot.transform.position + (aim * gunDistance);
        var gunDirection = aim;
        gun.transform.position = gunPosition;
        gun.transform.rotation = Quaternion.LookRotation(gunDirection);
    }

    //------------------------------------------------------------------------- 同期
    public partial class NetworkPlayerBehaviour {
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

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// 発砲の同期
public partial class Player {
    //-------------------------------------------------------------------------- 制御
    bool isFire = false; // 現在発射中かどうか

    //-------------------------------------------------------------------------- 制御
    // 銃の初期化
    void InitFire() {
        isFire = false;
    }

    // ローカル発砲
    void FireLocal() {
        var isFireInput = GameInputManager.IsFire;
        if (isFireInput) {
            gun.Fire(this);
        }

        // NOTE
        // 状態が変わったら同期
        if (isFire != isFireInput) {
            isFire = isFireInput;
            networkPlayer.SyncFire(isFire);
            gun.MuzzleFlash(isFire);
        }
    }

    // ネットワーク発砲
    void FireNetwork() {
        isFire = networkPlayer.syncFire;
        if (isFire) {
            gun.Fire(this);
            gun.MuzzleFlash(isFire);
        }
    }

    //------------------------------------------------------------------------- 同期
    public partial class NetworkPlayerBehaviour {
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

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// 投擲の同期
public partial class Player {
    //-------------------------------------------------------------------------- 制御
    float throwInterval = 0.0f; // 投擲間隔

    //-------------------------------------------------------------------------- 制御
    // 投擲の初期化
    void InitThrow() {
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
        var isThrow = GameInputManager.IsThrow;
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
            if (throwAudio != null) {
                throwAudio.Play();
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
        if (throwAudio != null) {
            throwAudio.Play();
        }
    }

    //------------------------------------------------------------------------- 同期
    public partial class NetworkPlayerBehaviour {
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

        // 投げを受信
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

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// アニメーション
public partial class Player {
    //-------------------------------------------------------------------------- 定義
    public const float MOVE_ANIMATION_SPEED = 0.8f; // アニメーション速度倍率 (勘)

    //-------------------------------------------------------------------------- 変数
    Animator animator = null;

    //-------------------------------------------------------------------------- 制御
    void InitAnimation() {
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

    void UpdateIK() {
        if (!animator) {
           return;
        }
        animator.SetLookAtWeight(1.0f, 0.0f, 1.0f, 0.0f, 0f);
        animator.SetLookAtPosition(gun.transform.position);
        animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1.0f);
        animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1.0f);
        animator.SetIKPosition(AvatarIKGoal.LeftHand, gun.leftHandle.transform.position);
        animator.SetIKRotation(AvatarIKGoal.LeftHand, gun.leftHandle.transform.rotation);
        animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1.0f);
        animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1.0f);
        animator.SetIKPosition(AvatarIKGoal.RightHand, gun.rightHandle.transform.position);
        animator.SetIKRotation(AvatarIKGoal.RightHand, gun.rightHandle.transform.rotation);
    }
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// ヒットポイント
public partial class Player {
    //-------------------------------------------------------------------------- 制御
    void InitHitPoint() {
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
            hitPointText.Begin(hitPoint).Apply();
            hitPointGauge.value = Mathf.Clamp(((float)hitPoint / MAX_HITPOINT), 0.0f, 1.0f);
            hitPointGauge.fillRect.gameObject.SetActive(hitPoint > 0); // NOTE 最後の 1 ミリが消えないのでワークアラウンド。後々スライダー側に処理を移す。
        }
    }

    //------------------------------------------------------------------------- 同期
    public partial class NetworkPlayerBehaviour {
        // [C 1-> S] ダメージを発行
        public void SyncDealDamage(NetworkInstanceId targetNetId, int damage, NetworkInstanceId shooterNetId) {
            Debug.Assert(this.isLocalPlayer, "ローカル限定です");
            CmdSyncDealDamage(targetNetId, damage, shooterNetId);
        }

        // [S ->* C] 残り HP をバラマキ
        [Command]
        public void CmdSyncDealDamage(NetworkInstanceId targetNetId, int damage, NetworkInstanceId shooterNetId) {
            Debug.Assert(this.isServer, "サーバ限定です");
            if (!NetworkServer.Instance.server.IsGameStarted) {
                return; // NOTE ゲーム開始していなければダメージにならない
            }
            var networkPlayer = NetworkPlayer.FindByNetId(targetNetId);
            if (networkPlayer != null) {
                networkPlayer.syncHitPoint -= damage;
                if (networkPlayer.syncHitPoint <= 0) {
                    networkPlayer.RpcDeath(shooterNetId);
                }

                // NOTE
                // サーバ側で SyncVar フック呼び出し
                if (this.isServer) {
                    networkPlayer.OnSyncHitPoint(networkPlayer.syncHitPoint);
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

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// 出発 (カウントダウン後のゲーム開始)
public partial class Player {
    //-------------------------------------------------------------------------- 制御
    public void OnDeparture(Vector3 position, Quaternion rotation) {
        // NOTE
        // 今の所処理なし
    }

    //-------------------------------------------------------------------------- 同期
    public partial class NetworkPlayerBehaviour {
        // 出発を受信
        [ClientRpc]
        public void RpcDeparture(Vector3 position, Quaternion rotation) {
            var playerRididbody = player.playerRididbody;
            playerRididbody.velocity        = new Vector3(0f,0f,0f);
            playerRididbody.angularVelocity = new Vector3(0f,0f,0f);
            playerRididbody.useGravity      = true;
            player.transform.position = position;
            player.transform.rotation = rotation;
            syncPosition = position;
            syncAim      = Vector3.forward;
            player.OnDeparture(position, rotation);
        }
    }
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// スポーン
public partial class Player {
    //-------------------------------------------------------------------------- 制御
    public void Spawn() {
        enabled = false;
        networkPlayer.SyncSpawn();
    }

    public void OnSpawn(Vector3 position, Quaternion rotation) {
        // NOTE
        // スポーンしたらカメラをバトルモードに変える
        if (networkPlayer.isLocalPlayer) {
            // TODO
            GameCamera.Instance.SetBattleMode(this);
        }
    }

    //-------------------------------------------------------------------------- 同期
    public partial class NetworkPlayerBehaviour {
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

        // スポーンを受信
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
            player.enabled   = true;
            player.OnSpawn(position, rotation);
        }
    }
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// 死
public partial class Player {
    //-------------------------------------------------------------------------- 変数
    bool isDead = false; // 死亡フラグ

    // 死亡フラグの取得
    public bool IsDead { get { return isDead; }}

    //-------------------------------------------------------------------------- 制御
    void OnDeath(NetworkInstanceId shooterNetId) {
        GameObject.Instantiate(explosionPrefab, transform.position, transform.rotation);

        // NOTE
        // 死亡フラグを立ててどこかに飛ばす
        // リスポーンする場合はこのあたりに処理を追加する
        this.isDead = true;
        this.transform.position = new Vector3(0.0f, -1000.0f, 0.0f);
        NetworkPlayer.SetDirtyAliveCount();

        // 得点追加
        var shooterNetworkPlayer = NetworkPlayer.FindByNetId(shooterNetId);
        if (shooterNetworkPlayer != null) {
            var shooterPlayer = shooterNetworkPlayer.player;
            if (shooterPlayer != null) {
                shooterPlayer.killPoint++;

                // キルポイント表示変更
                if (shooterNetworkPlayer.isLocalPlayer) {
                    killPointText.Begin(shooterPlayer.killPoint).Apply();
                }

                // 倒されたメッセージ
                if (this.networkPlayer.isLocalPlayer) {
                    messageText.Begin(shooterPlayer.playerName).Append(" に倒されました").Apply();
                }

                // 倒したメッセージ
                if (shooterNetworkPlayer.isLocalPlayer) {
                    messageText.Begin(this.playerName).Append(" を倒しました").Apply().For(3.0f);
                }
            }
        }

        // 自分がやられたら結果表示して終わり
        if (networkPlayer.isLocalPlayer) {
            GameCamera.Instance.SetResultMode();
            exitUI.Open();
        }
    }

    //-------------------------------------------------------------------------- 同期
    public partial class NetworkPlayerBehaviour {
        // [S ->* C] 死を宣告
        [ClientRpc]
        public void RpcDeath(NetworkInstanceId shooterNetId) {
            if (player != null) {
                player.OnDeath(shooterNetId);
            }
        }
    }

    //-------------------------------------------------------------------------- 生存者数ユーティリティ
    public partial class NetworkPlayerBehaviour {
        static int  playerCountCache       = 0;    // プレイヤー数キャッシュ
        static int  aliveCountCache        = 0;    // 生存者数キャッシュ
        static bool isDirtyAliveCountCache = true; // 生存者数キャッシュに変更あり

        public static int GetAliveCount() {
            if (!isDirtyAliveCountCache) {
                if (playerCountCache != NetworkPlayer.Instances.Count) {
                    playerCountCache = NetworkPlayer.Instances.Count;
                    isDirtyAliveCountCache = true;
                }
            }
            if (isDirtyAliveCountCache) {
                var a = 0;
                var l = NetworkPlayer.Instances.Count;
                for (int i = 0; i < l; i++) {
                    var networkPlayer = NetworkPlayer.Instances[i];
                    if (!networkPlayer.player.isDead) {
                        a++;
                    }
                }
                aliveCountCache = a;
            }
            return aliveCountCache;
        }

        public static void SetDirtyAliveCount() {
            isDirtyAliveCountCache = true;
        }
    }
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// プレイヤー情報の宣言と同期
public partial class Player {
    //-------------------------------------------------------------------------- 制御
    void InitPlayerInfo() {
        if (networkPlayer.isLocalPlayer) {
            playerId   = GameDataManager.PlayerData.playerId;
            playerName = GameDataManager.PlayerData.playerName;
        } else {
            // NOTE
            // クライアント接続時に SyncVar フック呼び出し
            networkPlayer.OnSyncPlayerId(networkPlayer.syncPlayerId);
            networkPlayer.OnSyncPlayerName(networkPlayer.syncPlayerName);
        }
    }

    void SendPlayerInfo() {
        Debug.Assert(networkPlayer.isLocalPlayer, "ローカル限定です");
        Debug.LogFormat("プレイヤー情報送信 (playerId = {0}, playerName = {1})", playerId, playerName);
        networkPlayer.SyncPlayerInfo(playerId, playerName);
    }

    void OnSyncPlayerId(string playerId) {
        this.playerId = playerId;
    }

    void OnSyncPlayerName(string playerName) {
        this.playerName = playerName;
        if (this.networkPlayer.isLocalPlayer) {
            messageText.Begin("あなた がゲームに参加しました").Apply().For(3.0f);
        } else {
            messageText.Begin(playerName).Append(" がゲームに参加しました").Apply().For(3.0f);
        }
    }

    //-------------------------------------------------------------------------- 同期
    public partial class NetworkPlayerBehaviour {
        // [C 1-> S] プレイヤー情報を同期
        public void SyncPlayerInfo(string playerId, string playerName) {
            Debug.Assert(this.isLocalPlayer, "ローカル限定です");
            CmdSyncPlayerInfo(playerId, playerName);
        }

        // [S ->* C] プレイヤー情報をばらまき
        [Command]
        public void CmdSyncPlayerInfo(string playerId, string playerName) {
            Debug.Assert(this.isServer, "サーバ限定です");
            Debug.LogFormat("プレイヤー情報受信 (playerId = {0}, playerName = {1})", playerId, playerName);
            syncPlayerId   = playerId;
            syncPlayerName = playerName;

            // NOTE
            // サーバ側で SyncVar フック呼び出し
            if (this.isServer) {
                OnSyncPlayerId(syncPlayerId);
                OnSyncPlayerName(syncPlayerName);
            }
        }

        // プレイヤー ID を受信
        public void OnSyncPlayerId(string value) {
            Debug.Assert(player != null, "プレイヤーなし");
            Debug.LogFormat("プレイヤーID受信 ({0})", value);
            player.OnSyncPlayerId(value);
        }

        // プレイヤー名を受信
        public void OnSyncPlayerName(string value) {
            Debug.Assert(player != null, "プレイヤーなし");
            Debug.LogFormat("プレイヤー名受信 ({0})", value);
            player.OnSyncPlayerName(value);
        }

        [HideInInspector, SyncVar(hook="OnSyncPlayerId")]   public string syncPlayerId   = null;
        [HideInInspector, SyncVar(hook="OnSyncPlayerName")] public string syncPlayerName = null;
    }
}
