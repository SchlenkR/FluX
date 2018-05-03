#r "binaries\FSiren.dll"
#r "binaries\NAudio.dll"
#r "binaries\FSiren.NAudio.dll"

open FSiren
open FSiren.Playback
  
let comp = circuit {
    let! r = random()
    return r
}

playSync comp 2.0<s> |> ignore
