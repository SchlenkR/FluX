
function play() {
    var bufferSize = 4096;
    var audioContext = new AudioContext();
    var whiteNoise = audioContext.createScriptProcessor(bufferSize, 1, 1);
    whiteNoise.onaudioprocess = function(e) {
        var output = e.outputBuffer.getChannelData(0);
        for (var i = 0; i < bufferSize; i++) {
            output[i] = Math.random() * 2 - 1;
        }
    }
    
    whiteNoise.connect(audioContext.destination);
};

$(function() {

    $("#play").click(play);
});
