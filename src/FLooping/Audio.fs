namespace FLooping

open Microsoft.FSharp.Data.UnitSystems.SI.UnitSymbols

[<AutoOpen>]
module Audio =

    type Env = {
        samplePos : float;
        sampleRate : float;
    }

    // Simplifies usage with 'Env' as reader state type.
    type LoopBuilder() =
        member this.Bind ((m:L<_,_,Env>), f) = Core.bind m f
        member this.Return x = Core.ret x
        member this.ReturnFrom l = Core.retFrom l
    let loop = LoopBuilder()

    let toSeconds (env:Env) = (env.samplePos / env.sampleRate) * 1.0<s>
