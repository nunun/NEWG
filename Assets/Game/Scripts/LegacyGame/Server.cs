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
    // このプレイヤーが使用するネットワークプレイヤー
    NetworkServer networkServer = null;

    //-------------------------------------------------------------------------- 実装 (NetworkBehaviour)
    void Start() {
        networkServer = NetworkServer.Instance;
        if (networkServer.isServer) {
            #if SERVER_CODE
            InitReservation();
            InitGameProgress();
            #endif
        }
    }

    void OnDestroy() {
        if (   networkServer != null
            && networkServer.isServer) {
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

    //-------------------------------------------------------------------------- 変数
    GameProgress gameProgress                  = GameProgress.Waiting; // ゲーム進捗
    int          updateGameProgressServerCount = 0;                    // ゲーム進捗更新カウント (サーバ)
    int          updateGameProgressCount       = 0;                    // ゲーム進捗更新カウント (共通)
    float        countdownTime                 = COUNTDOWN_TIME;       // 開始カウントダウン

    public bool IsGameStarted { get { return gameProgress == GameProgress.Started; }}

    //-------------------------------------------------------------------------- 初期化と更新
    void InitGameProgress() {
        gameProgress  = GameProgress.Waiting;
        countdownTime = COUNTDOWN_TIME;
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
                countdownTime = COUNTDOWN_TIME;
            }
            var countdownTimeOld = countdownTime;
            countdownTime = Mathf.Max(0.0f, countdownTime - Time.deltaTime);
            if ((int)Mathf.Floor(countdownTime / 3.0f) != (int)Mathf.Floor(countdownTimeOld / 3.0f)) {
                Debug.LogFormat("時刻同期 ({0}) ...", countdownTime);
                networkServer.SyncCountdown(countdownTime);
            }
            if (countdownTime <= 0.0f) {
                SetAliveCount(reservedCount);
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
        if (updateGameProgressCount == 1) {
            switch (gameProgress) {
            case GameProgress.Waiting:
                {
                    var text = GameObjectTag<Text>.Find("GameText");
                    text.text = "プレイヤーを待っています...";
                }
                break;
            case GameProgress.Countdown:
                {
                    var text = GameObjectTag<Text>.Find("GameText");
                    text.text = "カウントダウン";
                }
                break;
            case GameProgress.Starting:
            case GameProgress.Started:
            case GameProgress.End:
                {
                    var text = GameObjectTag<Text>.Find("GameText");
                    text.text = "";
                }
                break;
            case GameProgress.Ended:
                {
                    var text = GameObjectTag<Text>.Find("GameText");
                    text.text = "ゲーム終了";
                }
                break;
            default:
                break;
            }
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
        // TODO
        // カウントダウンタイムの同期
    }

    //-------------------------------------------------------------------------- 同期 (ゲーム進捗)
    public partial class NetworkServerBehaviour {
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
        // [S -> C] カウントダウン時間をバラマキ
        public void SyncCountdown(float countdownTime) {
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

        // TODO
        // UI 更新
        // 生存者テキスト更新
    }

    //-------------------------------------------------------------------------- 同期 (生存者数)
    public partial class NetworkServerBehaviour {
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

        // ゲーム開始しているので
        if (gameProgress == GameProgress.Started) {
            next("already started.");
            yield break;
        }

        // 人数オーバー？
        var reserveCount = users.Length;
        if ((reservedCount + reserveCount) >= MAX_RESERERVED_COUNT) {
            next("server full");
            yield break;
        }
        reservedCount += reserveCount;

        // ユーザ予約成功！
        Debug.LogFormat("Server: 予約成功 ({0})", reservedCount);
        next(null);
        yield break;
    }
}
