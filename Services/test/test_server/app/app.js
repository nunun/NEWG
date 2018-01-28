var config          = require('./config');
var logger          = require('./logger');
var webSocketServer = require('libservices').WebSocketServer.activate();

// websocket server
webSocketServer.setConfig(config.webSocketServer);
webSocketServer.setLogger(logger.webSocketServer);
//webSocketServer.setAccepter(function(req) {
//});
//matchingServer.on('start', function(webSocketClient) {
//});
//matchingServer.on('connect', function(webSocketClient) {
//});
//matchingServer.on('disconnect', function(webSocketClient) {
//});
//matchingServer.on('data', function(webSocketClient, data) {
//});

// start
webSocketServer.start();
