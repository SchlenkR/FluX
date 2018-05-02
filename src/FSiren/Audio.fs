namespace FSiren

open System

[<AutoOpen>]
module Audio =

    [<Measure>] type s
    [<Measure>] type Hz = 1/s

    type Env = {
        samplePos:   int;
        sampleRate : int<Hz>;
    }

    let counter (seed:float) (inc:float) =
        let f prev = prev + inc
        t1 (lift_rv f) seed

    let toggle seed =
        let f prev = if (prev = true) then (0.0, false) else (1.0, true)
        t1 (lift_r f) seed

    let random() =
        let f (prev:Random) =
            let v = prev.NextDouble()
            (v,prev)
        t1 (lift_r f) (new Random())
    
        
