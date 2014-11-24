var rfb = require('rfb2');
var ws = require('websocket.io');
var server = ws.listen(5000,function () {
    console.log("Websocket Server start");
});
var msgpack = require('msgpack-js');

var vnc = null;
var config;
var unity;



server.on('connection', function(client) {
    console.log('connection start');
    

    
    // クライアントからのメッセージ受信イベントを処理
    client.on('message', function(request) {
        var msg = msgpack.decode(request);

        switch (msg.mode) {
            case 'connect':
                var message = {
                    mode : 'connected',
                    ver  : 'v0.10.28',
                }
                var sendPack = msgpack.encode(message);
                client.send(sendPack,{binary:true});
                break;
            case 'init':
                console.log('init');
                unity  = client;
                config = msg.config;
                vnc    = createRfbConnection(config, unity);
                break;           
            case 'mouse':
                console.log('mouse');
                vnc.pointerEvent(msg.x, msg.y, msg.button);
                break;
            case 'keyboard':
                console.log('keyboard');
                vnc.keyEvent(msg.keyCode, msg.isDown);
                break;
            default:
                //client.send(request,{binary:true});
                console.log('mode:'+msg.mode);
        }
        
    });
 
    // クライアントが切断したときの処理
    client.on('disconnect', function(){
        console.log('connection disconnect');
    });
 
    // 通信がクローズしたときの処理
    client.on('close', function(){
        vnc.end();
        console.log('connection close');
    });
 
    // エラーが発生した場合
    client.on('error', function(err){
        console.log(err);
        console.log(err.stack);
    });
    
});





function createRfbConnection(config, socket) {
    console.log('host: ' + config.host + ' port:' + config.port + ' password: ' + config.password);
    var vnc = rfb.createConnection({
        host     : config.host,
        port     : config.port,
        password : config.password
    });
    addEventHandlers(vnc, socket);
    return vnc;
}

function addEventHandlers(r, socket) {
    r.on('connect', function () {
        console.log('successfully connected and authorised');
        console.log('remote screen name: ' + r.title + ' width:' + r.width + ' height: ' + r.height);
            
        var message = {
            mode  : 'init',
            title : vnc.title,
            width : vnc.width,
            height: vnc.height
        }
        var sendPack = msgpack.encode(message);
        socket.send(sendPack,{binary:true});        
    });
    r.on('rect', function (rect) {
        handleFrame(socket, rect, r);
    });
    r.on('clipboard', function(newPasteBufData) {
        console.log('remote clipboard updated!', newPasteBufData);
    });
}

function handleFrame(socket, rect, vnc) {
    var rgb    = new Buffer(rect.width * rect.height * 3, 'binary');
    var offset = 0;

    for (var i = 0; i < rect.data.length; i += 4) {
        rgb[offset++] = rect.data[i + 2];
        rgb[offset++] = rect.data[i + 1];
        rgb[offset++] = rect.data[i];
    }

    var message = {
        mode : 'frame',
        x: rect.x,
        y: rect.y,
        width: rect.width,
        height: rect.height,
        image: Array.prototype.slice.call(new Uint8Array(rgb))
    }
    
    var sendPack = msgpack.encode(message);
    socket.send(sendPack,{binary:true});     
}