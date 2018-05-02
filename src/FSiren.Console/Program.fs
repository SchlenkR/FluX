open FSiren
open FSiren.Playback

[<EntryPoint>]
let main argv =

    let comp = circuit {
        let! r = random()
        return r
    }

    playSync comp 2.0<s> |> ignore

    0
