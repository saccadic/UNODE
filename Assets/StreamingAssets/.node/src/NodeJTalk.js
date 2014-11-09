var nodejtalk   = require('./node_modules/NodeJTalk/nodejtalk');
var Dictionary  = "./node_modules/NodeJTalk/dic";
var Voice       = "./node_modules/NodeJTalk/voice\\mei_happy.htsvoice";
var Sampling    = 48000;
var file_name   = "../data/voice.wav";
var text        = '吾輩は猫である';

//OpenJTalk------------------------------------------
process.on("message",function(msg) {
    console.log("NodeJTalk:" + msg);
    
    nodejtalk.setup(Dictionary,Voice,Sampling,file_name);

    nodejtalk.run(msg);

    nodejtalk.end();
    
    process.send("{\"mode\":3,\"voice\":true}");
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

