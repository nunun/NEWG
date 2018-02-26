var EventEmitter = require('events').EventEmitter;
var assert       = require('assert');
var util         = require('util');
var jspath       = require('jspath');
var uuid         = require('uuid/v1');
var config       = require('services-library').config;
var logger       = require('services-library').logger;

// constructor
function MindlinkServer() {
    this.init();
}
util.inherits(MindlinkServer, EventEmitter);

// init
MindlinkServer.prototype.init = function() {
    this.servers         = {};     // known servers on amqp channel (include current server)
    this.clients         = {};     // connected clients on public channel
    this.services        = {};     // discovered services on public channel and amqp channel
    this.sendIntervalId  = null;   // send interval id
    this.checkIntervalId = null;   // check interval id
    this.uuid            = uuid(); // uuid
    this.started         = false;  // started flag

    // set this uuid to knonwn server and current server.
    this.updateServers(this.uuid, true, true);
};

// start
MindlinkServer.prototype.start = function() {
    logger.mindlinkServer.debug(this.uuid + ': start.');
    assert.ok(!this.started);
    this.started = true;
    var self = this;
    self.emit('start');
}

// apply jspath
MindlinkServer.prototype.applyJSPath = function(jspathString) {
    return jspath.apply(jspathString, this.services);
}

// find services
MindlinkServer.prototype.findServices = function(serverUuid) {
    var services = []
    for (var i in this.services) {
        var s = this.services[i];
        if (!serverUuid || s._serverUuid == serverUuid) {
            services.push(s)
        }
    }
    return services;
}

// find aliased services
MindlinkServer.prototype.findAliasedServices = function(alias) {
    var aliasedServices = null;
    var broadcast = false;
    if (alias.startsWith("*")) {
        broadcast = true;
        alias = alias.substring(1);
    }
    for (var i in this.services) {
        var s = this.services[i];
        if (s.alias && s.alias == alias) {
            if (!aliasedServices) {
                aliasedServices = [];
            }
            aliasedServices.push(s);
        }
    }
    if (!broadcast && aliasedServices && aliasedServices.length > 0) {
        aliasedServices = [aliasedServices[Math.random() * (aliasedServices.length - 1)]];
    }
    return aliasedServices;
}

// remove services
MindlinkServer.prototype.removeServices = function(serverUuid) {
    for (var i in this.services) {
        var s = this.services[i];
        if (!serverUuid || s.serverUuid == serverUuid) {
            delete this.services[i];
        }
    }
}

// update servers
MindlinkServer.prototype.updateServers = function(serverUuid, isKnown, isCurrentServer) {
    assert.ok(serverUuid, 'invalid serverUuid. (' + serverUuid +')');
    var s = this.servers[serverUuid];
    var f = (s != undefined); // is known?
    if (!f) {
        s = {
            uuid:            serverUuid,
            keepaliveCount:  0,    // -> reset keepalive count
            state:           'up', // -> force 'up'
            isCurrentServer: (isCurrentServer || false)
        };
        this.servers[serverUuid] = s;
    } else {
        s.keepaliveCount = 0;    // -> reset keepalive count
        s.state          = 'up'; // -> force 'up'
    }
    if (!f && !isKnown && !isCurrentServer) {
        this.emit('whoareyou', serverUuid);
    }
    return s;
}

// start keepalive
MindlinkServer.prototype.startKeepalive = function() {
    var self            = this;
    var keepaliveConfig = config.mindlinkServer.servers.keepalive;
    var sendInterval    = keepaliveConfig.sendInterval;
    var checkInterval   = keepaliveConfig.checkInterval;
    var downCount       = keepaliveConfig.downCount;
    var deleteCount     = keepaliveConfig.deleteCount;

    // stop keepalive
    self.stopKeepalive();

    // send interval
    setTimeout(function() {
        self.sendIntervalId = setInterval(function() {
            self.emit('keepalive');
        }, sendInterval);
    }, (Math.random() * sendInterval));

    // check interval
    setTimeout(function() {
        self.checkIntervalId = setInterval(function() {
            logger.mindlinkServer.debug(self.uuid + ': check keepalive ...');
            for (var uuid in self.servers) {
                var s = self.servers[uuid];
                if (s.isCurrentServer) {
                    continue; // exclude current server from keepalive work.
                }
                s.keepaliveCount++;

                // remove services
                if (s.keepaliveCount >= deleteCount) {
                    logger.mindlinkServer.debug(self.uuid + ': keepalive timeout: server removed. (' + s.uuid + ')');
                    delete self.servers[uuid]; // delete server
                    self.removeServices(uuid);
                    return;
                }

                // down
                if (s.keepaliveCount == downCount) {
                    logger.mindlinkServer.debug(self.uuid + ': keepalive timeout: server down. (' + s.uuid + ')');
                    s.state = "down"; // down server
                    return;
                }
            }
        }, checkInterval);
    }, sendInterval);
}

// stop keepalive
MindlinkServer.prototype.stopKeepalive = function() {
    if (this.sendIntervalId) {
        clearInterval(this.sendIntervalId);
    }
    if (this.checkIntervalId) {
        clearInterval(this.checkIntervalId);
    }
    this.sendIntervalId  = null;
    this.checkIntervalId = null;
}

module.exports = new MindlinkServer();
