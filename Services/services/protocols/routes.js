exports = {}
exports.setup = function(router, controller, middlewares, client, logger) {
    if (!client) {
        logger.debug('routes.setup: binding "Signup" for route "/signup".');
        // Signup
        // サインアップAPI
        var Signup_impl = controller.Signup;
        if (Signup_impl) {
            do {
                router.post("/signup", function(req, res) {
                    Signup_impl(req, res);
                });
            } while(false);
        } else {
            logger.error('routes.setup: routes controller has no implement "Signup" for route "/signup".');
        }
    }
    if (!client) {
        logger.debug('routes.setup: binding "Signin" for route "/signin".');
        // Signin
        // サインインAPI
        var Signin_impl = controller.Signin;
        if (Signin_impl) {
            do {
                router.post("/signin", function(req, res) {
                    Signin_impl(req, res);
                });
            } while(false);
        } else {
            logger.error('routes.setup: routes controller has no implement "Signin" for route "/signin".');
        }
    }
    if (!client) {
        logger.debug('routes.setup: binding "Test" for route "/test".');
        // Test
        // ユニットテスト用
        var Test_impl = controller.Test;
        if (Test_impl) {
            do {
                router.post("/test", function(req, res) {
                    Test_impl(req, res);
                });
            } while(false);
        } else {
            logger.error('routes.setup: routes controller has no implement "Test" for route "/test".');
        }
    }
}
module.exports = exports;
