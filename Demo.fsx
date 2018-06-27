#r @"./dist/FLooping.dll"
#r @"./dist/CSCore.dll"
#r @"./dist/FLooping.IO.dll"


open FLooping
open FLooping.IO
open Microsoft.FSharp.Data.UnitSystems.SI.UnitSymbols


(* 
  evaluate all code lines until here and start playing with the code below
*)


// increment a counter by 1, starting with 0 and print it to the output.
loop {
    let! x = counter 0.0 1.0
    return x
}
|> toList 20
|> List.iter (printfn "%f")


// play a sin wave (5kHz) for 5 seconds
loop {
    let! x = sin 5000.0
    return x
}
|> playSync 5.0<s>


// modulate the frequence of a sawtooth wave with an LFO (low frequency oscillator)
loop {
    let! modulator = sin 5.0
    let amount = 0.05
    let! s = saw (1000.0 * (1.0 - modulator * amount))
    return s
}
|> playSync 5.0<s>


// "tatü-tataa": switch the waveform every 1/2 second
loop {
    // get environment
    let! e = env()
    let! v =
        if (e.samplePos / e.sampleRate) % 1.0 > 0.5 
        then tri 2000.0 
        else sin 2000.0
    return v
}
|> playSync 5.0<s>



//let fdb1 seed (l:L<'v,'s,'r>) =
//    let f p r =
//        let valL,stateL = (run l) (Some p) r
//        (p,valL),stateL
//    liftSeed seed f

//loop {
//    let! c1,c0 = loop {
//        let! a,b =  counter 0.0 1.0 |> fdb1 100.0
//        return a,b
//    }
//    printfn "c-1: %f - c: %f" c1 c0
//    return c0
//}
//|> toList 5
//|> List.iter (printfn "%f")



let feedback seed (f: 'a -> L<('a * 'v),'s,'r>) =
    let f1 = fun prev r ->
        let myPrev,innerPrev = match prev with
                                | None            -> seed,None
                                | Some (my,inner) -> my,inner
        let l = f myPrev
        let monad = (run l)
        let (loopValue,realValue),innerState = monad innerPrev r
        (realValue,(loopValue,Some innerState))
    L f1

fun last -> loop {
   let! c = counter 0.0 (last * 0.1)
   return (c,c)
} 
|> feedback 1.0
|> toList 5
|> List.iter (printfn "%f")

////(*
////    x---(+)---
////         |
////         |---(T-1)---
////*)


////type RetVal<'a,'b> =
////    | V of 'a
////    | VS of ('a * 'b)


////let ret x = 
////    match x with
////    | V v -> (v,None)
////    | VS (v,s) -> (v,Some s)

////let res = ret (VS (5,"kjkj"))
