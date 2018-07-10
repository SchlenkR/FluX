namespace FLooping.Blocks

open System
open FLooping

[<AutoOpen>]
module Base =

    /// Delays a given value by 1 cycle.
    let delay seed current =
        let f prev = {value=prev; state=current}
        liftR f |> liftSeed seed |> L

    let counter (seed:float) (inc:float) =
        let f prev = prev + inc
        let lifted = liftRV f 
        lifted |> liftSeed seed |> L

    let counterAlt (seed:float) (inc:float) = 
        seed =-> fun last -> loop {
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
