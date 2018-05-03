#r "bin\Debug\FSiren.dll"
#r "bin\Debug\FSiren.NAudio.dll"

open FSiren
open FSiren.Playback
  
let comp = circuit {
    let! r = random()
        return r
}

playSync comp 2.0<s> |> ignore
