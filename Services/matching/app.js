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
    });
});
mindlinkClient.setDataFromRemoteEventListener(0, (data,res) => {
    var task = setupQueue.indexOfKey(data.matchId);
    if (!task) {
        var errorMessage = 'matchId not found? (' + matchId + ')';
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
    task.matchingId = task.key.acceptData.matchingId;
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

// マッチングID生成用
var matchIdCounter = 0;

// マッチングブレインキュー
// マッチングキューを監視して、
// ユーザにマッチング結果を通知します。
var matchingBrainQueue = new TaskQueue(config.matchingBrainQueue, logger.matchingBrainQueue);
matchingBrainQueue.setAddEventListener((task) => {
    matchingBrainQueue.logger.debug("task added.");
    return findServer;
});
matchingBrainQueue.setAbortEventListener((err, task) => {
    matchingBrainQueue.logger.debug("task aborted (" + err + ").");
    task.err = err;
    matchingBrainQueue.remove(task);
    setupErrorQueue.add(task);
});

// TODO
// 一時中断中
// NOTE
// マッチングブレイン強制注入
// 足りなくなったら一定時間おきに注入する。
//setTimeout(() => {
//    if (!matchingBrainQueue.isFull()) {
//        var matchingBrainTask = TaskQueue.createTaskFromKey(++matchingBrainIdCounter);
//        matchingBrainQueue.add(matchingBrainTask);
//    }
//}, 1000);

// NOTE
// 参加できそうなサービスを探す
// あらかじめセットアップリクエストを飛ばす実装にしておいても良いかも？
function findServer(task) {
    matchingBrainQueue.logger.debug("find server.");
    return new Promise((resolved) => {
        var cond = ".*{.alias == \"server\" && .serverState == \"standby\" && .load < 1.0}";
        mindlinkClient.sendQuery(cond, function(err,services) {
            if (err) {
                throw err;
            }
            if (!services || services.length <= 0) {
                resolved(1000);//サービスが無し
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
    var matchingTask = matchingQueue.at(0);
    matchingKeys.push(matchingTask.key);
    matchingClients.push(matchingTask.key);
    matchingUsers.push(matchingTask.userId);
    matchingQueue.remove(matchingTask);

    // サーバ セットアップ リクエストメッセージ
    var serverSetupRequestMessage = new ServerSetupRequestMessage();
    serverSetupRequestMessage.matchId       = matchIdCounter++;
    serverSetupRequestMessage.sceneName     = "NetworkProvingGround";
    serverSetupRequestMessage.matchingUsers = matchingUsers;
    matchingKeys.push(matchId);

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

// マッチ情報を初期化
async function initMatch(task) {
    setupQueue.logger.debug("init matching.");
    task.matchId = matchIdCounter++;
    return findServer;
}

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
    matchConnectData.serverAddress = serverAddress;
    matchConnectData.serverPort    = serverPort;
    matchConnectData.matchId       = matchData.matchId;
    task.matchConnectData = matchConnectData;
    return sendMatchConnectData;
}

// マッチ完了を通知
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
