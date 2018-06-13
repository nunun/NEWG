var url                       = require('url');
var util                      = require('util');
var config                    = require('./services/library/config');
var logger                    = require('./services/library/logger');
var models                    = require('./services/protocols/models');
var couchClient               = require('./services/library/couch_client').activate(config.couchClient, logger.couchClient);
var redisClient               = require('./services/library/redis_client').activate(config.redisClient, logger.redisClient);
var mindlinkClient            = require('./services/library/mindlink_client').activate(config.mindlinkClient, logger.mindlinkClient);
var matchingServer            = require('./services/library/websocket_server').activate(config.matchingServer, logger.matchingServer);
var TaskQueue                 = require('./task_queue');
var UserData                  = models.UserData;
var MatchingData              = models.MatchingData;
var MatchConnectData          = models.MatchConnectData;
var MatchingServerStatusData  = models.MatchingServerStatusData;
var ServerSetupRequestMessage = models.ServerSetupRequestMessage;
var statusData                = new MatchingServerStatusData();

// couch client
couchClient.setConnectEventListener(function() {
    redisClient.start();
});

// redis client
redisClient.setConnectEventListener(function() {
    mindlinkClient.start();
});

// mindlink client
mindlinkClient.setConnectEventListener(function() {
    // NOTE
    // マッチングサーバステータス設定
    statusData.matchingServerState = 'standby';
    statusData.matchingServerUrl   = 'ws://' + config.matchingServer.fqdn
                                   +     ':' + config.matchingServer.port;

    // マッチングサーバステータス送信
    mindlinkClient.sendStatus(statusData, function(err) {
        if (err) {
            logger.mindlinkClient.error(err.toString());
            process.exit(1);
            return;
        }
        logger.mindlinkClient.info('mindlink client initialized.');
        logger.mindlinkClient.info('starting matching server ...');
        matchingServer.start();

        // NOTE
        // マッチングブレイン供給を開始
        startMatchingBrainSupply();
    });
});
mindlinkClient.setDisconnectEventListener(function() {
    // NOTE
    // マッチングブレイン供給を停止
    stopMatchingBrainSupply();
});
mindlinkClient.setDataFromRemoteEventListener(0, (data,res) => {
    var task = setupQueue.getTaskAtKey(data.matchId);
    if (!task) {
        var errorMessage = 'matchId not found? (' + data.matchId + ')';
        logger.matchingServer.debug(errorMessage);
        mindlinkClient.sendToRemote(res.to, 0, {err:new Error(errorMessage)});
        return;
    }
    task.serverSetupDoneMessage = data;
});

// matching server
matchingServer.setStartEventListener(function() {
    // マッチングサーバステータス更新
    statusData.matchingServerState = 'ready';
    mindlinkClient.sendStatus(statusData);
});
matchingServer.setAcceptEventListener(function(req) {
    // クエリから マッチングID を確認, なければ弾く
    var location = url.parse(req.url, true);
    if (!location.query.matchingId) {
        return null;
    }
    // マッチングID を確認
    return {matchingId:location.query.matchingId};
});
matchingServer.setConnectEventListener(function(matchingClient) {
    logger.matchingServer.debug("matchingClient connected.");
    if (joinQueue.isFull()) {
        logger.matchingServer.debug("joinQueue is full. refuse matchingClient.");
        matchingClient.stop();
        return;
    }
    var task = TaskQueue.createTaskFromKey(matchingClient);
    joinQueue.add(task);
});
matchingServer.setDisconnectEventListener(function(matchingClient) {
    logger.matchingServer.debug("matchingClient disconnected.");
    joinQueue.removeKey(matchingClient);
    matchingQueue.removeKey(matchingClient);
    errorQueue.removeKey(matchingClient);
});

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// 参加キュー
// ユーザを識別し、マッチングキューに引継ぎます。
var joinQueue = new TaskQueue(config.joinQueue, logger.joinQueue);
joinQueue.setAddEventListener((task) => {
    joinQueue.logger.debug("task added.");
    return identifyUser;
});
joinQueue.setAbortEventListener((err, task) => {
    joinQueue.logger.debug("task aborted (" + err + ").");
    task.err = err;
    joinQueue.remove(task);
    errorQueue.add(task);
});

