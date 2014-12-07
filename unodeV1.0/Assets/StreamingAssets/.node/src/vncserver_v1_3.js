var rfb2 = require("rfb2");
var ws     = require('websocket.io');
var server = ws.listen(8080,function () {
    console.log("Websocket Server start");
});
var msgpack = require('msgpack-js');
var png = require('./node_modules/png/build/Release/png');
//var png = require('./node_modules/test/node-png/build/Release/png');
var util = require("util");
var vnc;

var image;

process.on('uncaughtException', function (err) {
    console.log('uncaughtException => ' + err);
});

function handleFrame(socket, rect, r) {
    console.log(util.format("rect-> [x:%d,y:%d]",rect.width,rect.height));
    //console.log(r);
   switch(rect.encoding) {
    case rfb2.encodings.raw:
            
       // rect.x, rect.y, rect.width, rect.height, rect.data
       // pixmap format is in r.bpp, r.depth, r.redMask, greenMask, blueMask, redShift, greenShift, blueShift
    case rfb2.encodings.copyRect:
       // pseudo-rectangle
       // copy rectangle from rect.src.x, rect.src.y, rect.width, rect.height, to rect.x, rect.y
    case rfb2.encodings.hextile:

   }    

    var rgb = new Buffer(rect.width * rect.height * 3, 'binary');
    rgb.fill();
    var offset = 0;

    console.log("Rect_data = "+rect.data.length);
    
    if (r.width == rect.width && r.height == rect.height) {
        for (var i = 0; i < rect.data.length; i += 2) {
            rgb[offset++] = rect.data[i+1];
            rgb[offset++] = rect.data[i+1];
            rgb[offset++] = rect.data[i];
        }        //code
        image = rgb;
    }else{
        for (var i = 0; i < rect.data.length; i += 2) {
            image[offset++] = rect.data[i+1];
            image[offset++] = rect.data[i+1];
            image[offset++] = rect.data[i];
        }          
    }
    
    console.log("R"+rgb[0]+",G"+rgb[1]+",B"+rgb[2]);
    

    var data = png.write_png_to_memory(r.width,r.height,image);
    var img  = Array.prototype.slice.call(new Uint8Array(data));
      
    var message = {
          mode: 'fream',
             x: rect.x,
             y: rect.y,
         width: rect.width,
        height: rect.height,
         image: img
    }
    
    var sendPack = msgpack.encode(message);
    
    socket.send(sendPack,{binary:true});
}

function pressKey(keysym) {
    setTimeout(
        function () {
            vnc.keyEvent(keysym, 1);
            console.log(util.format("%d key pressed", keysym));
        },
        1);
    setTimeout(
        function () {
            vnc.keyEvent(keysym, 0);
            console.log(util.format("%d key released", keysym));
        },
        2);
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

                vnc = rfb2.createConnection({
                  host: msg.config.host,
                  port: msg.config.port,
                  password: msg.config.password
                });

                vnc.on('rect', function(rect) {
                    console.log("rect");
                    handleFrame(client, rect, vnc);
                });
 
                vnc.on("error", function (error) {
                    console.log("errored");
                    console.log(error);
                });

                vnc.on("connect", function () {
                    setInterval(
                        function () {
                            vnc.requestUpdate(true, 0, 0, vnc.width, vnc.height);
                        },
                    100);
                });
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
        if (vnc != null) {
            vnc.end();
        }
    });
 
    // 通信がクローズしたときの処理
    client.on('close', function(){
        if (vnc != null) {
            vnc.end();
        }
        console.log('connection close');
    });
 
    // エラーが発生した場合
    client.on('error', function(err){
        console.log(err);
        console.log(err.stack);
    });
});


