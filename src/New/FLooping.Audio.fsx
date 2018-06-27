#load @"FLooping.Core.fsx"

open FLooping.Core
open Microsoft.FSharp.Data.UnitSystems.SI.UnitSymbols


type Env = {
    samplePos : float;
    sampleRate : float;
}

// Simplifies usage with 'Env' as reader state type.
type LoopBuilder() =
    member __.Bind ((m:L<_,_,Env>), f) = Core.bind m f
    member __.Return x = Core.ret x
    member __.ReturnFrom l = Core.retFrom l
let loop = LoopBuilder()

let toSeconds (env:Env) = (env.samplePos / env.sampleRate) * 1.0<s>
