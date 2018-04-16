var util   = require('util');
var config = require('./services/library/config');
var logger = require('./services/library/logger');

// WebAPI コントローラ
// spec.yml の WebAPI 定義に対応する経路の実装。
class WebAPIController {
    //-------------------------------------------------------------------------- 生成と破棄
    // コンストラクタ
    constructor() {
        this.middlewares = {};
    }

    //-------------------------------------------------------------------------- ルート処理
    // サインアップ
    //Signup(req, res) {
    //    // TODO
    //}

    // サインイン
    //Signin(req, res) {
    //    // TODO
    //}

    // テスト API
    Test(req, res) {
        var resValue = {resValue:15};
        logger.webapiServer.debug("Test: incoming: " + util.inspect(req.body, {depth:null,breakLength:Infinity}));
        logger.webapiServer.debug("Test: outgoing: " + util.inspect(resValue, {depth:null,breakLength:Infinity}));
        res.send(resValue);
    }
}

module.exports = WebAPIRoutesController;
