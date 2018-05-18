namespace FLooping.IO

open CSCore.SoundOut
open CSCore.Streams.SampleConverter

open FLooping
open FLooping.IO.CsCoreInterop

open Microsoft.FSharp.Data.UnitSystems.SI.UnitSymbols

open System.Threading

[<AutoOpen>]
module Playback =
    
    let private toSequenceInternal (gen:Gen<float,_,Env>) sampleRate =
        gen
        |> FLooping.Core.toSequence (fun i -> { samplePos = (float i); sampleRate = sampleRate })
    
    let toSequence (gen:Gen<float,_,Env>) = toSequenceInternal gen 44100.0

    let toList count (gen:Gen<float,'a,Env>) =
        (toSequence gen) 
        |> Seq.take count
        |> Seq.toList

    let playSync (duration:float<s>) (gen:Gen<float,'a,Env>) =
        let latencyInMs = 1000
        use waveOut = new DirectSoundOut(latencyInMs, ThreadPriority.AboveNormal)

        let genSequence = toSequenceInternal gen
        let sampleSource = new StereoSampleSource<_>(genSequence)

        waveOut.Initialize(new SampleToIeeeFloat32(sampleSource));
        waveOut.Play()

        let d = match duration with
                | 0.0<s> -> System.TimeSpan.MaxValue
                | v -> System.TimeSpan.FromSeconds (float v)
        Thread.Sleep d
        waveOut.Stop()
        ()
