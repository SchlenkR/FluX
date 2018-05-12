namespace FSiren

open System

[<AutoOpen>]
module Oscillators =

    let private pi = Math.PI
    let private pi2 = 2.0 * pi

    let counter (seed:float) (inc:float) =
        let f prev = prev + inc
        liftSeed (lift_rv f) seed

    let toggle seed =
        let f prev = if (prev = true) then (0.0, false) else (1.0, true)
        liftSeed (lift_r f) seed

    let random() =
        let f (prev:Random) =
            let v = prev.NextDouble()
            (v,prev)
        liftSeed (lift_r f) (new Random())
    
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
            (f newAngle, newAngle)
        liftSeed f 0.0

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
