namespace FLooping

open System

[<AutoOpen>]
module Logic =
    let counter (seed:float) (inc:float) =
        let f prev = prev + inc
        //let x = seed |> liftSeed (lift_rv f)
        liftRV f |> liftSeed seed

    let toggle seed =
        let f prev = if prev then (0.0, false) else (1.0, true)
        liftR f |> liftSeed seed

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