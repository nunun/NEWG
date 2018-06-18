using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

// サーバ
// ゲームの進行やルールを制御します。
public partial class Server : MonoBehaviour {
    //-------------------------------------------------------------------------- 定義
    // ネットワークサーバ挙動クラス
    public abstract partial class NetworkServerBehaviour : NetworkBehaviour {
        public abstract Server server { get; protected set; }
    }

    //-------------------------------------------------------------------------- 変数
    // このプレイヤーが使用するネットワークプレイヤー
    NetworkServer networkServer = null;

    //-------------------------------------------------------------------------- 実装 (NetworkBehaviour)
    void Start() {
        networkServer = NetworkServer.Instance;
        #if SERVER_CODE
        if (networkServer.isServer) {
            InitReservation();
            StartGameProgress();
        }
        #endif
    }

    void OnDestroy() {
        #if SERVER_CODE
        if (networkServer.isServer) {
            DestroyReservation();
        }
        #endif
    }

    void Update() {
        #if SERVER_CODE
        if (networkServer.isServer) {
            UpdateGameProgress();
        }
        #endif
    }
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// ゲームプログレスの更新
public partial class Server {
    //-------------------------------------------------------------------------- 定義
    // ゲーム進行状態
    public enum GameProgress {
        Waiting  = 0, // プレイヤー参加待ち
        Starting = 1, // カウントダウン中
        Started  = 2, // ゲーム開始済
        End      = 3, // ゲーム終了
        Ended    = 4, // ゲーム終了済
    };

    // カウントダウンを開始するプレイヤー数
    public static readonly int START_COUNTDOWN_RESERVED_COUNT = 1;

    //-------------------------------------------------------------------------- 変数
    GameProgress gameProgress = GameProgress.Waiting; // 待ち中
    //float      startTime    = 0.0f;                 // 開始カウントダウン

    //-------------------------------------------------------------------------- 初期化と更新
    void StartGameProgress() {
        gameProgress = GameProgress.Waiting;
        //startTime  = 0.0f;
    }

    void UpdateGameProgress() {
        // TODO
        switch (gameProgress) {
        case GameProgress.Waiting:
            break;
        case GameProgress.Starting:
            break;
        case GameProgress.Started:
            break;
        case GameProgress.End:
            break;
        case GameProgress.Ended:
            break;
        default:
            Debug.LogErrorFormat("Unknown GameProgress ({0})", gameProgress);
            break;
        }
    }

    //-------------------------------------------------------------------------- ゲーム進行状態の変更
    void ChangeGameProgress(GameProgress gameProgress) {
        Debug.Assert(networkServer.isServer, "サーバ限定です");
        Debug.LogFormat("Server: ゲーム進捗変更 ({0})", gameProgress);
        networkServer.SyncGameProgress(gameProgress);
    }

    void OnChangeGameProgress(GameProgress gameProgress) {
        // TODO
        // UI 切り替え
        Debug.LogFormat("Server: サーバ進捗が変更された ({0})", gameProgress);
    }

    //------------------------------------------------------------------------- 同期
    public partial class NetworkServerBehaviour {
        // [S -> C] ゲーム進捗をバラマキ
        public void SyncGameProgress(Server.GameProgress gameProgress) {
            Debug.Assert(this.isServer, "サーバ限定です");
            syncGameProgress = (int)gameProgress;
        }

        // ゲーム進捗を受信
        public void OnSyncGameProgress(int value) {
            if (server != null) {
                server.OnChangeGameProgress((Server.GameProgress)value);
            }
        }

        [HideInInspector, SyncVar(hook="OnSyncGameProgress")] public int syncGameProgress = (int)Server.GameProgress.Waiting;
    }
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// 予約の受付
public partial class Server {
    //-------------------------------------------------------------------------- 定義
    // 最大予約数
    public static readonly int MAX_RESERERVED_COUNT = 30;

    //-------------------------------------------------------------------------- 変数
    int reservedCount = 0; // 予約数

    //-------------------------------------------------------------------------- 開始と停止と更新
    void InitReservation() {
        Debug.Log("Server: 予約開始");
        GameMindlinkManager.SetReserveRequestMessageHandler(HandleReserve);
    }

