#r @"./bin/debug/CsCore.dll"
#r @"./bin/debug/FSiren.dll"
#r @"./bin/debug/FSiren.NAudio.dll"

open FSiren
open FSiren.Playback

circuit {
    //let! m = sin 5.0
    //let! s = sin (1000.0 * (1.0 - m * 0.01))
    //return s
    let! v = sin 2000.0
    return v
}
|> playSync 5.0<s>
