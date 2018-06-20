using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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
    TextBuilder aliveCountText = null; // 生存者数テキスト
    TextBuilder gameText       = null; // ゲームテキスト
    SceneUI     exitUI         = null; // 退出 UI

    // このプレイヤーが使用するネットワークプレイヤー
    NetworkServer networkServer = null;

    //-------------------------------------------------------------------------- 実装 (NetworkBehaviour)
    void Start() {
        // インスタンス取得
        networkServer = NetworkServer.Instance;

        // 初期化
        InitGameProgress();
        InitAliveCount();
        InitReservedCount();
        if (networkServer.isServer) {
            #if SERVER_CODE
            InitReservation();
            #endif
        }

        // 各種UI取得
        aliveCountText = GameObjectTag<TextBuilder>.Find("AliveCountText");
        gameText       = GameObjectTag<TextBuilder>.Find("GameText");
        exitUI         = GameObjectTag<SceneUI>.Find("ExitUI");
        Debug.Assert(aliveCountText != null, "生存者数テキストがシーンにいない");
        Debug.Assert(gameText       != null, "ゲームテキストがシーンにいない");
        Debug.Assert(exitUI         != null, "ExitUI がシーンにいない");
    }

    void OnDestroy() {
        if (networkServer == null) {
            return;
        }
        if (networkServer.isServer) {
            #if SERVER_CODE
            DestroyReservation();
            #endif
        }
    }

    void Update() {
        if (networkServer.isServer) {
            #if SERVER_CODE
            UpdateGameProgressServer();
            #endif
        }
        UpdateGameProgress();
    }
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// ゲームプログレス同期
public partial class Server {
    //-------------------------------------------------------------------------- 定義
    // ゲーム進行状態
    public enum GameProgress {
        Waiting   = 0, // プレイヤー参加待ち
        Countdown = 1, // カウントダウン中
        Starting  = 2, // ゲーム開始中
        Started   = 3, // ゲーム開始済
        End       = 4, // ゲーム終了
        Ended     = 5, // ゲーム終了済
    };

    // カウントダウンを開始するプレイヤー数
    public static readonly int START_COUNTDOWN_RESERVED_COUNT = 2;

    // カウントダウン秒数
    public static readonly float COUNTDOWN_TIME = 15.0f;

    // カウントダウン同期秒数
    public static readonly float COUNTDOWN_SYNC_TIME = 3.0f;

    //-------------------------------------------------------------------------- 変数
    GameProgress gameProgress                  = GameProgress.Waiting; // ゲーム進捗
    int          updateGameProgressCount       = 0;                    // ゲーム進捗更新カウント (共通)
    int          updateGameProgressServerCount = 0;                    // ゲーム進捗更新カウント (サーバ)
    float        countdownTime                 = COUNTDOWN_TIME;       // 開始カウントダウン時間
    float        countdownSyncTime             = COUNTDOWN_SYNC_TIME;  // 開始カウントダウン同期時間

    public bool IsGameStarted { get { return gameProgress == GameProgress.Started; }}

    //-------------------------------------------------------------------------- 初期化と更新
    void InitGameProgress() {
        gameProgress      = GameProgress.Waiting;
        countdownTime     = COUNTDOWN_TIME;
        countdownSyncTime = COUNTDOWN_SYNC_TIME;
        networkServer.InitGameProgress();
        networkServer.InitCountdownTime();
    }

    void UpdateGameProgressServer() {
        updateGameProgressServerCount++;
        switch (gameProgress) {
        case GameProgress.Waiting:
            if (reservedCount >= START_COUNTDOWN_RESERVED_COUNT) {
                ChangeGameProgress(GameProgress.Countdown);
            }
            break;
        case GameProgress.Countdown:
            if (updateGameProgressServerCount == 1) {
                countdownSyncTime = COUNTDOWN_SYNC_TIME;
            }
            countdownSyncTime = Mathf.Max(0.0f, countdownSyncTime - Time.deltaTime);
            if (countdownSyncTime <= 0.0f) {
                Debug.LogFormat("時刻同期 ({0}) ...", countdownTime);
                networkServer.SyncCountdownTime(countdownTime);
                countdownSyncTime = COUNTDOWN_SYNC_TIME;
            }
            if (countdownTime <= 0.0f) {
                SetAliveCount(GetReservedCount()); // TODO 現在の予約数を生存者数に設定 (要検討)
                ChangeGameProgress(GameProgress.Starting);
            }
            break;
        case GameProgress.Starting:
            foreach (var networkPlayer in NetworkPlayer.Instances) {
                networkPlayer.RpcDeparture(new Vector3(0.0f, 100.0f, 0.0f), Quaternion.identity); // NOTE プレイヤーを飛ばす。どこに飛ばすかは仮。
            }
            ChangeGameProgress(GameProgress.Started);
            break;
        case GameProgress.Started:
            if (aliveCount <= 1) { // NOTE 最後は一人とは限らないのでいずれ直す
                ChangeGameProgress(GameProgress.End);
            }
            break;
        case GameProgress.End:
            // TODO
            // 結果を記録
            ChangeGameProgress(GameProgress.Ended);
            break;
        case GameProgress.Ended:
            break;
        default:
            Debug.LogErrorFormat("Unknown GameProgress ({0})", gameProgress);
            break;
        }
    }

