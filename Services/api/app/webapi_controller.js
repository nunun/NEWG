var util           = require('util');
var uuid           = require('uuid/v1');
var config         = require('./services/library/config');
var logger         = require('./services/library/logger');
var models         = require('./services/protocols/models');
var UserData       = models.UserData;
var PlayerData     = models.PlayerData;
var SessionData    = models.SessionData;
var CredentialData = models.CredentialData;
var MatchingData   = models.MatchingData;
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
        await playerData.keyProperty("playerId").promiseSave("%8s");

        // ユーザ作成
        var userData = new UserData();
        userData.userId       = null;
        userData.playerId     = playerData.playerId;
        userData.sessionToken = null;
        userData.signinToken  = null;
        await userData.keyProperty("userId").promiseSave("%8s");

        // セッショントークン作成
        var sessionTokenData = new UniqueKeyData();
        sessionTokenData.associatedDataType = "userId";
        sessionTokenData.associatedDataKey  = userData.userId;
        await sessionTokenData.promiseSave("%16s");
        var sessionData = new SessionData();
        sessionData.sessionToken = sessionTokenData._id;

        // サインイントークン作成
        var signinTokenData = new UniqueKeyData();
        signinTokenData.associatedDataType = "userId";
        signinTokenData.associatedDataKey  = userData.userId;
        await signinTokenData.promiseSave("%16s");
        var credentialData = new CredentialData();
        credentialData.signinToken = signinTokenData._id;

        // ユーザデータ更新
        userData.sessionToken = sessionData.sessionToken;
        userData.signinToken  = credentialData.signinToken;
        await userData.promiseSave();

        // リクエスト完了！
        return {
            activeData: {
                playerData:     { active:true, data:playerData.export()     },
                sessionData:    { active:true, data:sessionData.export()    },
                credentialData: { active:true, data:credentialData.export() },
            },
        };
    }

    // サインイン
    async Signin(req, res) {
        var signinToken = req.body.signinToken;

        // サインイントークンを探す
        var signinTokenData = await UniqueKeyData.promiseGet(signinToken);

        // ユーザデータを特定
        var userData = await UserData.promiseGet(signinTokenData.associatedDataKey);
        if (userData.signinToken != signinToken) {
            throw new Error(401, 'invalid token'); // NOTE 古いトークンを使用しようとした
        }

        // セッショントークン作成
        var sessionTokenData = new UniqueKeyData();
        sessionTokenData.associatedDataType = "userId";
        sessionTokenData.associatedDataKey   = userData.userId;
        await sessionTokenData.promiseSave("%16s");
        var sessionData = new SessionData();
        sessionData.sessionToken = sessionTokenData._id;

        // ユーザデータ更新
        var oldSessionToken = userData.sessionToken;
        userData.sessionToken = sessionData.sessionToken;
        await userData.promiseSave();

        // 古いトークンは破棄
        await UniqueKeyData.promiseDestroy(oldSessionToken);

        // プレイヤー情報取得
        var playerData = await PlayerData.promiseGet(userData.playerId);

        // リクエスト完了！
        return {
            activeData: {
                playerData:  { active:true, data:playerData.export()  },
                sessionData: { active:true, data:sessionData.export() },
            },
        };
    }

    // プレイヤ情報の取得
    async Player(req, res) {
        // プレイヤー情報取得
        var playerData = await PlayerData.promiseGet(req.userData.playerId);

        // リクエスト完了！
        return {
            activeData: {
                playerData: { active:true, data:playerData.export() },
            },
        };
    }

    // 名前の変更
    async Rename(req, res) {
        // プレイヤー情報取得
        var playerData = await PlayerData.promiseGet(req.userData.playerId);

        // プレイヤ名を変更して保存
        playerData.playerName = req.body.playerName;
        await playerData.promiseSave();

        // リクエスト完了！
        return {
            activeData: {
                playerData: { active:true, data:playerData.export() },
            },
        };
    }

    // マッチングのリクエスト
    Matching(req, res) {
        // マッチング情報作成
        var matchingData = new MatchingData();
        matchingData.matchingId = null;
        matchingData.users      = [req.userData.userId];
        await matchingData.keyProperty("matchingId").cacheTTL(30).saveCache("%16s");

        // リクエスト完了！
        return {
            matchingServerUrl: "ws://localhost:7755?matchingId=" + matchingData.matchingId,
        };
    }

    // テスト API
    async Test(req, res) {
        var resValue = {resValue:15};
        logger.webapiServer.debug("Test: incoming: " + util.inspect(req.body, {depth:null,breakLength:Infinity}));
        logger.webapiServer.debug("Test: outgoing: " + util.inspect(resValue, {depth:null,breakLength:Infinity}));
        res.send(resValue);
    }

    //-------------------------------------------------------------------------- コール
    // コントローラメソッドの呼び出しとエラーハンドリング
    async call(method, req, res) {
        try {
            var result = await method(req, res);
            res.status(200).send(result);
        } catch (err) {
            if (!(err instanceof Error)) {
                err = new Error(500, err);
            }
            res.status(err.number || 500).send({err:err});
        }
    }

    //-------------------------------------------------------------------------- バリデーション
    // リクエストバリデーション
    validate(req, paramName, validates) {
        // TODO
        console.log(req.body, paramName, validates);
        return true;
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
        // ヘッダから探す
        var sessionToken = req.get('SessionToken');
        if (!sessionToken) {
            // 一応リクエスト本文からも探す
            sessionToken = req.body.sessionToken;
            if (!sessionToken) {
                req.userData = null;
                next();
                return;
            }
        }

        // サインイントークンを探す
        UniqueKeyData.get(sessionToken, (err, sessionTokenData) => {
            if (err) {
                req.userData = null;
                next();
                return;
            }

            // ユーザデータを特定
            UserData.get(sessionTokenData.associatedDataKey, (err, userData) => {
                if (err) {
                    req.userData = null;
                    next();
                    return;
                }

                // NOTE
                // 古いトークンを使用しようとした
                if (!userData || userData.sessionToken != sessionToken) {
                    req.userData = null;
                    next();
                    return;
                }

                // ユーザ特定成功
                req.userData = userData;
                next();
            });
        });
    }

    // サインインユーザ確認
    userSecurity(req, res, next) {
        // ユーザ情報チェック
        if (!req.userData) {
            res.status(401).send({err:new Error('unauthorized')});
            return;
        }
        next();
    }

    // 管理者ユーザ確認
    adminSecurity(req, res, next) {
        // ユーザ情報 && 役職チェック
        if (!req.userData || req.userData.role != 'admin') {
            res.status(401).send({err:new Error('unauthorized')});
            return;
        }
        next();
    }
}

module.exports = WebAPIController;
