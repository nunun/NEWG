var util   = require('util');
var config = require('./services/library/config');
var logger = require('./services/library/logger');

// WebAPI 経路ハンドラ
// spec.yml から生成した services/protocols/routes.js の実装。
class WebAPIRoutesHandler {
    //-------------------------------------------------------------------------- ルート処理
    // サインアップ
    static Signup(req, res) {
        // TODO
    }

    // サインイン
    static Signin(req, res) {
        // TODO
    }

    // テスト API
    static Test(req, res) {
        var resValue = {resValue:15};
        logger.webapiServer.debug("Test: incoming: " + util.inspect(req.body, {depth:null,breakLength:Infinity}));
        logger.webapiServer.debug("Test: outgoing: " + util.inspect(resValue, {depth:null,breakLength:Infinity}));
        res.send(resValue);
    }
}

module.exports = WebAPIRoutesHandler;
