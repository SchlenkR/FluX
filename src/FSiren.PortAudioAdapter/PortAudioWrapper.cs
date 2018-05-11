using System;
using PortAudioSharp;

namespace FSiren.PortAudioAdapter
{
    public delegate Tuple<double[], double[]> ProcessAudio(int bufferSize);

    public class PortAudioWrapper : IDisposable
    {
        private readonly int _bufferSize;
        private readonly int _sampleRate;
        private readonly PortAudio.PaDeviceInfo _inputDeviceInfo;
        private readonly PortAudio.PaDeviceInfo _outputDeviceInfo;
        private readonly ProcessAudio _processAudio;

        private bool _disposed;
        private IntPtr _stream;

        public PortAudioWrapper(
            int sampleRate,
            int bufferSize,
            PortAudio.PaDeviceInfo inputDevice,
            PortAudio.PaDeviceInfo outputDevice,
            ProcessAudio processAudio)
        {
            _sampleRate = sampleRate;
            _bufferSize = bufferSize;
            _inputDeviceInfo = inputDevice;
            _outputDeviceInfo = outputDevice;
            _processAudio = processAudio;

            PortAudioExtensions.Initialize();
            _stream = StreamOpen();
            PortAudioExtensions.ErrorCheck(PortAudio.Pa_StartStream(_stream));
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                PortAudioExtensions.ErrorCheck(PortAudio.Pa_Terminate());
                PortAudioExtensions.ErrorCheck(PortAudio.Pa_StopStream(_stream));
                PortAudioExtensions.ErrorCheck(PortAudio.Pa_CloseStream(_stream));
                _stream = new IntPtr(0);
            }

            _disposed = true;
            GC.SuppressFinalize(this);
        }

        ~PortAudioWrapper()
        {
            Dispose();
        }

        private IntPtr StreamOpen()
        {
            var userData = new IntPtr(0);
            var inputParameters = _inputDeviceInfo.Equals(PortAudioExtensions.NoDevice)
                ? null
                : new PortAudio.PaStreamParameters
                {
                    channelCount = _inputDeviceInfo.maxInputChannels,
                    device = _inputDeviceInfo.DeviceNumber(),
                    sampleFormat = PortAudio.PaSampleFormat.paFloat32,
                    suggestedLatency = _inputDeviceInfo.defaultLowInputLatency
                };
            var outputParameters = _outputDeviceInfo.Equals(PortAudioExtensions.NoDevice)
                ? null
                : new PortAudio.PaStreamParameters
                {
                    channelCount = _outputDeviceInfo.maxOutputChannels,
                    device = _outputDeviceInfo.DeviceNumber(),
                    sampleFormat = PortAudio.PaSampleFormat.paFloat32,
                    suggestedLatency = _outputDeviceInfo.defaultLowOutputLatency
                };

            PortAudioExtensions.ErrorCheck(
                PortAudio.Pa_OpenStream(
                    out var stream,
                    inputParameters,
                    outputParameters,
                    _sampleRate,
                    (uint)_bufferSize,
                    PortAudio.PaStreamFlags.paNoFlag,
                    AudioCallback,
                    userData));

            return stream;
        }

        private PortAudio.PaStreamCallbackResult AudioCallback(
            IntPtr audioInputBuffer,
            IntPtr audioOutputBuffer,
            uint blockSize,
            ref PortAudio.PaStreamCallbackTimeInfo timeInfo,
            PortAudio.PaStreamCallbackFlags statusFlags,
            IntPtr userData)
        {
            var outputChannelCount = _outputDeviceInfo.maxOutputChannels;

            // initialize input buffer
            var inputChannelCount = _inputDeviceInfo.maxInputChannels;
            var inBuffers = new double[inputChannelCount][];
            for (var n = 0; n < inputChannelCount; n++)
                inBuffers[n] = new double[blockSize];

            var outBuffers = _processAudio((int)blockSize);

            unsafe
            {
                // Fill audio buffer for all channels, multiply samples by volume range [0, 1]
                var audioBuffer = (float*)audioOutputBuffer.ToPointer();

                var i = 0;
                var j = 0;
                while (i < blockSize * outputChannelCount)
                {
                    // always stereo
                    for (var k = 0; k < outputChannelCount; k++)
                    {
                        //audioBuffer[i] = (float)outBuffers[k][j];
                        //i++;

                        audioBuffer[i] = (float)outBuffers.Item1[j];
                        i++;
                        audioBuffer[i] = (float)outBuffers.Item2[j];
                        i++;
                    }

                    j++;
                }
            }

            return PortAudio.PaStreamCallbackResult.paContinue;
        }
    }
}