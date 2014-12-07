var rfb = require('rfb2');
var r = rfb.createConnection({
  host: '192.168.56.101',
  port: 5901,
  password: 'savant'
});

r.on('connect', function() {
  console.log('successfully connected and authorised');
  console.log('remote screen name: ' + r.title + ' width:' + r.width + ' height: ' + r.height);
  r.updateClipboard('send text to remote clipboard');
});

// screen updates
r.on('rect', function(rect) {
    console.log("rect");
    
   switch(rect.encoding) {
   case rfb.encodings.raw:
        console.log("raw");
        
      // rect.x, rect.y, rect.width, rect.height, rect.data
      // pixmap format is in r.bpp, r.depth, r.redMask, greenMask, blueMask, redShift, greenShift, blueShift
      break;
   case rfb.encodings.copyRect:
      // pseudo-rectangle
      // copy rectangle from rect.src.x, rect.src.y, rect.width, rect.height, to rect.x, rect.y
      break;
   case rfb.encodings.hextile:
      // not fully implemented
      //rect.on('tile', handleHextileTile); // emitted for each subtile
      break;
   }
});

r.on('clipboard', function(newPasteBufData) {
  console.log('remote clipboard updated!', newPasteBufData);
});
r.on('bell', console.log.bind(null, 'Bell!!'));


process.stdin.on('data', function (chunk) {
  r.pointerEvent(10, 10, 0);
});










