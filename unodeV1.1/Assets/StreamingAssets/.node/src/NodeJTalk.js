
var nodejtalk   = require('./node_modules/NodeJTalk/nodejtalk');
var path        = "./node_modules/NodeJTalk/";
var Dictionary  = "./node_modules/NodeJTalk/dic";
var Voice       = "./node_modules/NodeJTalk/voice\\mei_happy.htsvoice";
var Sampling    = 48000;
var file_name   = "voice.wav";
var file_path   = "../data/";
var text        = '吾輩は猫である';


//OpenJTalk------------------------------------------
process.on("message",function(msg) {
    console.log("NodeJTalk@SYSTEM");
    
    nodejtalk.setup(path+msg.dic,path+msg.voice,msg.sample,file_path + msg.file);

    nodejtalk.run(msg.text);

    nodejtalk.end();
    
    var date = {
        "mode" : "talking",
        "name" : msg.name
    }
    
    process.send(date);
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