// ユーザ識別
async function identifyUser(task) {
    joinQueue.logger.debug("identify user.");

    // 次のキューがフルなら待ち
    if (matchingQueue.isFull()) {
        return 1000;
    }

    // 初期化
    var matchingClient = task._key;
    task.matchingId = matchingClient.acceptData.matchingId;
    task.userId     = null;
    task.userData   = null;

    // マッチングデータ取得
    joinQueue.logger.debug('matchingId = ' + task.matchingId);
    var matchingData = await MatchingData.promiseGetCache(task.matchingId);
    if (!matchingData) {
        throw new Erorr("invalid matchingId");
    }
    task.userId = matchingData.users[0];

    // ユーザ取得
    joinQueue.logger.debug('userId = ' + task.userId);
    var userData = await UserData.promiseGet(matchingClient.userId);
    if (!userData) {
        throw new Error("invalid userId");
    }

    // ユーザ確認完了
    joinQueue.logger.debug('user found.');
    joinQueue.remove(task);
    matchingQueue.add(task);
    return null;
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// マッチングキュー
// マッチング待ちのユーザのキューです。
var matchingQueue = new TaskQueue(config.matchingQueue, logger.matchingQueue);
matchingQueue.setAddEventListener((task) => {
    matchingQueue.logger.debug("task added.");
    return null;
});
matchingQueue.setAbortEventListener((err, task) => {
    joinQueue.logger.debug("task aborted (" + err + ").");
    task.err = err;
    matchingQueue.remove(task);
    errorQueue.add(task);
});

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// マッチングブレインID生成用
var matchingBrainIdCounter = 0;

// マッチID生成用
var matchIdCounter = 0;

// マッチングブレイン供給タイマー
var matchingBrainSupplyTimer = null;

// NOTE
// マッチングブレイン供給開始
// マインドリンクに接続した時点で開始する。
// 定期的にマッチングブレインを供給し続ける。
function startMatchingBrainSupply() {
    if (matchingBrainSupplyTimer) {
        matchingBrainQueue.logger.error("matchingBrainSupplyTimer is already started.");
        return;
    }
    matchingBrainQueue.logger.debug("start matchingBrainSupplyTimer ...");
    matchingBrainSupplyTimer = setInterval(() => {
        if (!matchingBrainQueue.isFull()) {
            matchingBrainQueue.logger.debug("supplying new matchingBrain ...");
            var matchingBrainTask = TaskQueue.createTaskFromKey(++matchingBrainIdCounter);
            matchingBrainQueue.add(matchingBrainTask);
        }
    }, 1000);
}

// NOTE
// マッチングブレイン供給停止
// マインドリンクから切断した時点で停止される。
function stopMatchingBrainSupply() {
    if (!matchingBrainSupplyTimer) {
        matchingBrainQueue.logger.error("matchingBrainSupplyTimer does not started yet.");
        return;
    }
    matchingBrainQueue.logger.debug("stop matchingBrainSupplyTimer ...");
    stopInterval(matchingBrainSupplyTimer);
    matchingBrainQueue.logger.debug("remove all matchingBrain ...");
    var matchingBrainTask = null;
    while (matchingBrainTask = matchingBrainQueue.getTaskAt(0)) {
        matchingBrainQueue.remove(matchingBrainTask);
    }
}

// マッチングブレインキュー
// 参加できそうなサーバに目星を付けた後、
// マッチングキューを監視してユーザのマッチングを行い、
// マッチングしたユーザに結果を通知します。
var matchingBrainQueue = new TaskQueue(config.matchingBrainQueue, logger.matchingBrainQueue);
matchingBrainQueue.setAddEventListener((task) => {
    matchingBrainQueue.logger.debug("task added.");
    return findServer; // NOTE サーバ探しから始める
});
matchingBrainQueue.setAbortEventListener((err, task) => {
    matchingBrainQueue.logger.debug("task aborted (" + err + ").");
    task.err = err;
    matchingBrainQueue.remove(task);
    setupErrorQueue.add(task);
});

// 参加できそうなサーバを探す
function findServer(task) {
    matchingBrainQueue.logger.debug("find server.");
    return new Promise((resolved, reject) => {
        // NOTE
        // 現在は立ち上がっているサーバには入れるようにしておく。
        // 空きサーバに所定のメンバーを入れる場合は、
        // サーバ参加確認と参加応答のプロトコルを組み込む必要がある。
        // これはデータベース経由で実現することも可能なので、どの実装が良いかは今後検討する。
        //
        // NOTE
        // 現在の実装では、マッチングブレインがセットアップキューに送り出されると
        // 次のマッチングブレインが "stanby" サーバを掴むので、マッチングが成立してしまう点に注意。
        var cond = ".*{.alias == \"server\" && (.serverState == \"standby\" || .serverState == \"ready\") && .load < 1.0}";
        mindlinkClient.sendQuery(cond, function(err,services) {
            if (err) {
                resolved(10000);//送信エラー(10秒待ってリトライ)
                return;
            }
            if (!services || services.length <= 0) {
                resolved(3000);//サービス無し(3秒待ってリトライ)
                return;
            }
            task.service = services[0];
            matchingBrainQueue.logger.debug('server found: service[' + util.inspect(task.service, {depth:null,breakLength:Infinity}) + ']');
            resolved(makeMatching);
        });
    });
}

// マッチングする
async function makeMatching(task) {
    matchingBrainQueue.logger.debug("make matching.");

    // 次のキューがフルならしばらく待ち
    if (setupQueue.isFull()) {
        return 1000;
    }
    // マッチングが成立するためのユーザいない
    if (matchingQueue.getLength() <= 0) {
        return 1000;
    }
    var matchingKeys    = [];
    var matchingClients = [];
    var matchingUsers   = [];

    // NOTE
    // 今は上から順にマッチング
    // そのうち上等なマッチングのロジックに治す。
    var matchingTask = matchingQueue.getTaskAt(0);
    matchingKeys.push(matchingTask._key);
    matchingClients.push(matchingTask._key);
    matchingUsers.push(matchingTask.userId);
    matchingQueue.remove(matchingTask);

    // サーバ セットアップ リクエストメッセージ
    var serverSetupRequestMessage = new ServerSetupRequestMessage();
    serverSetupRequestMessage.matchId       = (++matchIdCounter).toString();
    serverSetupRequestMessage.sceneName     = "MapProvingGround"; // TODO
    serverSetupRequestMessage.matchingUsers = matchingUsers;
    matchingKeys.push(serverSetupRequestMessage.matchId);

    // セットアップタスクを作成して投入
    var setupTask = TaskQueue.createTaskFromKey(matchingKeys);
    setupTask.service                   = task.service;
    setupTask.matchingClients           = matchingClients;
    setupTask.serverSetupRequestMessage = serverSetupRequestMessage;
    matchingBrainQueue.remove(task);
    setupQueue.add(setupTask);
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// セットアップキュー
// サーバを起動したりデータベースに書き込んだりした後
// ユーザにマッチング結果を通知します。
var setupQueue = new TaskQueue(config.setupQueue, logger.setupQueue);
setupQueue.setAddEventListener((task) => {
    setupQueue.logger.debug("task added.");
    return setupRequest;
});
setupQueue.setAbortEventListener((err, task) => {
    setupQueue.logger.debug("task aborted (" + err + ").");
    task.err = err;
    setupQueue.remove(task);
    setupErrorQueue.add(task);
});

// セットアップリクエストを飛ばす
async function setupRequest(task) {
    setupQueue.logger.debug("setup request.");
    mindlinkClient.sendToRemote(task.service.clientUuid, 0, task.serverSetupRequestMessage);
    return waitForSetupResponse;
}

// セットアップレスポンス待ち...
async function waitForSetupResponse(task) {
    if (task._count == 0) {
        setupQueue.logger.debug("wait for setup response.");
        task.waitCount = 0;
    }

    // メッセージ来た？
    if (!task.serverSetupDoneMessage) {
        if (task.waitCount++ >= 30) {
            throw new Error('setup wait count exceeded.');
        }
        return 1000;
    }

    // セットアップが完了したらしいので
    // マッチ完了を通知
    var matchConnectData = new MatchConnectData();
    matchConnectData.serverAddress = task.service.serverAddress;
    matchConnectData.serverPort    = task.service.serverPort;
    matchConnectData.matchId       = task.serverSetupDoneMessage.matchId;
    task.matchConnectData = matchConnectData;
    return sendMatchConnectData;
}

// セットアップアイテムに記録された全ユーザにマッチ完了を通知
async function sendMatchConnectData(task) {
    setupQueue.logger.debug("send match connect data.");
    var matchingClients = task.matchingClients;
    for (var i in matchingClients) {
        var matchingClient = matchingClients[i];
        matchingClient.send(0, task.matchConnectData);
    }
    return waitForSetupDisconnect;
}

// 数秒待って全て閉じる
async function waitForSetupDisconnect(task) {
    if (task._count == 0) {
        setupQueue.logger.debug("wait for setup disconnect.");
        return 3000;
    }
    var matchingClients = task.matchingClients;
    for (var i in matchingClients) {
        var matchingClient = matchingClients[i];
        matchingClient.stop();
    }
    setupQueue.remove(task);
    return null;
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// エラーキュー
// エラーを送信して数秒後に切断します。
var errorQueue = new TaskQueue(config.errorQueue, logger.errorQueue);
errorQueue.setAddEventListener((task) => {
    errorQueue.logger.debug("task added.");
    return sendError;
});
errorQueue.setAbortEventListener((err, task) => {
    errorQueue.logger.debug("task aborted (" + err + ").");
    errorQueue.remove(task);
});

// エラー送信
async function sendError(task) {
    errorQueue.logger.debug("send error.");
    var matchingClient = task._key;
    matchingClient.send(1, task.err);
    return waitForErrorDisconnect;
}

// 数秒まってキューから消す
async function waitForErrorDisconnect(task) {
    if (task._count == 0) {
        errorQueue.logger.debug("wait for error disconnect.");
        return 3000;
    }
    var matchingClient = task._key;
    matchingClient.stop();
    errorQueue.remove(task);
    return null;
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// セットアップエラーキュー
// データベースにエラーを書き込んだりした後、
// ユーザにマッチング失敗を通知します。
var setupErrorQueue = new TaskQueue(config.setupErrorQueue, logger.setupErrorQueue);
setupErrorQueue.setAddEventListener((task) => {
    setupErrorQueue.logger.debug("task added.");
    return sendSetupError;
});
setupErrorQueue.setAbortEventListener((err, task) => {
    setupErrorQueue.logger.debug("task aborted (" + err + ").");
    setupErrorQueue.remove(task);
});

// セットアップエラー送信
async function sendSetupError(task) {
    setupErrorQueue.logger.debug("send setup error.");
    var matchingClients = task.matchingClients;
    for (var i in matchingClients) {
        var matchingClient = matchingClients[i];
        matchingClient.send(1, task.err);
    }
    return waitForSetupErrorDisconnect;
}

// 数秒まってキューから消す
async function waitForSetupErrorDisconnect(task) {
    if (task._count == 0) {
        setupErrorQueue.logger.debug("wait for setup error disconnect.");
        return 3000;
    }
    var matchingClients = task.matchingClients;
    for (var i in matchingClients) {
        var matchingClient = matchingClients[i];
        matchingClient.stop();
    }
    setupQueue.remove(task);
    return null;
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// start app ...
couchClient.start();
