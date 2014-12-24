var WebSocket = require('ws');
var ws = new WebSocket('ws://172.20.10.2:8080');

var i=0;
ws.on('open', function open() {
    setInterval(function() {
        ws.send(i++ +"");
    }, 1000);
  
});

ws.on('message', function(data, flags) {
  console.log(data);
});