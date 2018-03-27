exports = {}
exports.setup = function(router, binder, client, logger) {
    if (!client) {
        logger.debug('routes.setup: binding "Signup" for route "/signup".');
        // Signup
        // サインアップAPI
        var Signup_impl = binder.Signup;
        if (!Signup_impl) {
            logger.error('routes.setup: binder has no implement "Signup" for route "/signup".');
            return;
        }
        router.post("/signup", function(req, res) {
            Signup_impl(req, res);
        });
    }
    if (!client) {
        logger.debug('routes.setup: binding "Signin" for route "/signin".');
        // Signin
        // サインインAPI
        var Signin_impl = binder.Signin;
        if (!Signin_impl) {
            logger.error('routes.setup: binder has no implement "Signin" for route "/signin".');
            return;
        }
        router.post("/signin", function(req, res) {
            Signin_impl(req, res);
        });
    }
    if (!client) {
        logger.debug('routes.setup: binding "Test" for route "/test".');
        // Test
        // テストインターフェイス
        var Test_impl = binder.Test;
        if (!Test_impl) {
            logger.error('routes.setup: binder has no implement "Test" for route "/test".');
            return;
        }
        router.post("/test", function(req, res) {
            Test_impl(req, res);
        });
    }
}
module.exports = exports;
