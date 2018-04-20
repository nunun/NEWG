var util           = require('util');
var uuid           = require('uuid/v1');
var config         = require('./services/library/config');
var logger         = require('./services/library/logger');
var models         = require('./services/protocols/models');
var UniqueKey      = require('./services/library/unique_key');
var UserData       = models.UserData;
var PlayerData     = models.PlayerData;
var SessionData    = models.SessionData;
var CredentialData = models.CredentialData;

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
    Signup(req, res) {
        // SessionToken を作成
        UniqueKey.create("%16s", (serr, sessionToken) => {
            if (serr) {
                res.status(500).send({err:serr});
                return;
            }
            var sessionData = new SessionData();
            sessionData.sessionToken = sessionToken;

            // SigninToken を作成
            UniqueKey.create("%32s", (ierr, signinToken) => {
                if (ierr) {
                    res.status(500).send({err:ierr});
                    return;
                }
                var credentialData = new CredentialData();
                credentialData.signinToken = signinToken;

                // PlayerData を登録
                var playerData = new PlayerData();
                playerData.playerName = "Player" + Math.floor(Math.random() * 1000);
                playerData.save("playerId", "%8s", (perr, pid, prev) => {
                    if (perr) {
                        res.status(500).send({err:perr});
                        return;
                    }

                    // UserData を登録
                    var userData = new UserData();
                    userData.playerId     = pid;
                    userData.sessionToken = sessionToken;
                    userData.signinToken  = signinToken;
                    userData.save("userId", "%8s", (uerr, uid, urev) => {
                        if (uerr) {
                            res.status(500).send({err:uerr});
                            return;
                        }

                        // サインアップ完了！
                        res.send({
                            activeData: {
                                playerData:     { active:true, data:playerData.export()     },
                                sessionData:    { active:true, data:sessionData.export()    },
                                credentialData: { active:true, data:credentialData.export() },
                            },
                        });
                    });
                });
            });
        });
    }

    // サインイン
    Signin(req, res) {
        // サインイントークンからユーザデータを特定
        var signinToken = req.body.signinToken;
        UserData.list({signinToken:signinToken, limit:1}, (uerr, userList) => {
            if (uerr) {
                res.status(500).send({err:uerr});
                return;
            }
            if (userList.length <= 0) {
                res.status(500).send({err:'no user.'});
                return;
            }
            var userData = userList[0].activate();

            // SessionToken を再作成
            UniqueKey.create("%16s", (serr, sessionToken) => {
                if (serr) {
                    res.status(500).send({err:serr});
                    return;
                }
                var sessionData = new SessionData();
                sessionData.sessionToken = sessionToken;

                // ユーザデータ更新
                var oldSessionToken = userData.sessionToken;
                userData.sessionToken = sessionToken;
                userData.save(userData.userId, (verr, vid, vrev) => {
                    if (verr) {
                        res.status(500).send({err:verr});
                        return;
                    }

                    // セッショントークンを
                    UniqueKey.destroy(oldSessionToken, (derr) => {
                        // NOTE
                        // 削除し損じても無視

                        // プレイヤーデータを取得
                        PlayerData.get(userData.playerId, (perr, playerData) => {
                            if (perr) {
                                res.status(500).send({err:perr});
                                return;
                            }

                            // サインイン完了！
                            res.send({
                                activeData: {
                                    playerData:  { active:true, data:playerData.export()  },
                                    sessionData: { active:true, data:sessionData.export() },
                                },
                            });
                        });
                    });
                });
            });
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

module.exports = WebAPIController;
