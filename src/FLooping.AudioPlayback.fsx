#r "./FLooping/bin/Debug/netcoreapp2.0/FLooping.dll"
#r @"../packages/CSCore/lib/net35-client/cscore.dll"

open Microsoft.FSharp.Data.UnitSystems.SI.UnitSymbols
open System.Threading
open CSCore
open CSCore.SoundOut
open CSCore.Streams.SampleConverter
open FLooping

type private StereoSampleSource<'a> (sequenceFactory: int -> float seq) =
    let channels = 2
    let sampleRate = 44100
    let sequence = sampleRate |> sequenceFactory
    let enumerator = sequence.GetEnumerator()

    interface CSCore.ISampleSource with
        member val CanSeek = false with get
        member val Length = 0L with get
        member val Position = 0L with get, set
        member val WaveFormat : WaveFormat = 
            new WaveFormat(sampleRate, 32, channels, AudioEncoding.IeeeFloat) with get

        member __.Dispose() = ()
        
        member __.Read(buffer, offset, count) =
            for i in offset..((count - 1) / channels) do
                enumerator.MoveNext() |> ignore
                let value = float32 enumerator.Current
                
                Array.set buffer (i * channels) value
                Array.set buffer (i * channels + 1) value
                ()
            count

let playSync (duration:float<s>) (l:L<float,'a,Env>) =
    let latencyInMs = 1000
    // TODO: Usage of newer backend better?
    use waveOut = new DirectSoundOut(latencyInMs, ThreadPriority.AboveNormal)

    let loopingSequence = Convert.toSeqSample l
    let sampleSource = new StereoSampleSource<_>(loopingSequence)

    waveOut.Initialize(new SampleToIeeeFloat32(sampleSource));
    waveOut.Play()

    let d = match duration with
            | 0.0<s> -> System.TimeSpan.MaxValue
            | v -> System.TimeSpan.FromSeconds (float v)
    Thread.Sleep d
    waveOut.Stop()
    ()
