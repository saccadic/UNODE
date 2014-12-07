rfb2 = require("rfb2");
util = require("util");

var device = rfb2.createConnection({
  host: '192.168.56.101',
  port: 5902,
  password: 'savant'
});

function pressKey(keysym) {
    setTimeout(
        function () {
            device.keyEvent(keysym, 1);
            console.log(util.format("%d key pressed", keysym));
        },
        100);
    setTimeout(
        function () {
            device.keyEvent(keysym, 0);
            console.log(util.format("%d key released", keysym));
        },
        200);
}

device.on('rect', function(rect) {
    console.log("rect");
    console.log( rect.width +","+rect.height);
    console.log( rect.data);
    
});

device.on("error", function (error) {
    console.log("errored");
    console.log(error);
});

device.on("connect", function () {
    setTimeout(
        function () {
            pressKey(0x0061);
            device.pointerEvent(10, 10, 0);
        },
        2000);
    setTimeout(
        function () {
            pressKey(0x0062);
             device.pointerEvent(50, 10, 0);
        },
        2500);
    setTimeout(
        function () {
            pressKey(0x0063);
             device.pointerEvent(0, 50, 1);
        },
        3000);

    setInterval(
        function () {
            device.requestUpdate(true, 0, 0, device.width, device.height);
        },
    10);
});