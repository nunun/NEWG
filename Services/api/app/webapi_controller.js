var util   = require('util');
var config = require('./services/library/config');
var logger = require('./services/library/logger');

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
    //Signup(req, res) {
    //    // TODO
    //}

    // サインイン
    //Signin(req, res) {
    //    // TODO
    //}

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
