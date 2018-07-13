namespace FLooping

module Convert =

    /// Converts a looping monad into a sequence.
    /// The getReaderState function is called for each evaluation.
    let toSeq getReaderState (l:L<_,_,_>) =
        let mutable lastState : 'a option = None
        Seq.initInfinite (fun i ->
            let res = (run l) lastState (getReaderState i)
            lastState <- Some res.state
            res.value
        )

    let toSeqOrd (l:L<'a,_,_>) : seq<'a> = toSeq id l

    let toSeqEnvSR (loop:L<_,_,Env>) sampleRate =
        loop
        |> toSeq (fun i -> {samplePos=i; sampleRate=sampleRate})

    let toSeqEnv (l:L<_,_,_>) = toSeqEnvSR l 44100

    let toList count (s:seq<_>) =
        s
        |> Seq.take count
        |> Seq.toList

