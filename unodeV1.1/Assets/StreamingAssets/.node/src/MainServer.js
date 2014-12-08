var child_process = require("child_process");
var ws = require('websocket.io');
var server = ws.listen(8080,function () {
    console.log("Websocket Server start");
  }
);

var msgpack = require('msgpack-js');

var children = {};

function sendTOunity(client,message){
    try{
        var bytedata = msgpack.encode(message);
        client.send(bytedata,{binary:true});
        console.log("Nodejs -> Unity:",bytedata);
    }catch(e){
        console.log("Error:",e);
    }
}

server.on('connection', function(client) {
    console.log('connection start:'+client.adress);
    
    // クライアントからのメッセージ受信イベントを処理
    client.on('message', function(request) {
        var data = msgpack.decode(request);
        //console.log("Unity -> Nodejs:",data);

	//--------------Main area--------------
        switch (data.mode) {
            case "connect":
                var message = {
                    mode : 'connected',
                    ver  : 'v0.10.28'
                }
                sendTOunity(client,message);
                break;
            case "child":
                console.log("Run:" + data.name);
                if (data.regist != null) {
                    var child = child_process.fork("./"+data.js);
                    children[data.name] = {
                        "child" : child,
                        "cilent": client
                    }
                    children[data.name].child.on("message", function(msg) {
                       console.log(msg);
                       sendTOunity(children[msg.name].cilent,msg); 
                    });
                }else{
                    children[data.name].child.send(data.options);
                }
                break;
            case "transform":
                //console.log(data)
                break;
            case "echo":
                var message = {
                    mode : data.mode,
                    text : data.text
                }
                sendTOunity(client,message);
                break;
            case "exit":
                process.exit();
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

