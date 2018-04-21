var util           = require('util');
var uuid           = require('uuid/v1');
var config         = require('./services/library/config');
var logger         = require('./services/library/logger');
var models         = require('./services/protocols/models');
var UserData       = models.UserData;
var PlayerData     = models.PlayerData;
var SessionData    = models.SessionData;
var CredentialData = models.CredentialData;
var UniqueKeyData  = models.UniqueKeyData;

// WebAPI コントローラ
// spec.yml の WebAPI 定義に対応する経路の実装。
class WebAPIController {
    //-------------------------------------------------------------------------- 生成と破棄
    // コンストラクタ
    constructor() {
        // NOTE
        // 今のところ処理なし
    }

    //-------------------------------------------------------------------------- 経路実装
    // サインアップ
    async Signup(req, res) {
        // プレイヤー作成
        var playerData = new PlayerData();
        playerData.playerId   = null;
        playerData.playerName = "Player" + Math.floor(Math.random() * 1000);
        await saveData(playerData, "playerId", "%8s");

        // ユーザ作成
        var userData = new UserData();
        userData.userId       = null;
        userData.playerId     = playerData.playerId;
        userData.sessionToken = null;
        userData.signinToken  = null;
        await saveData(userData, "userId", "%8s");

        // セッショントークン作成
        var sessionTokenData = new UniqueKeyData();
        sessionTokenData.associatedKey = userData.userId;
        await saveData(sessionTokenData, null, "%16s");
        var sessionData = new SessionData();
        sessionData.sessionToken = sessionTokenData._id;

        // サインイントークン作成
        var signinTokenData = new UniqueKeyData();
        signinTokenData.associatedKey = userData.userId;
        await saveData(signinTokenData, null, "%16s");
        var credentialData = new CredentialData();
        credentialData.signinToken = signinTokenData._id;

        // ユーザ更新
        userData.sessionToken = sessionData.sessionToken;
        userData.signinToken  = credentialData.signinToken;
        await saveData(userData);

        // サインアップ完了！
        res.send({
            activeData: {
                playerData:     { active:true, data:playerData.export()     },
                sessionData:    { active:true, data:sessionData.export()    },
                credentialData: { active:true, data:credentialData.export() },
            },
        });
    }

    // サインイン
    async Signin(req, res) {
        var signinToken = req.body.signinToken;

        // サインイントークンからユーザ情報を検索
        var userData = await findData(UserData, {signinToken:signinToken});

        // セッショントークン作成
        var sessionData = new SessionData();
        sessionData.sessionToken = await createUniqueKey("%16s");

        // ユーザ情報更新
        var oldSessionToken = userData.sessionToken;
        userData.sessionToken = sessionData.sessionToken;
        await saveData(userData);

        // 古いセッショントークンを破棄
        await destroyUniqueKey(oldSessionToken);

        // プレイヤー情報取得
        var playerData = await getData(PlayerData, userData.playerId);

        // サインイン完了！
        res.send({
            activeData: {
                playerData:  { active:true, data:playerData.export()  },
                sessionData: { active:true, data:sessionData.export() },
            },
        });
    }

    // マッチングのリクエスト
    //Matching(req, res) {
    //    // TODO
    //}

    // プレイヤ情報の取得
    //Player(req, res) {
    //    // TODO
    //}

    // テスト API
    Test(req, res) {
        var resValue = {resValue:15};
        logger.webapiServer.debug("Test: incoming: " + util.inspect(req.body, {depth:null,breakLength:Infinity}));
        logger.webapiServer.debug("Test: outgoing: " + util.inspect(resValue, {depth:null,breakLength:Infinity}));
        res.send(resValue);
    }

    //-------------------------------------------------------------------------- ミドルウェア
    // ミドルウェアの使用
    use(name) {
        var middlewares = null;
        switch (name) {
        case 'always':
            middlewares = [this.checkSessionToken];
            break;
        case 'userSecurity':
            middlewares = [this.userSecurity];
            break;
        case 'adminSecurity':
            middlewares = [this.adminSecurity];
            break;
        default:
            break;
        }
        return middlewares;
    }

    // セッショントークン確認
    checkSessionToken(req, res, next) {
        // TODO
        next();
    }

    // サインインユーザ確認
    userSecurity(req, res, next) {
        // TODO
        next();
    }

    // 管理者ユーザ確認
    adminSecurity(req, res, next) {
        // TODO
        next();
    }
}

// データの保存
async function saveData(data, fieldName, key) {
    return new Promise((resolve, reject) => {
        data.save(fieldName, key, (err) => {
            if (err) {
                reject(err);
                return;
            }
            resolve();
        });
    });
}

// データの取得
async function getData(dataType, key) {
    return new Promise((resolve, reject) => {
        dataType.get(key, (err, data) => {
            if (err) {
                reject(err);
                return;
            }
            if (!data) {
                reject(err);
                return;
            }
            resolve(data);
        });
    });
}

// データの検索
async function findData(dataType, params) {
    return new Promise((resolve, reject) => {
        params.limit = 1;
        dataType.list(params, (err, list) => {
            if (err) {
                reject(err);
                return;
            }
            if (list.length <= 0) {
                reject(new Error('no data'));
                return;
            }
            var data = list[0].activate();
            resolve(data);
        });
    });
}

module.exports = WebAPIController;
