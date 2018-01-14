var config         = require('./config');
var logger         = require('./logger');
var MindlinkClient = require('./mindlink_client');
var appServer      = require('./app_server');

mindlinkClient1 = new MindlinkClient(config.mindlinkClient1);
mindlinkClient1.on('join', function() {
    var port = Math.floor(Math.random() * 1000 + 20000);
    mindlinkClient1.send({port:port.toString()});
    appServer.start(port);
});
mindlinkClient1.on('disconnect', function() {
    setTimeout(function() {
        mindlinkClient1.start();
    }, 3000);
});
//mindlinkClient1.on('data', function(data) {
//});
mindlinkClient1.start();
