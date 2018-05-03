namespace FSiren

[<AutoOpen>]
module Audio =

    [<Measure>] type s
    [<Measure>] type Hz = 1/s
    [<Measure>] type Deg

    type Env = {
        samplePos:   float;
        sampleRate : float;
    }
