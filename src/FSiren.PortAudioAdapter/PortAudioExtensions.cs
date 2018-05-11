using System;
using System.Collections.Generic;
using System.Linq;
using PortAudioSharp;

namespace FSiren.PortAudioAdapter
{
    public static class PortAudioExtensions
    {
        static PortAudioExtensions()
        {
            NoDevice = new PortAudio.PaDeviceInfo
            {
                defaultSampleRate = 44100,
                hostApi = -1,
                name = "-- none --",
            };
        }

        public static readonly PortAudio.PaDeviceInfo NoDevice;

        public static void ErrorCheck(PortAudio.PaError errorCode)
        {
            if (errorCode != PortAudio.PaError.paNoError)
                throw new Exception("Error: " + errorCode);
        }

        public static PortAudio.PaHostApiInfo GetDefaultApi()
        {
            Initialize();

            var hostApiCount = PortAudio.Pa_GetHostApiCount();
            var desiredHostApis = Enumerable
                .Range(0, hostApiCount)
                .Select(PortAudio.Pa_GetHostApiInfo)
                .Where(it => it.deviceCount > 0)
                .Where(it =>
                    it.type == PortAudio.PaHostApiTypeId.paASIO
                        || it.type == PortAudio.PaHostApiTypeId.paDirectSound)
                .OrderByDescending(it => it.type) // ASIO == 3, DirectSOund == 1
                .ToList();

            return desiredHostApis.Any()
                ? desiredHostApis.First()
                : PortAudio.Pa_GetHostApiInfo(PortAudio.Pa_GetDefaultHostApi());
        }

        public static IEnumerable<PortAudio.PaHostApiInfo> GetHostApiInfos()
        {
            var hostApiCount = PortAudio.Pa_GetHostApiCount();
            return Enumerable
                .Range(0, hostApiCount)
                .Select(PortAudio.Pa_GetHostApiInfo)
                .ToList();
        }

        public static int ApiNumber(this PortAudio.PaHostApiInfo that)
        {
            var hostApiCount = PortAudio.Pa_GetHostApiCount();
            for (var i = 0; i < hostApiCount; i++)
            {
                var device = PortAudio.Pa_GetHostApiInfo(i);
                if (device.name == that.name)
                    return i;
            }

            throw new Exception("Unknown host api: " + that.name);
        }

        public static void Initialize()
        {
            ErrorCheck(PortAudio.Pa_Initialize());
        }

        public static IEnumerable<PortAudio.PaDeviceInfo> GetDevices()
        {
            var devicesCount = PortAudio.Pa_GetDeviceCount();
            return Enumerable
                .Range(0, devicesCount)
                .Select(PortAudio.Pa_GetDeviceInfo)
                .ToList();
        }

        public static IEnumerable<PortAudio.PaDeviceInfo> GetDevices(
            this PortAudio.PaHostApiInfo that)
        {
            return GetDevices()
                .Where(it => it.hostApi == that.ApiNumber())
                .ToList();
        }

        public static int DeviceNumber(this PortAudio.PaDeviceInfo that)
        {
            var devicesCount = PortAudio.Pa_GetDeviceCount();
            for (var i = 0; i < devicesCount; i++)
            {
                var device = PortAudio.Pa_GetDeviceInfo(i);
                if (device.name == that.name)
                    return i;
            }

            return -1;
        }

        public static PortAudio.PaHostApiInfo? TryGetHostApi(this PortAudio.PaDeviceInfo that)
        {
            return GetHostApiInfos()
                .Select(it => new PortAudio.PaHostApiInfo?(it))
                .FirstOrDefault(it => it.Value.GetDevices().Contains(that));
        }

        public static DefaultDevices GetDefaultDevices()
        {
            Initialize();

            var apiInfo = GetDefaultApi();
            return new DefaultDevices(
                apiInfo.defaultInputDevice != -1
                    ? PortAudio.Pa_GetDeviceInfo(apiInfo.defaultInputDevice)
                    : NoDevice,
                apiInfo.defaultOutputDevice != -1
                    ? PortAudio.Pa_GetDeviceInfo(apiInfo.defaultOutputDevice)
                    : NoDevice);
        }
    }
}