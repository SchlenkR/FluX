namespace FLooping

[<AutoOpen>]
module AudioEnvironment =

    type Env = {
        samplePos: int;
        sampleRate: int;
    }

    // let toSeconds (env:Env) = (env.samplePos / env.sampleRate) * 1.0<s>

    let env() = L(fun p (r:Env) -> {value=r; state=()})


[<RequireQualifiedAccess>]
module Audio =

    let toSequence1 (loop:L<_,_,Env>) sampleRate =
        loop
        |> FLooping.Seq.toSequence (fun i -> {samplePos=i; sampleRate=sampleRate})

    let toSequence (l:L<_,_,_>) = toSequence1 l 44100

    let toList count (l:L<_,_,_>) =
        (toSequence l)
        |> Seq.take count
        |> Seq.toList

