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
|> Convert.toSeqOrd
|> measure (TimeSpan.FromSeconds 1.0)


// TODO: Wow - second alternative is 2 - 3 times faster than the first one! Why?
[
    Convert.toSeqOrd <| loop {
        let! x = counter 0.0 1.0
        return x
    };
    
    Convert.toSeqOrd <| loop {
        let! x = counterAlt 0.0 1.0
        return x
    };
] |> compare (TimeSpan.FromSeconds 1.0)


let eval l =
    let start = DateTime.Now
    l
    |> Convert.toSeqEnv
    |> Seq.takeWhile (fun _ -> (DateTime.Now - start).TotalSeconds < 1.0)
    |> Seq.mapi (fun i _ -> i)
    |> Seq.last
    // |> Seq.fold (fun state _ -> state + 1) 0

loopGen<Env> {
    let! x = counter 0.0 1.0
    return x
}
|> eval

loopGen<Env> {
    let! x = counterAlt 0.0 1.0
    return x
}
|> eval

let (?=) (cond:bool) a b = 
    // let i1,i2 = a
    if cond then a else b

let (!=) = (<|)
let res = true ?= "true" != "false"
