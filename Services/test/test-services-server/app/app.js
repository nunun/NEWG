var util            = require('util');
var config          = require('services-library').config;
var logger          = require('services-library').logger;
var webSocketServer = require('services-library').WebSocketServer.activate(config.webSocketServer, logger.webSocketServer);

// websocket server
//webSocketServer.setAccepter(function(req) {
//});
webSocketServer.setStartEventListener(function() {
    logger.webSocketServer.info("start");
});
webSocketServer.setConnectEventListener(function(webSocketClient) {
    logger.webSocketServer.info(util.inspect(webSocketClient));
});
webSocketServer.setDisconnectEventListener(function(webSocketClient, code, reason) {
    logger.webSocketServer.info(util.inspect(webSocketClient));
    logger.webSocketServer.info(code);
    logger.webSocketServer.info(reason);
});
webSocketServer.setDataEventListener(0, function(webSocketClient, data, res) {
    res.send(data); // NOTE response to requester's callback.
});
webSocketServer.setDataEventListener(1, function(webSocketClient, data, res) {
    webSocketClient.send(1, data); // NOTE response to requester's data event listener.
});

// start
webSocketServer.start();