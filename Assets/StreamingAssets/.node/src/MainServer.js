var child_process = require("child_process");

var ws = require('websocket.io');
var server = ws.listen(8080,function () {
    console.log("Websocket Server start");
  }
);

var msgpack = require('msgpack-js');

var unity;
var children = {};

server.on('connection', function(client) {
    console.log('connection start');

    // クライアントからのメッセージ受信イベントを処理
    client.on('message', function(request) {
        var data = msgpack.decode(request);
        console.log("Unity -> Nodejs:"+data+",MODE:"+data.mode);

	//--------------Main area--------------
        switch (data.mode) {
            case "connect":
                unity = client;
                
                var message = {
                    mode : 'connected',
                    ver  : 'v0.10.28'
                }
                var bytedata = msgpack.encode(message);
                unity.send(bytedata,{binary:true});
                console.log("Nodejs -> Unity:"+bytedata);
                
                break;
            case "child":
                    var child = child_process.fork(data.js);
                    children[data.name] = {
                        "child" : child,
                        "cilent": client
                    }
                    children[data.name].child.on("message", function(msg) {
                       console.log(msg);
                       children[data.name].cilent.send(msg)    
                    });
                break;
            case "hello":
                console.log("Nodejs -> Unity:"+data.text);
                var message = {
                    mode : 'hello',
                    text : data.text
                }
                var bytedata = msgpack.encode(message);
                client.send(bytedata,{binary:true});                
                break;
            case 3:
                children[data.name].child.send(data.massage);
                break;
        }
	//--------------------------------------
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

