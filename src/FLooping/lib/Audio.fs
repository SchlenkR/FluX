namespace FLooping

[<AutoOpen>]
module AudioEnvironment =

    type Env = {
        samplePos: int;
        sampleRate: int;
    }

    // let toSeconds (env:Env) = (env.samplePos / env.sampleRate) * 1.0<s>

    let env() = L(fun p (r:Env) -> {value=r; state=()})


[<AutoOpen>]
module SeqAudio =

    let toSequence (loop:L<_,_,Env>) sampleRate =
        loop
        |> toReaderSequence (fun i -> { samplePos=i; sampleRate=sampleRate })

    let toAudioSequence (l:L<_,_,_>) = toSequence l 44100

    let toList count (l:L<_,_,_>) =
        (toAudioSequence l)
        |> Seq.take count
        |> Seq.toList

