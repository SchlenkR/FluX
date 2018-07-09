#load @"../src/FLooping.fsx"

open System

open FLooping.Core
open FLooping.Audio
open FLooping.Audio.IO.Analysis


loop {
    let! x = counter 0.0 1.0
    return x
}
|> measureLoop (TimeSpan.FromSeconds 1.0)


// TODO: Wow - second alternative is 2 - 3 times faster than the first one! Why?
[
    toSequence id <| loop {
        let! x = counter 0.0 1.0
        return x
    };
    
    toSequence id <| loop {
        let! x = counterAlt 0.0 1.0
        return x
    };
] |> compare (TimeSpan.FromSeconds 1.0)


