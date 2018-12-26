
const fetch = require('node-fetch');
const Speaker = require('audio-speaker/stream');
const Generator = require('audio-generator/stream');

const bufferSize = 44100;
const stereoBufferSize = bufferSize * 2;

const url = 'http://127.0.0.1:50300?count=' + bufferSize;
console.log('GET url is: ' + url);

let wasSuccessful = false;
const fillStereoBuffer = (setBuffer) => {
	fetch(url, { method: 'GET' })
		.catch(err => {
			if (wasSuccessful) {
				console.log("Could not get audio data from server. Error: " + err);
			}
			wasSuccessful = false;
		})
		.then(res => {
			if (wasSuccessful === false) {
				console.log("Audio data successfully fetched from server");
				wasSuccessful = true;
			} else {
				if (res === undefined || !res.ok) {
					console.log("Could not get audio data from server. Response: " + res);
					wasSuccessful = false;
				}
			}

			return res;
		})
		.then(res => res.arrayBuffer())
		.then(arrayBuffer => {
			// var values = Array.from({length: stereoBufferSize}, () => Math.random());
			// console.log(values.length);
			let values = new Float64Array(arrayBuffer);
			setBuffer(values);
		});
}

let currentFrontBuffer = new Array(stereoBufferSize);
let currentBackBuffer = currentFrontBuffer;
let currentIndex = 0;

Generator(time => {
	
	// stereo
	currentIndex += 2;

	// flip buffers
	if (currentIndex > currentFrontBuffer.length) {
		//console.log("Flip 'n fill (at time: " + time);

		currentFrontBuffer = currentBackBuffer;
		currentIndex = 0;
		
		fillStereoBuffer(newBuffer => currentBackBuffer = newBuffer);
	}

	return [
		currentFrontBuffer[currentIndex],
		currentFrontBuffer[currentIndex + 1]];
	// //panned unisson effect
	// var tau = Math.PI * 2;
	// return [Math.sin(tau * time * 441), Math.sin(tau * time * 439)];
})
.pipe(Speaker({
	//PCM input format defaults, optional.
	//byteOrder: 'LE',
	//signed: true,
	//interleaved: true,
	channels: 2,          // 2 channels
	bitDepth: 16,         // 16-bit samples
	sampleRate: 44100,    // 44,100 Hz sample rate
	float: false,
	samplesPerFrame: bufferSize
	// device: 'hw:0,0'
  }));
