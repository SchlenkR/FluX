#load @"FLooping.Core.fsx"

open System
open System.Threading
open Microsoft.FSharp.Data.UnitSystems.SI.UnitSymbols
open FLooping.Core


[<AutoOpen>]
module Math =
    let pi = Math.PI
    let pi2 = 2.0 * pi


[<AutoOpen>]
module AudioEnvironment =

    type Env = {
        samplePos: int;
        sampleRate: int;
    }

    // let toSeconds (env:Env) = (env.samplePos / env.sampleRate) * 1.0<s>

    let env() = L(fun p (r:Env) -> {value=r; state=()})


[<AutoOpen>]
module Seq =

    let toSequence (loop:L<_,_,Env>) sampleRate =
        loop
        |> toReaderSequence (fun i -> { samplePos=i; sampleRate=sampleRate })

    let toAudioSequence (l:L<_,_,_>) = toSequence l 44100

    let toList count (l:L<_,_,_>) =
        (toAudioSequence l)
        |> Seq.take count
        |> Seq.toList


[<AutoOpen>]
module Oscillators =

    // TODO
    // static calculation result in strange effects when modulating :D
    //let sin (frq:float<Hz>) (phase:float<Deg>) =
    //    let f (env:Env) = 
    //        let rad = env.samplePos / env.sampleRate
    //        Math.Sin(rad * pi2 * (float frq))
    //    build (lift_s f) ()
        
    let private osc (frq:float) f =
        let f angle (env:Env) =
            let newAngle = (angle + pi2 * frq / (float env.sampleRate)) % pi2
            {value=f newAngle; state=newAngle}
        f |> liftSeed 0.0 |> L

    // TODO: phase
    let sin (frq:float) = osc frq Math.Sin
    let saw (frq:float) = osc frq (fun angle ->
        1.0 - (1.0 / pi * angle))
    let tri (frq:float) = osc frq (fun angle ->
        if angle < pi
        then -1.0 + (2.0 / pi) * angle
        else 3.0 - (2.0 / pi) * angle)
    let square (frq:float) = osc frq (fun angle ->
        if angle < pi then 1.0 else -1.0)

    // TODO: ringBuffer
    // TODO: flipFlop
    // TODO: hysteresis
    // TODO: follower
    // TODO: HP/LP/BP/Comb
    // TODO: ADSR
    // TODO: bitCrusher
    // TODO: chorus
    // TODO: flanger
    // TODO: phaser
    // TODO: reverb
    // TODO: saturator

    // TODO: Voices

