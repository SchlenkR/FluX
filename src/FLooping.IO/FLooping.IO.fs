namespace FLooping.IO

open CSCore.SoundOut
open CSCore.Streams.SampleConverter

open FLooping
open FLooping.IO.CsCoreInterop

open Microsoft.FSharp.Data.UnitSystems.SI.UnitSymbols

open System.Threading

[<AutoOpen>]
module Playback =
    
    let private toSequenceInternal (block:Block<float,_,Env>) sampleRate =
        block
        |> FLooping.Core.toSequence (fun i -> { samplePos = (float i); sampleRate = sampleRate })
    
    let toSequence (block:Block<float,_,Env>) = toSequenceInternal block 44100.0

    let toList count (block:Block<float,'a,Env>) =
        (toSequence block) 
        |> Seq.take count
        |> Seq.toList

    let playSync (duration:float<s>) (block:Block<float,'a,Env>) =
        let latencyInMs = 1000
        use waveOut = new DirectSoundOut(latencyInMs, ThreadPriority.AboveNormal)

        let blockSequence = toSequenceInternal block
        let sampleSource = new StereoSampleSource<_>(blockSequence)

        waveOut.Initialize(new SampleToIeeeFloat32(sampleSource));
        waveOut.Play()

        let d = match duration with
                | 0.0<s> -> System.TimeSpan.MaxValue
                | v -> System.TimeSpan.FromSeconds (float v)
        Thread.Sleep d
        waveOut.Stop()
        ()