    void UpdateGameProgress() {
        updateGameProgressCount++;
        switch (gameProgress) {
        case GameProgress.Waiting:
            if (updateGameProgressCount == 1) {
                exitUI.Open();
            }
            gameText.Begin("プレイヤーを待っています ... ").Apply();
            aliveCountText.Begin(reservedCount).Apply();
            break;
        case GameProgress.Countdown:
            if (updateGameProgressCount == 1) {
                exitUI.Close();
            }
            countdownTime = Mathf.Max(0.0f, countdownTime - Time.deltaTime);
            gameText.Begin("ゲームを開始します ... ").Append((int)countdownTime).Apply();
            aliveCountText.Begin(reservedCount).Apply();
            break;
        case GameProgress.Starting:
        case GameProgress.Started:
        case GameProgress.End:
            if (updateGameProgressCount == 1) {
                gameText.Begin("").Apply();
                exitUI.Close();
            }
            aliveCountText.Begin(aliveCount).Apply();
            break;
        case GameProgress.Ended:
            if (updateGameProgressCount == 1) {
                gameText.Begin("ゲーム終了！").Apply();
                exitUI.Open();
            }
            break;
        default:
            break;
        }
    }

    //-------------------------------------------------------------------------- ゲーム進行状態の変更
    void ChangeGameProgress(GameProgress gameProgress) {
        Debug.Assert(networkServer.isServer, "サーバ限定です");
        Debug.LogFormat("Server: ゲーム進捗変更 ({0})", gameProgress);
        networkServer.SyncGameProgress(gameProgress);
        OnChangeGameProgress(gameProgress); // NOTE サーバは即座に受け取り
    }

    void OnChangeGameProgress(GameProgress gameProgress) {
        Debug.LogFormat("Server: サーバ進捗が変更された ({0})", gameProgress);
        this.gameProgress                  = gameProgress;
        this.updateGameProgressServerCount = 0;
        this.updateGameProgressCount       = 0;
    }

    //-------------------------------------------------------------------------- カウントダウンタイマ
    void OnCountdownTime(float countdownTime) {
        this.countdownTime = countdownTime;
    }

    //-------------------------------------------------------------------------- 同期 (ゲーム進捗)
    public partial class NetworkServerBehaviour {
        // 初期化
        public void InitGameProgress() {
            OnSyncGameProgress(syncGameProgress);
        }

        // [S -> C] ゲーム進捗をバラマキ
        public void SyncGameProgress(Server.GameProgress gameProgress) {
            Debug.Assert(this.isServer, "サーバ限定です");
            syncGameProgress = (int)gameProgress;
        }

        // ゲーム進捗を受信
        public void OnSyncGameProgress(int value) {
            if (!this.isServer) { // NOTE クライアントのみ処理。サーバ自身は受け取らない。
                if (server != null) {
                    server.OnChangeGameProgress((Server.GameProgress)value);
                }
            }
        }

        [HideInInspector, SyncVar(hook="OnSyncGameProgress")] public int syncGameProgress = (int)Server.GameProgress.Waiting;
    }

    //-------------------------------------------------------------------------- 同期 (カウントダウン)
    public partial class NetworkServerBehaviour {
        // 初期化
        public void InitCountdownTime() {
            OnSyncCountdownTime(syncCountdownTime);
        }

        // [S -> C] カウントダウン時間をバラマキ
        public void SyncCountdownTime(float countdownTime) {
            Debug.Assert(this.isServer, "サーバ限定です");
            syncCountdownTime = countdownTime;
        }

        // カウントダウン時間を受信
        public void OnSyncCountdownTime(float value) {
            if (!this.isServer) { // NOTE クライアントのみ処理。サーバ自身は受け取らない。
                if (server != null) {
                    server.OnCountdownTime(value);
                }
            }
        }

