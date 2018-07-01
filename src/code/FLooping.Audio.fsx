#load @"FLooping.Core.fsx"

open FLooping.Core
open Microsoft.FSharp.Data.UnitSystems.SI.UnitSymbols

type Env = {
    samplePos: float;
    sampleRate: float;
}

let toSeconds (env:Env) = (env.samplePos / env.sampleRate) * 1.0<s>

let env() = L(fun p (r:Env) -> {value=r; state=()})
