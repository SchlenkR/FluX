namespace FLooping

module Convert =

    /// Converts a looping monad into a sequence.
    /// The getReaderState function is called for each evaluation.
    let toSeqBase getReaderState (l:L<_,_,_>) =
        let mutable lastState : 'a option = None
        Seq.initInfinite (fun i ->
            let res = (run l) lastState (getReaderState i)
            lastState <- Some res.state
            res.value
        )

    let toSeqOrd (l:L<'a,_,_>) : seq<'a> = toSeqBase id l

    let toSeqSample (loop:L<_,_,Env>) sampleRate =
        loop
        |> toSeqBase (fun i -> {samplePos=i; sampleRate=sampleRate})

    let toSeq (l:L<_,_,_>) = toSeqSample l 44100

    let toList count l =
        l
        |> toSeq
        |> Seq.take count
        |> Seq.toList

    let iter count f l =
        l
        |> toList count
        |> List.iter f
