var rfb2 = require("rfb2");
var util = require("util");

var rfb = rfb2.createConnection({
  host: '192.168.56.101',
  port: 5901,
  password: 'savant'
});

rfb.on("connect", function () {
    sendKeys("Hello World!\n");

    setTimeout(function () {
        sendKeys("10 seconds.\n");
    }, 10000);

    setTimeout(function () {
        sendKeys("20 seconds.\n");
    }, 20000);

    setTimeout(function () {
        sendKeys("30 seconds.\n");
        sendKeys("Goodbye World!\n");
        rfb.end();
    }, 30000);
    
    setInterval(
        function () {
            rfb.requestUpdate(true, 0, 0, 1, 1);
        },
    10);
});

rfb.on('rect', function(rect) {
    console.log("rect");
});

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
        rfb.keyEvent(0xFFE1, 1);
        rfb.keyEvent(code, 1);
        rfb.keyEvent(code, 0);
        rfb.keyEvent(0xFFE1, 0);
    } else {
        // 0xFF0D is the X11 keysym for enter.
        if (c === '\n' || c === '\r') code = 0xFF0D;
        rfb.keyEvent(code, 1);
        rfb.keyEvent(code, 0);
    }
}

function isUpperCase(c) {
    return c.toLowerCase() !== c;
}