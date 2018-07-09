#r @"../packages/CSCore/lib/net35-client/cscore.dll"

#load @"FLooping.Core.fsx"
#load @"FLooping.Audio.fsx"

open System
open System.Threading
open CSCore
open CSCore.SoundOut
open CSCore.Streams.SampleConverter
open FLooping.Core
open FLooping.Audio
open Microsoft.FSharp.Data.UnitSystems.SI.UnitSymbols


type private StereoSampleSource<'a> (sequenceFactory: float -> float seq) =
    let channels = 2
    let sampleRate = 44100
    let sequence = (float sampleRate) |> sequenceFactory
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


module Analysis =

    let measureSeq (time:TimeSpan) (s:seq<_>) =
        let enumerator = s.GetEnumerator()
        let mutable count = 0
        let mutable run = true
        let proc = fun _ ->
            while run do
                count <- count + 1
                enumerator.MoveNext() |> ignore
                enumerator.Current |> ignore
                ()
        let thread = new Thread (ThreadStart proc)
        thread.Start()
        
        Thread.Sleep time
        run <- false

        count

    // let measureSeq (time:TimeSpan) (s:seq<_>) =
    //     let startTime = DateTime.Now
    //     let enumerator = s.GetEnumerator()
    //     let mutable evaluations = 0
    //     while (DateTime.Now - startTime) < time do
    //         let max = 10000
    //         for _ in 1..max do
    //             enumerator.MoveNext() |> ignore
    //             enumerator.Current |> ignore
    //             ()
    //         evaluations <- evaluations + max
    //     (float evaluations) / time.TotalSeconds

    let measureLoop (time:TimeSpan) (l:L<_,_,_>) =
        let s = (toSequence l)
        measureSeq time s

    // type Measurable<'a,'b,'c> =
    //     | S of seq<'a>
    //     | L of L<'a,'b,'c>

    let compare (time:TimeSpan) (ls:seq<_> list) =
        let measured = ls |> List.map (fun l -> measureSeq time l)
        match measured with
        | [] -> []
        | h::t -> (h,1.0) :: (t |> List.map (fun x -> (x,(float x) / (float h))))
