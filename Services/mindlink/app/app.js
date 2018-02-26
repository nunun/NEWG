var util             = require('util');
var config           = require('services-library').config;
var logger           = require('services-library').logger;
var mindlinkServer   = require('./mindlink_server');
var amqpChannel      = require('./amqp_channel');
var publicChannel    = require('./public_channel');
var debugHttpChannel = require('./debug_http_channel');

// mindlink server
// manage information for servers, clients, and services.
mindlinkServer.on('start', function() {
    logger.mindlinkServer.info('uuid is "' + mindlinkServer.uuid + '".');
    amqpChannel.start(mindlinkServer.uuid);
});
mindlinkServer.on('whoareyou', function(uuid) {
    logger.mindlinkServer.debug(mindlinkServer.uuid + ': unknown server detected. send "whoareyou" ... (' + uuid + ')');
    amqpChannel.sendWhoareyou(uuid);
});
mindlinkServer.on('keepalive', function() {
    amqpChannel.broadcastKeepalive();
});

// amqp channel
// handle server and service discovery requests and keepalives.
amqpChannel.on('start', function() {
    publicChannel.start();
});
amqpChannel.on('data', function(data) {
    switch(data.type) {
    case amqpChannel.DATA_TYPE.U: // client update
       logger.amqpChannel.debug(mindlinkServer.uuid + ': U: service[' + data.service + '] serverFrom[' + data.serverFrom + ']');
       data.mindlinkServer = mindlinkServer.updateServers(data.service.serverUuid);
       mindlinkServer.services[data.service.clientUuid] = data.service;
       break;
    case amqpChannel.DATA_TYPE.D: // client disconnect
       logger.amqpChannel.debug(mindlinkServer.uuid + ': D: uuid[' + data.uuid + '] serverFrom[' + data.serverFrom + ']');
       delete mindlinkServer.services[data.uuid];
       break;
    case amqpChannel.DATA_TYPE.M: // send message
       logger.amqpChannel.debug(mindlinkServer.uuid + ': M: messageData[' + util.inspect(data.messageData, {depth:null,breakLength:Infinity}) + ']');
       if (data.messageData) {
           var messageData  = data.messageData;
           var remoteHeader = messageData.remote;
           if (mindlinkServer.clients[remoteHeader.to] != undefined) {
               mindlinkServer.clients[remoteHeader.to].send(messageData); // NOTE send to client on *this* server
           } else {
               logger.amqpChannel.error('no uuid to foward message. (' + remoteHeader.to + ')');
           }
       } else {
           logger.amqpChannel.error('no data to foward message. (' + util.inspect(data, {depth:null,breakLength:Infinity}) + ')');
       }
       break;
    case amqpChannel.DATA_TYPE.SQ: // services requset
       if (data.type == amqpChannel.DATA_TYPE.SQ) {
           logger.amqpChannel.debug(mindlinkServer.uuid + ': SQ: services[' + data.services + '] serverFrom[' + data.serverFrom + ']');
       }
       //FALLTHROUGH
    case amqpChannel.DATA_TYPE.SR: // services response
       if (data.type == amqpChannel.DATA_TYPE.SR) {
           logger.amqpChannel.debug(mindlinkServer.uuid + ': SR: services[' + data.services + '] serverFrom[' + data.serverFrom + ']');
       }
       mindlinkServer.updateServers(data.serverFrom, true); // NOTE add sender server as 'known' and 'up'
       mindlinkServer.removeServices(data.serverFrom);
       for (var i in data.services) {
           var s = data.services[i];
           s.server = mindlinkServer.updateServers(s.serverUuid);
           mindlinkServer.services[s._clientUuid] = s;
       }
       if (data.type == amqpChannel.DATA_TYPE.SQ) {
           amqpChannel.sendServicesResponse(data.serverFrom);
       }
       break;
    case amqpChannel.DATA_TYPE.K: // keepalive
       logger.amqpChannel.debug(amqpChannel.uuid + ': K: serverFrom[' + data.serverFrom + ']');
       mindlinkServer.updateServers(data.serverFrom);
       break;
    case amqpChannel.DATA_TYPE.W: // whoareyou
       logger.amqpChannel.debug(amqpChannel.uuid + ': W: serverFrom[' + data.serverFrom + ']');
       amqpChannel.sendServicesResponse(data.serverFrom);
       break;
    default:
       logger.amqpChannel.error('unknown type. (' + data.type + ')');
       break;
    }
});

