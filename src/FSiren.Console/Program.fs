open FSiren
open FSiren.Playback

[<EntryPoint>]
let main argv =

    circuit {
        //let! r = random()
        //return r
        let! s = sin 500.0<Hz> 0.0<Deg>
        return s
    }
    |> playSync 10.0<s>

    0
