var path = require('path')
var fs   = require('fs')

require('date-utils');



var ws = require('websocket.io');
var server = ws.listen(5000,function () {
    console.log("Websocket Server start");
});

var msgpack = require('msgpack-js');

var msg;

server.on('connection', function(client) {
    console.log('connection start');
    
    // クライアントからのメッセージ受信イベントを処理
    client.on('message', function(request) {
        msg = msgpack.decode(request);
        
        console.log('mode:'+msg.mode);
        switch (msg.mode) {
            case 1:
                var message = {
                    mode : 1,
                    ver  : 'v0.10.28',
                }
                var sendPack = msgpack.encode(message);
                client.send(sendPack,{binary:true});
                break;
            
            case 10:
                
                //var data = new Buffer('ggggggg','binary');
                var filePath  = path.join(__dirname, msg.name);
                var file_data = fs.readFileSync(filePath)
                var array     = Array.prototype.slice.call(new Uint8Array(file_data));
                
                var message = {
                    mode : 10,
                    str  : filePath,
                    data : array
                }
                
                //console.log(message.data);
                var sendPack = msgpack.encode(message);
                
                client.send(sendPack,{binary:true});
                //client.send("test");
                break;
            case 255:
                console.log('data:'+msg.data);
                var dt = new Date();
                var formatted = dt.toFormat("HH24時MI分SS秒");
                var message = {
                    mode : 255,
                    date : formatted,
                }
                var sendPack = msgpack.encode(message);
                client.send(sendPack,{binary:true});
                
                break;
            default:
                //client.send(request,{binary:true});
                console.log('mode:'+msg.mode);
        }
        
        msg = null;
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