        [HideInInspector, SyncVar(hook="OnSyncCountdownTime")] public float syncCountdownTime = Server.COUNTDOWN_TIME;
    }
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// 生存者
public partial class Server {
    //-------------------------------------------------------------------------- 変数
    int aliveCount = 0; // 生存者数

    //-------------------------------------------------------------------------- 初期化と操作
    void InitAliveCount() {
        aliveCount = 0;
        networkServer.InitAliveCount();
    }

    void SetAliveCount(int aliveCount) {
        Debug.Assert(networkServer.isServer, "サーバ限定です");
        this.aliveCount = aliveCount;
        networkServer.SyncAliveCount(this.aliveCount);
        OnAliveCount(this.aliveCount);
    }

    public void InformDeath(Player player) {
        // NOTE
        // 今はなんでもデクリメント。
        // ちゃんとプレイヤーのチェックを入れる。
        Debug.Assert(networkServer.isServer, "サーバ限定です");
        this.aliveCount = Mathf.Max(0, this.aliveCount - 1);
        networkServer.SyncAliveCount(this.aliveCount);
        OnAliveCount(this.aliveCount);
    }

    void OnAliveCount(int aliveCount) {
        Debug.LogFormat("Server: 生存者数 ({0})", aliveCount);
        this.aliveCount = aliveCount;
    }

    //-------------------------------------------------------------------------- 同期 (生存者数)
    public partial class NetworkServerBehaviour {
        // 初期化
        public void InitAliveCount() {
            OnSyncAliveCount(syncAliveCount);
        }

        // [S -> C] 生存者数をバラマキ
        public void SyncAliveCount(int aliveCount) {
            Debug.Assert(this.isServer, "サーバ限定です");
            syncAliveCount = aliveCount;
        }

        // 生存者数を受信
        public void OnSyncAliveCount(int value) {
            if (!this.isServer) { // NOTE クライアントのみ処理。サーバ自身は受け取らない。
                if (server != null) {
                    server.OnAliveCount(value);
                }
            }
        }

        [HideInInspector, SyncVar(hook="OnSyncAliveCount")] public int syncAliveCount = 0;
    }
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// 予約数の同期
public partial class Server {
    //-------------------------------------------------------------------------- 変数
    int reservedCount = 0; // 予約数

    //-------------------------------------------------------------------------- 初期化と操作
    void InitReservedCount() {
        networkServer.InitReservedCount();
    }

    void AddReservedCount(int reserveCount) {
        Debug.Assert(networkServer.isServer, "サーバ限定です");
        reservedCount += reserveCount;
        networkServer.SyncReservedCount(reservedCount);
    }

    int GetReservedCount() {
        Debug.Assert(networkServer.isServer, "サーバ限定です");
        return reservedCount;
    }

    void OnReservedCount(int reservedCount) {
        Debug.LogFormat("Server: 予約数 ({0})", reservedCount);
        this.reservedCount = reservedCount;
    }

    //-------------------------------------------------------------------------- 同期 (生存者数)
    public partial class NetworkServerBehaviour {
        // 初期化
        public void InitReservedCount() {
            OnSyncReservedCount(syncReservedCount);
        }

        // [S -> C] 予約数をバラマキ
        public void SyncReservedCount(int reservedCount) {
            Debug.Assert(this.isServer, "サーバ限定です");
            syncReservedCount = reservedCount;
        }

        // 予約数を受信
        public void OnSyncReservedCount(int value) {
            if (!this.isServer) { // NOTE クライアントのみ処理。サーバ自身は受け取らない。
                if (server != null) {
                    server.OnReservedCount(value);
                }
            }
        }

        [HideInInspector, SyncVar(hook="OnSyncReservedCount")] public int syncReservedCount = 0;
    }
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
#if SERVER_CODE

// 予約の受付
public partial class Server {
    //-------------------------------------------------------------------------- 定義
    // 最大予約数
    public static readonly int MAX_RESERERVED_COUNT = 30;

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

        // ゲーム開始しているので
        if (gameProgress == GameProgress.Started) {
            next("already started.");
            yield break;
        }

        // 人数オーバー？
        var reserveCount = users.Length;
        if ((GetReservedCount() + reserveCount) >= MAX_RESERERVED_COUNT) {
            next("server full");
            yield break;
        }
        AddReservedCount(reserveCount);

        // ユーザ予約成功！
        Debug.LogFormat("Server: 予約成功 ({0})", reservedCount);
        next(null);
        yield break;
    }
}
#endif
