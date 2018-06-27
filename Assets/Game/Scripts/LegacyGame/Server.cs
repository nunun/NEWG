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
        protected void Link(Server server) { server.networkServer = this as NetworkServer; }
    }

    //-------------------------------------------------------------------------- 変数
    TextBuilder aliveCountText = null; // 生存者数テキスト
    TextBuilder gameText       = null; // ゲームテキスト
    SceneUI     exitUI         = null; // 退出 UI

    // このプレイヤーが使用するネットワークプレイヤー
    NetworkServer networkServer = null;

    //-------------------------------------------------------------------------- 実装 (NetworkBehaviour)
    void Start() {
        // 初期化
        if (networkServer.isServer) {
            #if SERVER_CODE
            InitReservation();
            InitPlayers();
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
            UpdatePlayers();
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
    // ゲーム終了からシャットダウンまでの秒数
    public static readonly float SHUTDOWN_TIME = 30.0f;

    //-------------------------------------------------------------------------- 変数
    GameProgress gameProgress                  = GameProgress.Waiting; // ゲーム進捗
    int          updateGameProgressCount       = 0;                    // ゲーム進捗更新カウント (共通)
    int          updateGameProgressServerCount = 0;                    // ゲーム進捗更新カウント (サーバ)
    float        countdownTime                 = COUNTDOWN_TIME;       // 開始カウントダウン時間
    float        countdownSyncTime             = COUNTDOWN_SYNC_TIME;  // 開始カウントダウン同期時間
    float        shutdownTime                  = SHUTDOWN_TIME;        // ゲーム終了からシャットダウンまでの時間

    public bool IsGameStarted { get { return gameProgress == GameProgress.Started; }}

    //-------------------------------------------------------------------------- 制御
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
            if (NetworkPlayer.GetAliveCount() >= START_COUNTDOWN_RESERVED_COUNT) {
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
                ChangeGameProgress(GameProgress.Starting);
            }
            break;
        case GameProgress.Starting:
            var sequenceIndex = 0;
            foreach (var networkPlayer in NetworkPlayer.Instances) {
                var position = Vector3.zero;
                var rotation = Quaternion.identity;
                SpawnPoint.GetRandomSpawnPoint(out position, out rotation, sequenceIndex++);
                networkPlayer.RpcDeparture(position, rotation);
            }
            ChangeGameProgress(GameProgress.Started);
            break;
        case GameProgress.Started:
            // NOTE
            // 現在は最後の一人になるまでだが
            // チーム戦だとそうとは限らないのでいずれ直す
            if (NetworkPlayer.GetAliveCount() <= 1) {
                ChangeGameProgress(GameProgress.End);
            }
            break;
        case GameProgress.End:
            // TODO
            // 結果を記録
            ChangeGameProgress(GameProgress.Ended);
            break;
        case GameProgress.Ended:
            if (updateGameProgressServerCount == 1) {
                shutdownTime = SHUTDOWN_TIME;
            }
            shutdownTime -= Time.deltaTime;
            if (shutdownTime <= 0.0f) {
                GameManager.Quit();
            }
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
            aliveCountText.Begin(NetworkPlayer.GetAliveCount()).Apply();
            break;
        case GameProgress.Countdown:
            if (updateGameProgressCount == 1) {
                exitUI.Close();
            }
            countdownTime = Mathf.Max(0.0f, countdownTime - Time.deltaTime);
            gameText.Begin("試合を開始します ... ").Append((int)countdownTime).Apply();
            aliveCountText.Begin(NetworkPlayer.GetAliveCount()).Apply();
            break;
        case GameProgress.Starting:
        case GameProgress.Started:
        case GameProgress.End:
            if (updateGameProgressCount == 1) {
                gameText.Begin("").Apply();
                exitUI.Close();
            }
            aliveCountText.Begin(NetworkPlayer.GetAliveCount()).Apply();
            break;
        case GameProgress.Ended:
            if (updateGameProgressCount == 1) {
                gameText.Begin("試合終了！").Apply();
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
#if SERVER_CODE

// プレイヤー情報と同期処理
public partial class Server {
    //-------------------------------------------------------------------------- 定義
    // 不信なユーザ情報
    public class UntrustedPlayerInfo {
        public Player player = null;
        public float  timer  = 0.0f;
    }

    // 不信なユーザをキックする時間 [秒]
    public static readonly float UNTRUSTED_KICK_TIME = 5.0f;

    // プレイヤーを場外判定する深さ
    public static readonly float OUTOFAREA_Y = -2.0f;

    //-------------------------------------------------------------------------- 変数
    static List<UntrustedPlayerInfo> untrustedPlayerList = new List<UntrustedPlayerInfo>();

    //-------------------------------------------------------------------------- プレイヤーの追加と削除
    public static void AddPlayer(Player player) {
        var untrustedPlayerInfo = new UntrustedPlayerInfo();
        untrustedPlayerInfo.player = player;
        untrustedPlayerInfo.timer  = UNTRUSTED_KICK_TIME;
        untrustedPlayerList.Add(untrustedPlayerInfo);
    }

    public static void RemovePlayer(Player player) {
        for (int i = untrustedPlayerList.Count - 1; i >= 0; i--) {
            var untrustedPlayerInfo = untrustedPlayerList[i];
            if (untrustedPlayerInfo.player == player) {
                untrustedPlayerList.RemoveAt(i);
            }
        }
    }

    //-------------------------------------------------------------------------- 制御
    void InitPlayers() {
        // NOTE
        // 特に処理なし
    }

    void UpdatePlayers() {
        UpdateUntrustedPlayerInfo();
        UpdateOutOfArea();
    }

    //-------------------------------------------------------------------------- 更新 (不信なプレイヤー)
    void UpdateUntrustedPlayerInfo() {
        var time = Time.deltaTime;
        for (int i = untrustedPlayerList.Count - 1; i >= 0; i--) {
            var untrustedPlayerInfo = untrustedPlayerList[i];
            if (untrustedPlayerInfo.player == null) {
                untrustedPlayerList.RemoveAt(i);
                continue;
            }
            var playerId = untrustedPlayerInfo.player.playerId;
            if (!string.IsNullOrEmpty(playerId)) {
                Debug.LogFormat("信用 ({0})", playerId);
                untrustedPlayerList.RemoveAt(i);
                continue;
            }
            untrustedPlayerInfo.timer -= time;
            if (untrustedPlayerInfo.timer <= 0.0f) {
                untrustedPlayerList.RemoveAt(i);
                var networkPlayer = NetworkPlayer.FindByPlayer(untrustedPlayerInfo.player);
                if (networkPlayer != null) {
                    networkPlayer.Kick(); // NOTE 不信なユーザは切断！
                }
                continue;
            }
        }
    }

    //-------------------------------------------------------------------------- 更新 (場外)
    void UpdateOutOfArea() {
        for (var i = NetworkPlayer.Instances.Count - 1; i >= 0; i--) {
            var networkPlayer = NetworkPlayer.Instances[i];
            var player        = networkPlayer.player;
            if (player == null) {
                continue;
            }
            if (!player.IsDead && player.transform.position.y <= OUTOFAREA_Y) {
                var position = Vector3.zero;
                var rotation = Quaternion.identity;
                SpawnPoint.GetRandomSpawnPoint(out position, out rotation);
                networkPlayer.RpcDeparture(position, rotation);
            }
        }
    }
}

#endif
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
#if SERVER_CODE

// 予約の受付
public partial class Server {
    //-------------------------------------------------------------------------- 定義
    // 最大予約数
    public static readonly int MAX_RESERERVED_COUNT = 10;

    //-------------------------------------------------------------------------- 定義
    int reservedCount = 0; // 予約数

    //-------------------------------------------------------------------------- 制御
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
#endif
