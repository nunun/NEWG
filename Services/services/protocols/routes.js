exports = {}
exports.setup = function(router, controller, group, logger) {
    var alwaysMiddlewares = (!controller.use)? null : controller.use("always");
    if (alwaysMiddlewares) {
        for (var i in alwaysMiddlewares) {
            router.use(alwaysMiddlewares[i]);
        }
    }
    if (!group) {
        logger.debug('routes.setup: binding controller method "Signup" to route "/signup".');
        // Signup
        // サインアップAPI
        var Signup_impl = controller.Signup;
        if (Signup_impl) {
            do {
                var routeMidlewares = null;
                router.post("/signup", function(req, res) {
                    Signup_impl(req, res);
                });
            } while(false);
        } else {
            logger.error('routes.setup: controller has no method "Signup" for route "/signup".');
        }
    }
    if (!group) {
        logger.debug('routes.setup: binding controller method "Signin" to route "/signin".');
        // Signin
        // サインインAPI
        var Signin_impl = controller.Signin;
        if (Signin_impl) {
            do {
                var routeMidlewares = null;
                router.post("/signin", function(req, res) {
                    Signin_impl(req, res);
                });
            } while(false);
        } else {
            logger.error('routes.setup: controller has no method "Signin" for route "/signin".');
        }
    }
    if (!group) {
        logger.debug('routes.setup: binding controller method "Matching" to route "/matching".');
        // Matching
        // マッチングをリクエスト
        var Matching_impl = controller.Matching;
        if (Matching_impl) {
            do {
                var routeMidlewares = null;
                routeMiddlewares = (!controller.use)? null : controller.use("userSecurity");
                if (routeMiddlewares) {
                    for (var i in routeMiddlewares) {
                        router.use("/matching", routeMidlewares[i]);
                    }
                } else {
                    logger.error('routes.setup: controller has no middleware "userSecurity" for route "/matching".');
                    break;
                }
                router.post("/matching", function(req, res) {
                    Matching_impl(req, res);
                });
            } while(false);
        } else {
            logger.error('routes.setup: controller has no method "Matching" for route "/matching".');
        }
    }
    if (!group) {
        logger.debug('routes.setup: binding controller method "Player" to route "/player".');
        // Player
        // プレイヤー情報の取得
        var Player_impl = controller.Player;
        if (Player_impl) {
            do {
                var routeMidlewares = null;
                routeMiddlewares = (!controller.use)? null : controller.use("userSecurity");
                if (routeMiddlewares) {
                    for (var i in routeMiddlewares) {
                        router.use("/player", routeMidlewares[i]);
                    }
                } else {
                    logger.error('routes.setup: controller has no middleware "userSecurity" for route "/player".');
                    break;
                }
                router.post("/player", function(req, res) {
                    Player_impl(req, res);
                });
            } while(false);
        } else {
            logger.error('routes.setup: controller has no method "Player" for route "/player".');
        }
    }
    if (!group) {
        logger.debug('routes.setup: binding controller method "Test" to route "/test".');
        // Test
        // ユニットテスト用
        var Test_impl = controller.Test;
        if (Test_impl) {
            do {
                var routeMidlewares = null;
                router.post("/test", function(req, res) {
                    Test_impl(req, res);
                });
            } while(false);
        } else {
            logger.error('routes.setup: controller has no method "Test" for route "/test".');
        }
    }
}
module.exports = exports;
