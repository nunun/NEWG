exports = {}
exports.setup = function(router, routesController, client, logger) {
    if (!client) {
        logger.debug('routes.setup: binding "Signup" for route "/signup".');
        // Signup
        // サインアップAPI
        var Signup_impl = routesController.Signup;
        if (Signup_impl) {
            router.post("/signup", function(req, res) {
                Signup_impl(req, res);
            });
        } else {
            logger.error('routes.setup: routes controller has no implement "Signup" for route "/signup".');
        }
    }
    if (!client) {
        logger.debug('routes.setup: binding "Signin" for route "/signin".');
        // Signin
        // サインインAPI
        var Signin_impl = routesController.Signin;
        if (Signin_impl) {
            router.post("/signin", function(req, res) {
                Signin_impl(req, res);
            });
        } else {
            logger.error('routes.setup: routes controller has no implement "Signin" for route "/signin".');
        }
    }
    if (!client) {
        logger.debug('routes.setup: binding "Test" for route "/test".');
        // Test
        // ユニットテスト用
        var Test_impl = routesController.Test;
        if (Test_impl) {
            router.post("/test", function(req, res) {
                Test_impl(req, res);
            });
        } else {
            logger.error('routes.setup: routes controller has no implement "Test" for route "/test".');
        }
    }
}
module.exports = exports;
