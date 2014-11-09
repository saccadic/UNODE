var nodejtalk   = require('./node_modules/NodeJTalk/nodejtalk');
var path        = "./node_modules/NodeJTalk/";
var Dictionary  = "./node_modules/NodeJTalk/dic";
var Voice       = "./node_modules/NodeJTalk/voice\\mei_happy.htsvoice";
var Sampling    = 48000;
var file_name   = "voice.wav";
var file_path   = "../data/";
var text        = '吾輩は猫である';

//OpenJTalk------------------------------------------
process.on("message",function(request) {
    var msg = JSON.parse(request);
    console.log("NodeJTalk:" + msg.file);
    
    nodejtalk.setup(path+msg.dic,path+msg.voice,msg.sample,file_path + msg.file);

    nodejtalk.run(msg.text);

    nodejtalk.end();
    
    process.send("{\"mode\":3,\"voice\":true,\"file\":\""+file_name+"\"}");
});

//---------------------------------------------------
/*
//バイナリとしてBuffer変数にロードしてあれこれする準備----------
var fs = require("fs");
var buffer = fs.readFileSync( file_name );
console.log("Buffer size:"+buffer.length);
//---------------------------------------------------
*/
process.on("exit", function () {
    console.log("child exit");
});

/*
var sys = require('sys');
for(var i=0;i<buffer.length;i++){
    if (i % 15 == 0 && i>0) {
        console.log("");
    }
    sys.print(buffer[i]);
}
*/

