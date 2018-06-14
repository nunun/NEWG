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
    var task = matchingBrainQueue.getTaskAtKey(data.joinId);
    if (!task) {
        var errorMessage = 'joinId not found? (' + data.joinId + ')';
        logger.matchingServer.debug(errorMessage);
        mindlinkClient.sendToRemote(res.to, 0, {err:new Error(errorMessage)});
        return;
    }
    task.joinResponseMessage = data;
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
    if (connectingQueue.isFull()) {
        logger.matchingServer.debug("connectingQueue is full. refuse matchingClient.");
        matchingClient.stop();
        return;
    }
    var task = TaskQueue.createTaskFromKey(matchingClient);
    connectingQueue.add(task);
});
matchingServer.setDisconnectEventListener(function(matchingClient) {
    logger.matchingServer.debug("matchingClient disconnected.");
    connectingQueue.removeKey(matchingClient);
    matchingQueue.removeKey(matchingClient);
    errorQueue.removeKey(matchingClient);
});

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// 接続キュー
// 接続してきたユーザを識別し、マッチングキューに引継ぎます。
var connectingQueue = new TaskQueue(config.connectingQueue, logger.connectingQueue);
connectingQueue.setAddEventListener((task) => {
    connectingQueue.logger.debug("task added.");
    return identifyUser;
});
connectingQueue.setAbortEventListener((err, task) => {
    connectingQueue.logger.debug("task aborted (" + err + ").");
    task.err = err;
    connectingQueue.remove(task);
    errorQueue.add(task);
});

