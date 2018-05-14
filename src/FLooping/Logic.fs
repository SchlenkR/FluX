namespace FLooping

open System

[<AutoOpen>]
module Logic =

    let counter (seed:float) (inc:float) =
        let f prev = prev + inc
        liftSeed (lift_rv f) seed

    let toggle seed =
        let f prev = if (prev = true) then (0.0, false) else (1.0, true)
        liftSeed (lift_r f) seed
