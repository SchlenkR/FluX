#r "./FLooping/bin/Debug/netcoreapp2.0/FLooping.dll"
#load "./FLooping.AudioPlayback.fsx"

open System
open FLooping
open FLooping.Blocks
open FLooping.Analysis


loop {
    let! x = counter 0.0 1.0
    return x
}
|> toIdSequence
|> measure (TimeSpan.FromSeconds 1.0)


// TODO: Wow - second alternative is 2 - 3 times faster than the first one! Why?
[
    toAudioSequence <| loop {
        let! x = counter 0.0 1.0
        return x
    };
    
    toAudioSequence <| loop {
        let! x = counterAlt 0.0 1.0
        return x
    };
] |> compare (TimeSpan.FromSeconds 1.0)