// ユーザ識別
async function identifyUser(task) {
    if (matchingQueue.isFull()) {
        return 1000;//次のキューがフルなら待ち
    }
    connectingQueue.logger.debug("identify user.");

    // 初期化
    var matchingClient = task._key;
    task.matchingId = matchingClient.acceptData.matchingId;
    task.userId     = null;
    task.userData   = null;

    // マッチングデータ取得
    connectingQueue.logger.debug('matchingId = ' + task.matchingId);
    var matchingData = await MatchingData.promiseGetCache(task.matchingId);
    if (!matchingData) {
        throw new Erorr("invalid matchingId");
    }
    task.userId = matchingData.users[0];

    // ユーザ取得
    connectingQueue.logger.debug('userId = ' + task.userId);
    var userData = await UserData.promiseGet(matchingClient.userId);
    if (!userData) {
        throw new Error("invalid userId");
    }

    // ユーザ確認完了
    connectingQueue.logger.debug('user found.');
    connectingQueue.remove(task);
    matchingQueue.add(task);
    return null;
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// マッチングキュー
// マッチング待ちユーザのキューです。
// 基本的に何もせず、マッチングブレインによりマッチングされるのを待ちます。
var matchingQueue = new TaskQueue(config.matchingQueue, logger.matchingQueue);
matchingQueue.setAddEventListener((task) => {
    matchingQueue.logger.debug("task added.");
    return null;
});
matchingQueue.setAbortEventListener((err, task) => {
    matchingQueue.logger.debug("task aborted (" + err + ").");
    task.err = err;
    errorQueue.add(task);
});

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// マッチングブレイン供給タイマー
var matchingBrainSupplyTimer = null;

// 参加ID生成用
var joinIdCounter = 0;

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
            var matchingBrainTask = TaskQueue.createTaskFromKey(++joinIdCounter);
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
// マッチングキューを監視してユーザのマッチングを行い、
// 参加できそうなサーバに目星を付けた後、サーバのセットアップを行い
// マッチング結果をユーザに通知します。
var matchingBrainQueue = new TaskQueue(config.matchingBrainQueue, logger.matchingBrainQueue);
matchingBrainQueue.setAddEventListener((task) => {
    matchingBrainQueue.logger.debug("task added.");
    return makeMatching;
});
matchingBrainQueue.setAbortEventListener((err, task) => {
    matchingBrainQueue.logger.debug("task aborted (" + err + ").");
    abortMatching(task);
});

// マッチング
async function makeMatching(task) {
    //マッチングが成立するためのユーザいない
    if (matchingQueue.getLength() <= 0) {
        return 1000;
    }
    matchingBrainQueue.logger.debug("waiting user found. make matching ...");

    // NOTE
    // マッチングするユーザを探す。
    // 今はマッチングキューの先頭からひとつずつ取る。
    var matchingTasks = [];
    var l = matchingQueue.getLength();
    for (var i = 0; i < l; i++) {
        var matchingTask = matchingQueue.getTaskAt(i);
        if (matchingTask.lockId) {
            if (matchingBrainTask.indexOfKey(matchingTask.lockId) < 0) {
                matchingTask.lockId = null;//ロックID無効につき解除
            }
        }
        if (!matchingTask.lockId) {
            matchingTask.lockId = task._key;//ロック
            matchingTasks.push(matchingTask);
            break;
        }
    }

    // マッチングできるユーザが見つからない
    if (matchingTasks.length <= 0) {
        return 1000;
    }

    // ユーザID一覧作成
    var matchingUsers = [];
    for (var i in matchingTasks) {
        var matchingTask = matchingTasks[i];
        matchingUsers.push(matchingTask.userId);
    }

    // 参加リクエスト準備
    var joinRequestMessage = new JoinRequestMessage();
    joinRequestMessage.joinId    = task._key;
    joinRequestMessage.sceneName = "MapProvingGround"; // TODO マップ選択
    joinRequestMessage.users     = matchingUsers;

    // セットアップ開始
    task.matchingTasks      = matchingTasks;
    task.joinRequestMessage = joinRequestMessage;
    return findServer;
}

// 参加できそうなサーバを探す
function findServer(task) {
    matchingBrainQueue.logger.debug("find server.");
    return new Promise((resolved, reject) => {
        var cond = ".*{.alias == \"server\" && (.serverState == \"standby\" || .serverState == \"ready\") && .load < 1.0}";
        mindlinkClient.sendQuery(cond, function(err,services) {
            if (err) {
                resolved(abortMatching);//送信エラー
                return;
            }
            if (!services || services.length <= 0) {
                resolved(abortMatching);//サービス無し
                return;
            }
            task.service = services[0];
            matchingBrainQueue.logger.debug('server found: service[' + util.inspect(task.service, {depth:null,breakLength:Infinity}) + ']');
            resolved(joinRequest);
        });
    });
}

// 参加リクエスト
async function joinRequest(task) {
    matchingBrainQueue.logger.debug("join request.");
    mindlinkClient.sendToRemote(task.service.clientUuid, 0, task.joinRequestMessage);
    return waitForJoinResponse;
}

// 参加レスポンス待ち
async function waitForJoinResponse(task) {
    if (task._count == 0) {
        matchingBrainQueue.logger.debug("wait for join response.");
        task.waitCount = 0;
    }

    // メッセージ来た？
    if (!task.joinResponseMessage) {
        if (task.waitCount++ >= 30) {
            return abortMatching;
        }
        return 1000;
    }

    // なんと、エラーだった
    // ユーザには通知せずにマッチングを中断
    if (!task.joinResponseMessage.error) {
        matchingBrainQueue.logger.debug(task.joinResponseMessage.error);
        return abortMatching;
    }

    // セットアップが完了したらしいので
    // 各ユーザにマッチ完了を通知
    var matchConnectData = new MatchConnectData();
    matchConnectData.serverAddress   = task.service.serverAddress;
    matchConnectData.serverPort      = task.service.serverPort;
    matchConnectData.serverToken     = task.joinResponseMessage.serverToken;
    matchConnectData.serverSceneName = task.joinResponseMessage.sceneName;
    task.matchConnectData = matchConnectData;
    return sendMatchConnectData;
}

// 各ユーザにマッチ完了を通知
async function sendMatchConnectData(task) {
    matchingBrainQueue.logger.debug("send match connect data.");
    var matchingTasks = task.matchingTasks;
    for (var i in matchingTasks) {
        var matchingTask = matchingTasks[i];
        matchingTask._key.send(0, task.matchConnectData);
    }
    return waitForMatchingDisconnect;
}

// 数秒待って閉じる
async function waitForMatchingDisconnect(task) {
    if (task._count == 0) {
        matchingBrainQueue.logger.debug("wait for matching disconnect.");
        return 3000;
    }
    var matchingTasks = task.matchingTasks;
    for (var i in matchingTasks) {
        var matchingTask = matchingTasks[i];
        matchingTask._key.stop();
        matchingQueue.remove(matchingTask);
    }
    matchingBrainQueue.remove(task);
    return null;
}

// マッチング中断
async function abortMatching(task) {
    var matchingTasks = task.matchingTasks;
    for (var i in matchingTasks) {
        var matchingTask = matchingTasks[i];
        matchingTask.lockId = null;//ロック解除
    }
    matchingBrainQueue.remove(task);
    return null;
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// エラーキュー
// エラーを送信して数秒後に切断するキューです。
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

// start app ...
couchClient.start();
