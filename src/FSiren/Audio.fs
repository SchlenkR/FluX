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

    //// Uses Env type explicitly to simplify usage.
    //type SignalBuilder() =
    //    member this.Bind (m:Block<'a,'sa,Env>) (f:'a -> Block<'b,'sb,Env>) : Block<'b, ('sa * 'sb), Env> = bind m f
    //    member this.Return x = ret x
    //let signal = SignalBuilder()

    let toSeconds (env:Env) = (env.samplePos / env.sampleRate) * 1.0<s>
