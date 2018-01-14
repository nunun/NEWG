var net          = require('net');
var EventEmitter = require('events').EventEmitter;
var assert       = require('assert');
var util         = require('util');
var config       = require('./config');
var logger       = require('./logger');

// constructor
function AppServer(port) {
    this.init();
}
util.inherits(AppServer, EventEmitter);

// init
AppServer.prototype.init = function() {
    this.tcpServer = null; // tcp server
    this.port      = null; // server port
};

// start
AppServer.prototype.start = function(port) {
    logger.appServer.debug('start.');
    var self = this;
    self.port = port;

    // tcp server
    logger.appServer.info('listening on tcp port ' + self.port + '.');
    self.tcpServer = net.createServer(function(client) {
        var clientName = client.remoteAddress + ':' + client.remotePort;
        logger.appServer.info('client connected. (' + clientName + ')');
    }).listen(self.port, function() {
        logger.appServer.info('listen started.');
    });
}

module.exports = new AppServer();

// // public channel protocol
// client.ok = function(data) {
//     client.send(true, data);
// }
// client.ng = function(data) {
//     client.send(false, data);
// }
// client.send = function(ok, data) {
//     if (!data) {
//         data = ok;
//         ok = true;
//     }
//     if (typeof(data) === 'boolean') {
//         ok = data;
//         data = {};
//     }
//     if (typeof(data) === 'string') {
//         data = {message: data};
//     }
//     data.ok = (ok)? true : false;
//     var sendData = JSON.stringify(data);
//     client.emit('send', sendData);
// }
//
//         // send
//         client.on('send', function(sendData) {
//             logger.appServer.debug(mindlinkServer.uuid + ': ' + client.uuid +': send: sendData[' + sendData + '] addr[' + client.remoteAddress + '] port[' + client.remotePort + ']');
//             client.write(sendData);
//         });
//
//         // on data
//         client.on('data', function(receivedData){
//             logger.appServer.debug(mindlinkServer.uuid + ': ' + client.uuid +': recv: receivedData[' + receivedData + '] addr[' + client.remoteAddress + '] port[' + client.remotePort + ']');
//
//             // null?
//             if (!receivedData) {
//                 return;
//             }
//
//             // empty?
//             receivedData = trim(receivedData);
//             if (receivedData == '') {
//                 return;
//             }
//
//             // parse receivedData as json into data
//             var data = null;
//             try {
//                 data = JSON.parse(receivedData);
//             } catch (e) {
//                 client.ng('json parse fail (' + receivedData + ')');
//                 return;
//             }
//
//             // handle command
//             self.emit('data', client, data);
//         });
//
//         // on close
//         client.on('close', function(){
//             logger.appServer.info('client disconnected. (' + clientName + ')');
//             self.emit('leave', client);
//         });
//
// , function() {
//         self.emit('start');
//     }
