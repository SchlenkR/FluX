#r @"./bin/debug/FSiren.dll"
#r @"./bin/debug/CsCore.dll"
#load "CsCoreAudio.fs"

open FSiren
open FSiren.IO

block {
    //let! m = sin 5.0
    //let! s = sin (1000.0 * (1.0 - m * 0.01))
    //return s
    let! x = sin 5.0
    return x
    //let! v = readState (fun (env:Env) -> 
    //    if (env.samplePos / env.sampleRate) % 1.0 > 0.5 
    //    then tri 2000.0 
    //    else sin 2000.0)
    //return v
}
|> playSync 5.0<s>
