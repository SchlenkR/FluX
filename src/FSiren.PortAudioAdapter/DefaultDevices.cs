using PortAudioSharp;

namespace FSiren.PortAudioAdapter
{
    public class DefaultDevices
    {
        public DefaultDevices(PortAudio.PaDeviceInfo defaultInputDevice, PortAudio.PaDeviceInfo defaultOutputDevice)
        {
            DefaultInputDevice = defaultInputDevice;
            DefaultOutputDevice = defaultOutputDevice;
        }

        public PortAudio.PaDeviceInfo DefaultInputDevice { get; }
        public PortAudio.PaDeviceInfo DefaultOutputDevice { get; }
    }
}