// public channel
// standard tcp server for handle connected client and whom request.
publicChannel.on('start', function() {
    amqpChannel.broadcastServicesRequest();
    mindlinkServer.startKeepalive();
    debugHttpChannel.start();
});
publicChannel.on('connect', function(mindlinkClient) {
    mindlinkServer.clients[mindlinkClient.uuid] = mindlinkClient;
});
publicChannel.on('disconnect', function(mindlinkClient) {
    delete mindlinkServer.clients[mindlinkClient.uuid];
    delete mindlinkServer.services[mindlinkClient.uuid];
    amqpChannel.broadcastClientDisconnect(mindlinkClient.uuid);
});
publicChannel.on('data', function(mindlinkClient, data) {
    switch (data.type) {
    case publicChannel.DATA_TYPE.S: // update status of service
        logger.publicChannel.debug(mindlinkServer.uuid + ': ' + mindlinkClient.uuid + ': S: data[' + util.inspect(data, {depth:null,breakLength:Infinity}) + ']');
        try {
            var service = data.data;
            service.clientUuid = mindlinkClient.uuid;
            service.serverUuid = mindlinkServer.uuid;
            service.server     = mindlinkServer.updateServers(mindlinkServer.uuid);
            mindlinkServer.services[mindlinkClient.uuid] = service;
            amqpChannel.broadcastClientUpdate(service);
            mindlinkClient.res(data, service);
        } catch(e) {
            mindlinkClient.res(data, 'failed to update service information (' + e.toString() + ')');
        }
        break;
    case publicChannel.DATA_TYPE.Q: // query services with jspath (ex: ".*{clientUuid === xxxxxx}")
        logger.publicChannel.debug(mindlinkServer.uuid + ': ' + mindlinkClient.uuid + ': Q: data[' + util.inspect(data, {depth:null,breakLength:Infinity}) + ']');
        try {
            var query = data.data;
            var responseData = {};
            responseData.services = mindlinkServer.applyJSPath(query.jspath);
            mindlinkClient.res(data, responseData);
        } catch(e) {
            mindlinkClient.res(data, 'jspath failed (' + e.toString() + ')');
        }
        break;
    case publicChannel.DATA_TYPE.M: // send message to remote client
        logger.publicChannel.debug(mindlinkServer.uuid + ': ' + mindlinkClient.uuid + ': M: data[' + util.inspect(data, {depth:null,breakLength:Infinity}) + ']');
        var remoteHeader = data.remote;
        remoteHeader.from = mindlinkClient.uuid;
        var sendFail = false;
        var aliasedServices = mindlinkServer.findAliasedServices(remoteHeader.to); // NOTE solve alias
        do {
            remoteHeader.to = (aliasedServices && aliasedServices.length > 0)? aliasedServices.shift().clientUuid : remoteHeader.to;
            if (mindlinkServer.clients[remoteHeader.to] != undefined) {
                mindlinkServer.clients[remoteHeader.to].send(data); // NOTE send message to client on *this* server
            } else if (mindlinkServer.services[remoteHeader.to] != undefined) {
                var s = mindlinkServer.services[remoteHeader.to];
                amqpChannel.sendMessage(s.serverUuid, data); // NOTE send message to client on *other* server
            } else {
                if (!sendFail) {
                    sendFail = true;
                    logger.publicChannel.debug('no uuid to send message (' + remoteHeader.to + ')');
                    mindlinkClient.res(data, 'no uuid to send message (' + remoteHeader.to + ')');
                }
            }
        } while(aliasedServices && aliasedServices.length > 0)
        break;
    default:
        mindlinkClient.res(data, 'unknown type (' + data.type + ')');
        break;
    }
});

// debug http channel
// provide mindlink WebUI.
debugHttpChannel.on('start', function() {
    // nothing to do yet.
});
debugHttpChannel.on('request', function(req, res) {
    res.writeHead(200, {'Content-Type': 'application/json'});
    var responseText = "";
    responseText += '{"uuid": "'   + mindlinkServer.uuid                     + '",';
    responseText += '"servers": '  + JSON.stringify(mindlinkServer.servers)  + ',';
    responseText += '"services": ' + JSON.stringify(mindlinkServer.services) + '}';
    res.end(responseText);
});

// start mindlink server
mindlinkServer.start();
