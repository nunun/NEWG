var url          = require('url');
var assert       = require('assert');
var util         = require('util');
var EventEmitter = require('events').EventEmitter;
var WebSocket    = require('ws');
var uuid         = require('uuid/v1');
var trim         = require('string.prototype.trim');
var config       = require('./config');
var logger       = require('./logger');

// コンストラクタ
function MatchingServer() {
    this.init();
}
util.inherits(MatchingServer, EventEmitter);

// 初期化
MatchingServer.prototype.init = function() {
    this.wss     = null;                  // ウェブソケットサーバ
    this.config  = config.matchingServer; // 設定
    this.uuid    = uuid();                // サーバ固有番号
    this.started = false;                 // 開始フラグ
};

// start
MatchingServer.prototype.start = function(port) {
    logger.matchingServer.debug('start.');
    assert.ok(!this.started);
    this.started = true;
    var self = this;

    // ウェブソケットサーバ作成
    logger.matchingServer.info('listening on port ' + self.config.port + '.');
    self.wss = new WebSocket.Server({port:self.config.port});

    // on connect
    self.wss.on('connection', function(ws, req) {
        var clientName = req.connection.remoteAddress;
        logger.matchingServer.info(clientName + ': matching client connected.');

        // 接続キー
        // マッチングサーバの接続キーはユーザ識別用
        var location = url.parse(req.url, true);
        if (!location.query.ck) {
            ws.terminate();
            return;
        }

        // マッチングクライアント
        var matchingClient = {};
        matchingClient.ws   = ws;
        matchingClient.uuid = location.query.ck;
        self.emit('connect', matchingClient);

        // send
        matchingClient.ok = function(data) {
            matchingClient.send(true, data);
        }
        matchingClient.ng = function(data, message) {
            if (message) {
                data.message = message;
            }
            matchingClient.send(false, data);
        }
        matchingClient.send = function(ok, data) {
            if (!data) {
                data = ok;
                ok = true;
            }
            if (typeof(data) === 'boolean') {
                ok = data;
                data = {};
            }
            if (typeof(data) === 'string') {
                data = {message: data};
            }
            data.ok = (ok)? true : false;
            matchingClient.ws.emit('send', data);
        }

        // on send
        matchingClient.ws.on('send', function(data) {
            logger.matchingServer.debug(clientName + ': ' + matchingServer.uuid + ': ' + matchingClient.uuid +': send: data[' + data + ']');
            var message = JSON.stringify(data);
            matchingClient.ws.send(message);
        });

        // on message
        matchingClient.ws.on('message', function(message){
            logger.matchingServer.debug(clientName + ': ' + matchingServer.uuid + ': ' + matchingClient.uuid + ': message: message[' + message + ']');

            // parse message to json data
            var data = null;
            try {
                // null?
                if (!message) {
                    throw new Error('message is null.');
                }

                // empty?
                message = trim(message);
                if (message == '') {
                    throw new Error('message is empty.');
                }

                // parse json
                data = JSON.parse(message);
                if (!data) {
                    throw new Error('data is null.');
                }
            } catch (e) {
                logger.matchingServer.debug(clientName + ': ' + matchingServer.uuid + ': ' + matchingClient.uuid +': message: failed to parse json. (e = "' + err.toString() + '")');
                matchingClient.ng('failed to parse json. (' + err.toString() + ')');
                return;
            }

            // handle data
            self.emit('data', matchingClient, data);
        });

        // on close
        matchingClient.ws.on('close', function(code, reason){
            logger.matchingServer.info(clientName + ': ' + matchingServer.uuid + ': ' + matchingClient.uuid + ': matching client disconnected. (code = ' + code + ', reason = "' + reason + '")');
            self.emit('disconnect', matchingClient, code, reason);
        });

        // on error
        matchingClient.ws.on('error', function(err){
            logger.matchingServer.info(clientName + ': ' + matchingServer.uuid + ': ' + matchingClient.uuid + ': matching client error. (' + err + ')');
        });
    });

    // on listening
    self.wss.on('listening', function() {
        self.emit('start');
    });
}

module.exports = new MatchingServer();
