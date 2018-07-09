#load @"FLooping.Core.fsx"

open System
open FLooping.Core

open Microsoft.FSharp.Data.UnitSystems.SI.UnitSymbols

// Helper
let private pi = Math.PI
let private pi2 = 2.0 * pi


type Env = {
    samplePos: float;
    sampleRate: float;
}

let toSeconds (env:Env) = (env.samplePos / env.sampleRate) * 1.0<s>

let env() = L(fun p (r:Env) -> {value=r; state=()})



let counter (seed:float) (inc:float) =
    let f prev = prev + inc
    let lifted = liftRV f 
    lifted |> liftSeed seed |> L

let counterAlt (seed:float) (inc:float) = 
    seed -=> fun last -> loop {
        let value = last + inc
        return {out=value; feedback=value}
}

let toggle seed =
    let f prev = if prev
                 then {value=0.0; state=false}
                 else {value=1.0; state=true}
    liftR f |> liftSeed seed |> L

let noise() =
    let f (prev:Random) =
        let v = prev.NextDouble()
        {value=v; state=prev}
    liftR f |> liftSeed (new Random()) |> L

// TODO
// static calculation result in strange effects when modulating :D
//let sin (frq:float<Hz>) (phase:float<Deg>) =
//    let f (env:Env) = 
//        let rad = env.samplePos / env.sampleRate
//        Math.Sin(rad * pi2 * (float frq))
//    build (lift_s f) ()
    
let private osc (frq:float) f =
    let f angle (env:Env) =
        let newAngle = (angle + pi2 * frq / env.sampleRate) % pi2
        //let trimmedAngle = 
        //    if newAngle > pi2 
        //    then newAngle - pi2
        //    else newAngle
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