namespace FLooping

[<AutoOpen>]
module Audio =

    [<Measure>] type s
    [<Measure>] type Hz = 1/s
    [<Measure>] type Deg

    type Env = {
        samplePos : float;
        sampleRate : float;
    }

    // Uses Env type explicitly to simplify usage.
    type SignalBuilder() =
        member this.Bind (m, f) = Core.bind m f
        member this.Return x = Core.ret x
    let signal = SignalBuilder()

    let toSeconds (env:Env) = (env.samplePos / env.sampleRate) * 1.0<s>
