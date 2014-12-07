var ws = require('websocket.io');
var server = ws.listen(8080,function () {
    console.log("Websocket Server start");
});

var unity;
var r;
server.on('connection', function(client) {
    console.log('connection start');
    unity = client;
    // クライアントからのメッセージ受信イベントを処理
    client.on('message', function(request) {
        msg = JSON.parse(request);
        if (msg.mode==2) {
            r = createRfbConnection(msg.config, client);
        }
        //console.log(request);
    });
 
    // クライアントが切断したときの処理
    client.on('disconnect', function(){
        console.log('connection disconnect');
    });
 
    // 通信がクローズしたときの処理
    client.on('close', function(){
        console.log('connection close');
    });
 
    // エラーが発生した場合
    client.on('error', function(err){
        console.log(err);
        console.log(err.stack);
    });
});




var rfb = require('rfb2');
var socketio = require('socket.io').listen(5904);
//var Png = require('./node_modules/node-png/build/Release/png').Png;

var clients = [];

function createRfbConnection(config, socket) {
    console.log('host: ' + config.host + ' port:' + config.port + ' password: ' + config.password);
    var r = rfb.createConnection({
        host     : config.host,
        port     : config.port,
        password : config.password
    });
    addEventHandlers(r, socket);
    return r;
}

function addEventHandlers(r, socket) {
    r.on('connect', function () {
        console.log('successfully connected and authorised');
        console.log('remote screen name: ' + r.title + ' width:' + r.width + ' height: ' + r.height);
    
        socket.emit('init', {
            width: r.width,
            height: r.height
        });
        clients.push({
            socket: socket,
            rfb: r
        });
    });
    r.on('rect', function (rect) {
        handleFrame(socket, rect, r);
    });
    r.on('clipboard', function(newPasteBufData) {
        console.log('remote clipboard updated!', newPasteBufData);
    });
}

function handleFrame(socket, rect, r) {
    var rgb    = new Buffer(rect.width * rect.height * 3, 'binary');
    var offset = 0;

    for (var i = 0; i < rect.data.length; i += 4) {
        rgb[offset++] = rect.data[i + 2];
        rgb[offset++] = rect.data[i + 1];
        rgb[offset++] = rect.data[i];
    }
  //var image = new Png(rgb, r.width, r.height, 'rgb');
  //image = image.encodeSync();
    client.send(rgb);
    socket.emit('frame', {
        x: rect.x,
        y: rect.y,
        width: rect.width,
        height: rect.height,
        image: rgb
    });
}

console.log("Start-vncserver");
socketio.sockets.on('connection', function (socket) {
    console.log("New connection from " + socket.id);
    socket.on('init', function (config) {
        
        socket.on('mouse', function (evnt) {
            r.pointerEvent(evnt.x, evnt.y, evnt.button);
        });
        socket.on('keyboard', function (evnt) {
            r.keyEvent(evnt.keyCode, evnt.isDown);
        });
        socket.on('disconnect', function () {
            disconnectClient(socket);
        });
    });
});