    void DestroyReservation() {
        Debug.Log("Server: 予約停止");
        GameMindlinkManager.SetReserveRequestMessageHandler(null);
    }

    //-------------------------------------------------------------------------- 予約のハンドル
    IEnumerator HandleReserve(string[] users, Action<string> next) {
        Debug.LogFormat("Server: 予約 ({0}) ...", users.Length);
        var reserveCount = users.Length;

        // 人数オーバー？
        if ((reservedCount + reserveCount) >= MAX_RESERERVED_COUNT) {
            next("server full");
            yield break;
        }
        reservedCount += reserveCount;

        // ユーザ予約成功！
        next(null);
        yield break;
    }
}

//// サーバ
//public partial class Server : MonoBehaviour {
//    //-------------------------------------------------------------------------- 定義
//    // ネットワークサーバ挙動クラス
//    public abstract partial class NetworkServerBehaviour : NetworkBehaviour {
//        public abstract Server server { get; protected set; }
//    }
//
//    //-------------------------------------------------------------------------- 変数
//    // このプレイヤーが使用するネットワークプレイヤー
//    NetworkPlayer networkPlayer = null;
//
//    //-------------------------------------------------------------------------- 実装 (NetworkBehaviour)
//    void Start() {
//        Debug.Assert(gun             != null, "銃の設定なし");
//        Debug.Assert(head            != null, "頭の設定なし");
//        Debug.Assert(aimPivot        != null, "視線の始点の設定なし");
//        Debug.Assert(explosionPrefab != null, "爆発プレハブの設定なし");
//
//        networkPlayer = NetworkPlayer.FindByPlayer(this);
//
//        InitializeMove();
//        InitializeFire();
//        InitializeThrow();
//        InitializeAnimation();
//        InitializeHitPoint();
//        InitializeIK();
//
//        // 地面レイヤを取得
//        var groundLayerNo = LayerMask.NameToLayer("Ground");
//        groundLayerMask = 1 << groundLayerNo;
//
//        // NOTE
//        // 即座にスポーンをリクエスト
//        if (networkPlayer.isLocalPlayer) {
//            Spawn();
//        }
//    }
//
//    void Update() {
//        if (networkPlayer.isLocalPlayer) {
//            MoveLocal();
//            FireLocal();
//            ThrowLocal();
//        } else {
//            MoveNetwork();
//            FireNetwork();
//            ThrowNetwork();
//        }
//        UpdateAnimation();
//        UpdateHitPoint();
//    }
//
//    void LateUpdate() {
//        UpdateIK();
//    }
//}
//// 位置と向きの同期
//public partial class Player {
//    //-------------------------------------------------------------------------- 定義
//    public const float MOVE_SPEED              = 2.5f;  // 移動速度 (m/s)
//    public const float SYNC_POSITION_LERP_RATE = 15.0f; // 位置補完レート
//
//    //-------------------------------------------------------------------------- 変数
//    float currentMoveSpeed = 0.0f; // 現在の移動速度
//
//    //-------------------------------------------------------------------------- 制御
//    // 移動の初期化
//    void InitializeMove() {
//        currentMoveSpeed = 0.0f;
//    }
//
//    // ローカル移動
//    void MoveLocal() {
//        var deltaTime  = Time.deltaTime;
//        var gameCamera = GameCamera.Instance;
//
//        // 移動入力
//        var horizontal = GameInputManager.MoveHorizontal;
//        var vertical   = GameInputManager.MoveVertical;
//        var right      = (gameCamera == null)? Vector3.right   : gameCamera.transform.right;
//        var forward    = (gameCamera == null)? Vector3.forward : gameCamera.transform.forward;
//        var moveInput  = (right * horizontal) + (forward * vertical);
//        moveInput.y = 0.0f; // NOTE 入力から Y 成分を消す
//        moveInput = moveInput.normalized;
//
//        // ジャンプ入力
//        var jumpInput = GameInputManager.IsJump;
//
//        // 視線を作成する
//        var target = forward;
//        if (gameCamera != null) {
//            var hit = default(RaycastHit);
//            if (Physics.Raycast(gameCamera.transform.position + (gameCamera.transform.forward * 2.5f), gameCamera.transform.forward, out hit)) {
//                target = (hit.point - gun.transform.position).normalized;
//            }
//        }
//
//        // 移動速度
//        var move = moveInput * MOVE_SPEED;
//        currentMoveSpeed = (move.sqrMagnitude >= 0.0001f)? MOVE_SPEED : 0.0f;
//
//        // 位置と向きを更新
//        var position  = transform.position + (move * deltaTime);
//        var direction = (new Vector3(forward.x, 0.0f, forward.z)).normalized;
//        var aim       = target;
//        var look      = forward;
//        transform.position      = position;
//        transform.rotation      = Quaternion.LookRotation(direction);
//        gun.transform.rotation  = Quaternion.LookRotation(aim);
//        head.transform.rotation = Quaternion.LookRotation(look);
//
//        // ジャンプを更新
//        if (jumpInput) {
//            var hit = default(RaycastHit);
//            if (Physics.Raycast(transform.position + Vector3.up, -Vector3.up, out hit, 10.0f, groundLayerMask) && hit.distance < 1.001f) {
//                playerRididbody.AddForce(transform.up * jumpPower);
//            }
//        }
//
//        // NOTE
//        // ローカルの位置と向きを同期
//        networkPlayer.SyncMove(transform.position, aim, look);
//    }
//
//    // ネットワーク移動
//    void MoveNetwork() {
//        var deltaTime = Time.deltaTime;
//
//        // 移動速度
//        var move = transform.position - networkPlayer.syncPosition;
//        currentMoveSpeed = (move.sqrMagnitude >= 0.0001f)? MOVE_SPEED : 0.0f;
//
//        // 位置と向きを更新
//        var position  = Vector3.Lerp(transform.position, networkPlayer.syncPosition, deltaTime * SYNC_POSITION_LERP_RATE);
//        var direction = (new Vector3(networkPlayer.syncAim.x, 0.0f, networkPlayer.syncAim.z)).normalized;
//        var aim       = networkPlayer.syncAim;
//        var look      = networkPlayer.syncLook;
//        transform.position      = position;
//        transform.rotation      = Quaternion.LookRotation(direction);
//        gun.transform.rotation  = Quaternion.LookRotation(aim);
//        head.transform.rotation = Quaternion.LookRotation(look);
//    }
//
//    //------------------------------------------------------------------------- 同期
//    public partial class NetworkPlayerBehaviour {
//        // [C 1-> S] 位置と向きの同期を発行
//        public void SyncMove(Vector3 position, Vector3 aim, Vector3 look) {
//            Debug.Assert(this.isLocalPlayer, "ローカル限定です");
//            CmdSyncMove(position, aim, look);
//        }
//
//        // [S ->* C] 位置と向きをバラマキ
//        [Command]
//        public void CmdSyncMove(Vector3 position, Vector3 aim, Vector3 look) {
//            Debug.Assert(this.isServer, "サーバ限定です");
//            syncPosition = position;
//            syncAim      = aim;
//            syncLook     = look;
//        }
//
//        [HideInInspector, SyncVar] public Vector3 syncPosition = Vector3.zero;
//        [HideInInspector, SyncVar] public Vector3 syncAim      = Vector3.forward;
//        [HideInInspector, SyncVar] public Vector3 syncLook     = Vector3.forward;
//    }
//}
//
//// 発砲の同期
//public partial class Player {
//    //-------------------------------------------------------------------------- 制御
//    bool isFire = false; // 現在発射中かどうか
//
//    //-------------------------------------------------------------------------- 制御
//    // 銃の初期化
//    void InitializeFire() {
//        isFire = false;
//    }
//
//    // ローカル発砲
//    void FireLocal() {
//        var isFireInput = GameInputManager.IsFire;
//        if (isFireInput) {
//            gun.Fire(this);
//        }
//
//        // NOTE
//        // 状態が変わったら同期
//        if (isFire != isFireInput) {
//            isFire = isFireInput;
//            networkPlayer.SyncFire(isFire);
//        }
//    }
//
//    // ネットワーク発砲
//    void FireNetwork() {
//        isFire = networkPlayer.syncFire;
//        if (isFire) {
//            gun.Fire(this);
//        }
//    }
//
//    //------------------------------------------------------------------------- 同期
//    public partial class NetworkPlayerBehaviour {
//        // [C 1-> S] 発砲の同期を発行
//        public void SyncFire(bool isFire) {
//            Debug.Assert(this.isLocalPlayer, "ローカル限定です");
//            CmdSyncFire(isFire);
//        }
//
//        // [S ->* C] 発砲状態をバラマキ
//        [Command]
//        public void CmdSyncFire(bool isFire) {
//            Debug.Assert(this.isServer, "サーバ限定です");
//            syncFire = isFire;
//        }
//
//        [HideInInspector, SyncVar] public bool syncFire = false;
//    }
//}
//
//// 投擲の同期
//public partial class Player {
//    //-------------------------------------------------------------------------- 制御
//    float throwInterval = 0.0f; // 投擲間隔
//
//    //-------------------------------------------------------------------------- 制御
//    // 投擲の初期化
//    void InitializeThrow() {
//        throwInterval = 0.0f;
//    }
//
//    // ローカル投擲
//    void ThrowLocal() {
//        // 投擲間隔待ち
//        if (throwInterval > 0.0f) {
//            var deltaTime = Time.deltaTime;
//            throwInterval -= deltaTime;
//            return;
//        }
//
//        // 投擲入力をチェック
//        var isThrow = GameInputManager.IsThrow;
//        if (isThrow) {
//            var gameCamera = GameCamera.Instance;
//            var forward = (gameCamera == null)? Vector3.forward : gameCamera.transform.forward;
//            var up      = (gameCamera == null)? Vector3.up      : gameCamera.transform.up;
//            var throwPosition = throwPoint.transform.position;
//            var throwVector   = (forward + up).normalized;
//            var grenadeObject = GameObject.Instantiate(grenadePrefab, throwPosition, Quaternion.LookRotation(throwVector));
//            var grenade = grenadeObject.GetComponent<Grenade>();
//            if (grenade != null) {
//                grenade.thrower = this;
//            }
//            var grenadeRigidbody = grenadeObject.GetComponent<Rigidbody>();
//            if (grenadeRigidbody != null) {
//                grenadeRigidbody.AddForce(throwVector * throwPower);
//            }
//            throwInterval = throwCooldown;
//
//            // NOTE
//            // 投擲を同期
//            networkPlayer.SyncThrow(throwPosition, throwVector);
//        }
//    }
//
//    // ネットワーク投擲
//    void ThrowNetwork() {
//        // NOTE
//        // 今の所処理なし
//        // イベントで処理するので。
//    }
//
//    // ネットワーク投擲
//    void OnThrowNetwork(Vector3 throwPosition, Vector3 throwVector) {
//        var grenadeObject = GameObject.Instantiate(grenadePrefab, throwPosition, Quaternion.LookRotation(throwVector));
//        var grenade = grenadeObject.GetComponent<Grenade>();
//        if (grenade != null) {
//            grenade.thrower = this;
//        }
//        var grenadeRigidbody = grenadeObject.GetComponent<Rigidbody>();
//        if (grenadeRigidbody != null) {
//            grenadeRigidbody.AddForce(throwVector * throwPower);
//        }
//    }
//
//    //------------------------------------------------------------------------- 同期
//    public partial class NetworkPlayerBehaviour {
//        // [C 1-> S] 投擲をリクエスト
//        public void SyncThrow(Vector3 throwPosition, Vector3 throwVector) {
//            Debug.Assert(this.isLocalPlayer, "ローカル限定です");
//            CmdThrow(throwPosition, throwVector, this.netId);
//        }
//
//        // [S ->* C] 投げをばらまき
//        [Command]
//        public void CmdThrow(Vector3 throwPosition, Vector3 throwVector, NetworkInstanceId throwerNetId) {
//            Debug.Assert(this.isServer, "サーバ限定です");
//            RpcThrow(throwPosition, throwVector, throwerNetId);
//        }
//
//        [ClientRpc]
//        public void RpcThrow(Vector3 throwPosition, Vector3 throwVector, NetworkInstanceId throwerNetId) {
//            var networkPlayer = NetworkPlayer.FindByNetId(throwerNetId);
//            if (networkPlayer != null) {
//                if (networkPlayer == NetworkPlayer.Instance) {
//                    return; // NOTE 自分の投げた分はローカルで反映済なので、自分で受け取らない...
//                }
//                var player = networkPlayer.player;
//                if (player != null) {
//                    player.OnThrowNetwork(throwPosition, throwVector);
//                }
//            }
//        }
//    }
//}
//
//// アニメーション
//public partial class Player {
//    //-------------------------------------------------------------------------- 定義
//    public const float MOVE_ANIMATION_SPEED = 0.8f; // アニメーション速度倍率 (勘)
//
//    //-------------------------------------------------------------------------- 変数
//    Animator animator = null;
//
//    //-------------------------------------------------------------------------- 制御
//    void InitializeAnimation() {
//        animator = GetComponent<Animator>();
//        Debug.Assert(animator != null, "アニメーターなし");
//    }
//
//    void UpdateAnimation() {
//        // アニメータに速度を通知 (止まる、歩く、走るの自動切換え)
//        animator.SetFloat("MoveSpeed", currentMoveSpeed);
//
//        // アニメーション速度も変えておく
//        var animationSpeed = (currentMoveSpeed >= 0.0001f)? currentMoveSpeed * MOVE_ANIMATION_SPEED : 1.0f;
//        animator.speed = animationSpeed;
//    }
//}
//
//// ヒットポイント
//public partial class Player {
//    //-------------------------------------------------------------------------- 制御
//    void InitializeHitPoint() {
//        // NOTE
//        // 今のところ処理なし
//    }
//
//    void UpdateHitPoint() {
//        // TODO
//        // 将来的にここで無敵状態とかやる
//    }
//
//    // このプレイヤーがダメージを与える
//    public void DealDamage(Player target, int damage) {
//        if (networkPlayer.isLocalPlayer) {
//            networkPlayer.SyncDealDamage(target.networkPlayer.netId, damage, networkPlayer.netId);
//        }
//    }
//
//    void OnSyncHitPoint(int value) {
//        hitPoint = value;
//        if (networkPlayer.isLocalPlayer) {
//            //GameMain.Instance.battleUI.SetHitPoint(hitPoint); // TODO
//        }
//    }
//
//    //------------------------------------------------------------------------- 同期
//    public partial class NetworkPlayerBehaviour {
//        // [C 1-> S] ダメージを発行
//        public void SyncDealDamage(NetworkInstanceId targetNetId, int damage, NetworkInstanceId shooterNetId) {
//            Debug.Assert(this.isLocalPlayer, "ローカル限定です");
//            CmdSyncDealDamage(targetNetId, damage, shooterNetId);
//        }
//
//        // [S ->* C] 残り HP をバラマキ
//        [Command]
//        public void CmdSyncDealDamage(NetworkInstanceId targetNetId, int damage, NetworkInstanceId shooterNetId) {
//            Debug.Assert(this.isServer, "サーバ限定です");
//            var networkPlayer = NetworkPlayer.FindByNetId(targetNetId);
//            if (networkPlayer != null) {
//                networkPlayer.syncHitPoint -= damage;
//                if (networkPlayer.syncHitPoint <= 0) {
//                    networkPlayer.RpcDeath(shooterNetId);
//                }
//            }
//        }
//
//        // 残り HP を受信
//        public void OnSyncHitPoint(int value) {
//            if (player != null) {
//                player.OnSyncHitPoint(value);
//            }
//        }
//
//        [HideInInspector, SyncVar(hook="OnSyncHitPoint")] public int syncHitPoint = 0;
//    }
//}
//
//// スポーン
//public partial class Player {
//    //-------------------------------------------------------------------------- 制御
//    public void Spawn() {
//        enabled = false;
//        networkPlayer.SyncSpawn();
//    }
//
//    //-------------------------------------------------------------------------- 同期
//    public partial class NetworkPlayerBehaviour {
//        // [C 1-> S] スポーンをリクエスト
//        public void SyncSpawn() {
//            Debug.Assert(this.isLocalPlayer, "ローカル限定です");
//            CmdSpawn();
//        }
//
//        // [S ->* C] スポーンをばらまき
//        [Command]
//        public void CmdSpawn() {
//            Debug.Assert(this.isServer, "サーバ限定です");
//            var position = Vector3.zero;
//            var rotation = Quaternion.identity;
//            SpawnPoint.GetRandomSpawnPoint(out position, out rotation);
//            syncHitPoint = 100; // NOTE サーバヒットポイントをリセット
//            RpcSpawn(position, rotation);
//        }
//
//        [ClientRpc]
//        public void RpcSpawn(Vector3 position, Quaternion rotation) {
//            var playerRididbody = player.playerRididbody;
//            playerRididbody.velocity        = new Vector3(0f,0f,0f);
//            playerRididbody.angularVelocity = new Vector3(0f,0f,0f);
//            playerRididbody.useGravity      = true;
//            player.transform.position = position;
//            player.transform.rotation = rotation;
//            syncPosition = position;
//            syncAim      = Vector3.forward;
//            player.killPoint = 0;
//            player.enabled = true;
//            if (this.isLocalPlayer) {
//                GameCamera.Instance.SetBattleMode(player);
//                //GameMain.Instance.battleUI.SetKillPoint(player.killPoint); // TODO
//            }
//        }
//    }
//}
//// 死
//public partial class Player {
//    //-------------------------------------------------------------------------- 変数
//    // サーバからの死の宣告を受けたとき
//    void OnDeath(NetworkInstanceId shooterNetId) {
//        GameObject.Instantiate(explosionPrefab, transform.position, transform.rotation);
//        this.transform.position = new Vector3(0.0f, -1000.0f, 0.0f);
//
//        // 得点追加
//        var shooterNetworkPlayer = Server.FindByNetId(shooterNetId);
//        if (shooterNetworkPlayer != null) {
//            var player = shooterNetworkPlayer.player;
//            if (player != null) {
//                player.killPoint++;
//
//                // キルポイント表示変更
//                if (shooterNetworkPlayer.isLocalPlayer) {
//                    //GameMain.Instance.battleUI.SetKillPoint(player.killPoint); // TODO
//                }
//            }
//        }
//
//        // 自分がやられたら結果表示
//        if (networkPlayer.isLocalPlayer) {
//            // TODO
//            //var killPoint = 0;
//            //if (networkPlayer.player != null) {
//            //    killPoint = networkPlayer.player.killPoint;
//            //}
//            //GameMain.Instance.StartResult(killPoint); // TODO
//        }
//    }
//
//    //-------------------------------------------------------------------------- 同期
//    public partial class NetworkPlayerBehaviour {
//        // [S ->* C] 死を宣告
//        [ClientRpc]
//        public void RpcDeath(NetworkInstanceId shooterNetId) {
//            if (player != null) {
//                player.OnDeath(shooterNetId);
//            }
//        }
//    }
//}
//float time = 5.0f;
//void UpdateHitPoint() {
//    // TODO
//    // 将来的にここで無敵状態とかやる
//    time -= Time.deltaTime;
//    if (time <= 0) {
//        DealDamage(this, 10);
//    }
//}
////-------------------------------------------------------------------------- スポーン
//// TODO
//// これは Player に移しておく。
//// [C 1-> S] スポーンをリクエスト
//public void Spawn() {
//    Debug.Assert(this.isLocalPlayer, "ローカル限定です");
//    CmdSpawn(); // TODO
//}
//
//// [S ->* C] スポーンをばらまき
//[Command]
//public void CmdSpawn() {
//    Debug.Assert(this.isServer, "サーバ限定です");
//    var position = Vector3.zero;
//    var rotation = Quaternion.identity;
//    SpawnPoint.GetRandomSpawnPoint(out position, out rotation);
//    RpcSpawn(position, rotation);
//}
//
//[ClientRpc]
//public void RpcSpawn(Vector3 position, Quaternion rotation) {
//    player.transform.position = position;
//    player.transform.rotation = rotation;
//    var rigidbody = player.GetComponent<Rigidbody>();
//    rigidbody.velocity        = new Vector3(0f,0f,0f);
//    rigidbody.angularVelocity = new Vector3(0f,0f,0f);
//    if (this.isLocalPlayer) {
//        GameCamera.Instance.SetBattleMode(player);
//    }
//}
