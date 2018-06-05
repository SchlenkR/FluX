namespace FLooping.IO

open CSCore.SoundOut
open CSCore.Streams.SampleConverter

open FLooping
open FLooping.IO.CsCoreInterop

open Microsoft.FSharp.Data.UnitSystems.SI.UnitSymbols

open System.Threading

[<AutoOpen>]
module Playback =
    
    let private toSequenceInternal (loop:L<float,_,Env>) sampleRate =
        loop
        |> FLooping.Core.toSequence (fun i -> { samplePos = (float i); sampleRate = sampleRate })
    
    let toSequence (l:L<float,_,Env>) = toSequenceInternal l 44100.0

    let toList count (l:L<float,'a,Env>) =
        (toSequence l) 
        |> Seq.take count
        |> Seq.toList

    let playSync (duration:float<s>) (l:L<float,'a,Env>) =
        let latencyInMs = 1000
        use waveOut = new DirectSoundOut(latencyInMs, ThreadPriority.AboveNormal)

        let loopingSequence = toSequenceInternal l
        let sampleSource = new StereoSampleSource<_>(loopingSequence)

        waveOut.Initialize(new SampleToIeeeFloat32(sampleSource));
        waveOut.Play()

        let d = match duration with
                | 0.0<s> -> System.TimeSpan.MaxValue
                | v -> System.TimeSpan.FromSeconds (float v)
        Thread.Sleep d
        waveOut.Stop()
        ()
