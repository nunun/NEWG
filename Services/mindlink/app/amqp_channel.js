var EventEmitter   = require('events').EventEmitter;
var assert         = require('assert');
var util           = require('util');
var amqp           = require('amqp');
var config         = require('./services/library/config');
var logger         = require('./services/library/logger');
var mindlinkServer = require('./mindlink_server');

// constructor
function AmqpChannel() {
    this.init();
}
util.inherits(AmqpChannel, EventEmitter);

// data types
AmqpChannel.DATA_TYPE = {};
AmqpChannel.DATA_TYPE.U  = 9000; // CLIENT UPDATE
AmqpChannel.DATA_TYPE.D  = 9001; // CLIENT DISCONNECT
AmqpChannel.DATA_TYPE.M  = 9002; // MESSAGE
AmqpChannel.DATA_TYPE.SQ = 9003; // SERVICE REQUEST
AmqpChannel.DATA_TYPE.SR = 9004; // SERVICE RESPONSE
AmqpChannel.DATA_TYPE.K  = 9005; // KEEPALIVE
AmqpChannel.DATA_TYPE.W  = 9006; // WHOAREYOU
AmqpChannel.prototype.DATA_TYPE = AmqpChannel.DATA_TYPE;

// init
AmqpChannel.prototype.init = function() {
    this.amqp    = null;               // internal amqp connection
    this.uuid    = null;               // mindlink server uuid
    this.config  = config.amqpChannel; // configuration
    this.started = false;              // started flag
};

// start
AmqpChannel.prototype.start = function(uuid) {
    logger.amqpChannel.debug('start.');
    assert.ok(!this.started);
    this.started = true;
    var self = this;

    // uuid
    self.uuid = uuid;

    // amqp connection
    self.amqp = amqp.createConnection(this.config);

    // on ready
    self.amqp.on('ready', function() {
        var qname  = self.uuid;
        var exname = self.config.exchange.name;

        // exchange
        logger.amqpChannel.info('declare exchange "' + exname + '".');
        var ex = self.amqp.exchange(exname, {type: 'direct'});
        self.defaultExchange = ex;

        // queue
        logger.amqpChannel.info('declare queue "' + qname + '".');
        self.amqp.queue(qname, function(q) {
            // bind
            logger.amqpChannel.info('binding queue "' + qname + '" to "' + exname + '" with key "broadcast".');
            q.bind(ex, 'broadcast');
            logger.amqpChannel.info('binding queue "' + qname + '" to "' + exname + '" with key "' + self.uuid + '".');
            q.bind(ex, self.uuid);

            // subscribe
            q.subscribe(function(receivedData) {
                receivedData = receivedData.data.toString('utf8');
                //logger.amqpChannel.debug(self.uuid + ': recv: data[' + receivedData + ']');

                // parse receivedData as json into data
                var data = null;
                try {
                    data = JSON.parse(receivedData);
                } catch (e) {
                    logger.amqpChannel.error('json parse fail. (' + receivedData + ')');
                    return;
                }

                // discard if send from self.
                if (data.serverFrom == self.uuid) {
                    return;
                }
                logger.amqpChannel.debug(self.uuid + ': recv: data[' + receivedData + ']');

                // emit 'data' event handler
                self.emit('data', data);
            });

            // start queueing
            logger.amqpChannel.info(self.uuid + ': start queueing.');

            // start
            self.emit('start');
        });
    });

    // on close
    self.amqp.on('close', function() {
        self.emit('stop');
    });

    // on error
    self.amqp.on('error', function(e) {
        logger.amqpChannel.error(e.toString());
    });

    // on publish
    self.amqp.on('publish', function(key, sendData) {
        logger.amqpChannel.debug(self.uuid + ': publish: key[' + key + '] sendData[' + util.inspect(sendData, {depth:null,breakLength:Infinity}) + ']');
        if (!self.defaultExchange) {
            logger.amqpChannel.debug(self.uuid + ': exchange is not initialized yet.');
            return;
        }
        self.defaultExchange.publish(key, sendData);
    });
}

// publish
AmqpChannel.prototype.publish = function(key, ok, data) {
    if (data === undefined) {
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
    var sendData = JSON.stringify(data);
    this.amqp.emit('publish', key, sendData);
}

// broadcast
AmqpChannel.prototype.broadcast = function(data) {
    data.serverFrom = this.uuid;
    data.serverTo   = "";
    this.publish('broadcast', true, data);
}

// send
AmqpChannel.prototype.send = function(uuid, data) {
    data.serverFrom = this.uuid;
    data.serverTo   = uuid;
    this.publish(uuid, true, data);
}

// broadcast client update
AmqpChannel.prototype.broadcastClientUpdate = function(service) {
    this.broadcast({type:AmqpChannel.DATA_TYPE.U, service:service});
}

// broadcast client disconnect
AmqpChannel.prototype.broadcastClientDisconnect = function(uuid) {
    this.broadcast({type:AmqpChannel.DATA_TYPE.D, uuid:uuid});
}

// send message
AmqpChannel.prototype.sendMessage = function(serverUuid, messageData) {
    var data = {};
    data.type        = AmqpChannel.DATA_TYPE.M;
    data.messageData = messageData;
    this.send(serverUuid, data);
}

// broadcast services request
AmqpChannel.prototype.broadcastServicesRequest = function() {
    this.broadcast({type:AmqpChannel.DATA_TYPE.SQ, services:mindlinkServer.findServices(mindlinkServer.uuid)});
}

// send services response
AmqpChannel.prototype.sendServicesResponse = function(serverUuid) {
    this.send(serverUuid, {type:AmqpChannel.DATA_TYPE.SR, services:mindlinkServer.findServices(mindlinkServer.uuid)});
}

// broadcast keepalive
AmqpChannel.prototype.broadcastKeepalive = function() {
    this.broadcast({type:AmqpChannel.DATA_TYPE.K});
}

// send whoareyou
AmqpChannel.prototype.sendWhoareyou = function(serverUuid) {
    this.send(serverUuid, {type:AmqpChannel.DATA_TYPE.W});
}

module.exports = new AmqpChannel();
