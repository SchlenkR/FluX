namespace FSiren

open System

[<AutoOpen>]
module Generators =

    let counter (seed:float) (inc:float) =
        let f prev = prev + inc
        build (lift_rv f) seed

    let toggle seed =
        let f prev = if (prev = true) then (0.0, false) else (1.0, true)
        build (lift_r f) seed

    let random() =
        let f (prev:Random) =
            let v = prev.NextDouble()
            (v,prev)
        build (lift_r f) (new Random())
    
    let sin (frq:float<Hz>) (phase:float<Deg>) =
        let f (env:Env) = 
            let rad = env.samplePos / env.sampleRate
            Math.Sin(rad * Math.PI * (float frq))
        build (lift_s f) ()
        
