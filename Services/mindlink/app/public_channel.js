var url            = require('url');
var assert         = require('assert');
var util           = require('util');
var uuid           = require('uuid/v1');
var trim           = require('string.prototype.trim');
var EventEmitter   = require('events').EventEmitter;
var WebSocket      = require('ws');
var config         = require('services-library').config;
var logger         = require('services-library').logger;
var mindlinkServer = require('./mindlink_server');

// constructor
function PublicChannel() {
    this.init();
}
util.inherits(PublicChannel, EventEmitter);

// data types
PublicChannel.DATA_TYPE = {};
PublicChannel.DATA_TYPE.S = 1; // SERVICE
PublicChannel.DATA_TYPE.Q = 2; // QUERY
PublicChannel.DATA_TYPE.M = 3; // MESSAGE
PublicChannel.prototype.DATA_TYPE = PublicChannel.DATA_TYPE;

// init
PublicChannel.prototype.init = function() {
    this.wss     = null;                 // websocket server
    this.config  = config.publicChannel; // configuration
    this.started = false;                // started flag
};

// start
PublicChannel.prototype.start = function() {
    logger.publicChannel.debug('start.');
    assert.ok(!this.started);
    this.started = true;
    var self = this;

    // websocket server
    logger.publicChannel.info('listening on port ' + self.config.port + '.');
    self.wss = new WebSocket.Server({port:self.config.port});

    // on connect
    self.wss.on('connection', function(ws, req) {
        var clientName = req.connection.remoteAddress;
        logger.publicChannel.info(clientName + ': mindlink client connected.');

        // check connect key
        if (self.config.connectKey) {
            var location = url.parse(req.url, true);
            if (location.query.ck != self.config.connectKey) {
                ws.terminate();
                return;
            }
        }

        // mindlink client
        var mindlinkClient = {};
        mindlinkClient.ws   = ws;
        mindlinkClient.uuid = uuid();
        self.emit('connect', mindlinkClient);

        // send
        mindlinkClient.ok = function(data) {
            mindlinkClient.send(true, data);
        }
        mindlinkClient.ng = function(data, message) {
            if (message) {
                data.message = message;
            }
            mindlinkClient.send(false, data);
        }
        mindlinkClient.send = function(ok, data) {
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
            mindlinkClient.ws.emit('send', data);
        }

        // on send
        mindlinkClient.ws.on('send', function(data) {
            logger.publicChannel.debug(clientName + ': ' + mindlinkServer.uuid + ': ' + mindlinkClient.uuid +': send: data[' + util.inspect(data, {depth:null,breakLength:Infinity}) + ']');
            var message = JSON.stringify(data);
            mindlinkClient.ws.send(message);
        });

        // on message
        mindlinkClient.ws.on('message', function(message){
            logger.publicChannel.debug(clientName + ': ' + mindlinkServer.uuid + ': ' + mindlinkClient.uuid + ': message: message[' + message + ']');

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
                logger.publicChannel.debug(clientName + ': ' + mindlinkServer.uuid + ': ' + mindlinkClient.uuid +': message: failed to parse json. (e = "' + e.toString() + '")');
                mindlinkClient.ng('failed to parse json. (' + e.toString() + ')');
                return;
            }

            // handle data
            self.emit('data', mindlinkClient, data);
        });

        // on close
        mindlinkClient.ws.on('close', function(code, reason){
            logger.publicChannel.info(clientName + ': ' + mindlinkServer.uuid + ': ' + mindlinkClient.uuid + ': mindlink client disconnected. (code = ' + code + ', reason = "' + reason + '")');
            self.emit('disconnect', mindlinkClient, code, reason);
        });

        // on error
        mindlinkClient.ws.on('error', function(err){
            logger.publicChannel.info(clientName + ': ' + mindlinkServer.uuid + ': ' + mindlinkClient.uuid + ': mindlink client error. (' + util.inspect(err) + ')');
        });
    });

    // on listening
    self.wss.on('listening', function() {
        self.emit('start');
    });
}

module.exports = new PublicChannel();
