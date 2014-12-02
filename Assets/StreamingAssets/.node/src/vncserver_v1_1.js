var rfb    = require('rfb2');
var ws     = require('websocket.io');
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
                console.log('keyboard:'+msg.str);
                sendKeys(msg.str);
                //vnc.keyEvent(msg.keyCode, msg.isDown);
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
        password : config.password,
        //encodings: [rfb.encodings.raw, rfb.encodings.copyRect]
    });
    addEventHandlers(vnc, socket);
    return vnc;
}

function addEventHandlers(r, socket) {
    r.on('connect', function () {
        console.log('successfully connected and authorised');
        console.log('remote screen name: ' + r.title + ' width:' + r.width + ' height: ' + r.height);
        r.updateClipboard('send text to remote clipboard');    
        var message = {
            mode  : 'init',
            title : vnc.title,
            width : vnc.width,
            height: vnc.height
        }
        var sendPack = msgpack.encode(message);
        socket.send(sendPack,{binary:true})
        
        r.on('rect', function (rect) {
                console.log("rect:"+rect.encoding);
                handleFrame(socket, rect, r);
        });
        r.on('clipboard', function(newPasteBufData) {
                console.log('remote clipboard updated!', newPasteBufData);
        });
        r.on("error", function (error) {
            console.log("errored");
            console.log(error);
        });
        setInterval(
            function () {
                r.requestUpdate(true, 0, 0, r.width, r.height);
            },
        10);  
    });
}

function handleFrame(socket, rect, r) {    
    var red   = new Buffer(rect.width * rect.height, 'binary');
    var green = new Buffer(rect.width * rect.height, 'binary');
    var blue  = new Buffer(rect.width * rect.height, 'binary');
    
    var offset = 0;
    
    for (var i = 0; i < rect.width * rect.height; i += 4) {
            red[i] = rect.data[i + 2];
          green[i] = rect.data[i + 1];
           blue[i] = rect.data[i];
    }

    var message = {
        mode : 'fream',
        x: rect.x,
        y: rect.y,
        width: rect.width,
        height: rect.height,
        image: {
                red  : Array.prototype.slice.call(new Uint8Array(red)),
                green: Array.prototype.slice.call(new Uint8Array(green)),
                blue : Array.prototype.slice.call(new Uint8Array(blue)),
        }
    }
    console.log("send fream:"+rect.data.length);
    var sendPack = msgpack.encode(message);
    socket.send(sendPack,{binary:true});     
}

var shiftMap = {
    '!': '1',
    '@': '2',
    '#': '3',
    '$': '4',
    '%': '5',
    '^': '6',
    '&': '7',
    '*': '8',
    '(': '9',
    ')': '0'
};

function sendKeys(string) {
    string.split("").forEach(keyPress);
}

function keyPress(c) {
    var code = c.charCodeAt(0);

    if (isUpperCase(c) || shiftMap.hasOwnProperty(c)) {
        // 0xFFE1 is the X11 keysym for shift.
        vnc.keyEvent(0xFFE1, 1);
        vnc.keyEvent(code, 1);
        vnc.keyEvent(code, 0);
        vnc.keyEvent(0xFFE1, 0);
    } else {
        // 0xFF0D is the X11 keysym for enter.
        if (c === '\n' || c === '\r') code = 0xFF0D;
        vnc.keyEvent(code, 1);
        vnc.keyEvent(code, 0);
    }
}

function isUpperCase(c) {
    return c.toLowerCase() !== c;
}