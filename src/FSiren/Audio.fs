namespace FSiren

[<AutoOpen>]
module Audio =

    [<Measure>] type s
    [<Measure>] type Hz = 1/s
    [<Measure>] type Deg

    type Env = {
        samplePos : float;
        sampleRate : float;
    }

    let toSeconds (env:Env) = (env.samplePos / env.sampleRate) * 1.0<s>
