#r @"./FLooping/bin/debug/netstandard2.0/FLooping.dll"
#load "./FLooping/CsCoreAudio.fsx"

open FLooping
open FLooping.IO

loop {
    //let! m = sin 5.0
    //let! s = sin (1000.0 * (1.0 - m * 0.01))
    //return s

    let! env = getState()
    let! v =
        if (env.samplePos / env.sampleRate) % 1.0 > 0.5 
        then tri 2000.0 
        else sin 2000.0
    return v

    //let! v = interceptState (fun (env:Env) -> 
    //    if (env.samplePos / env.sampleRate) % 1.0 > 0.5 
    //    then tri 2000.0 
    //    else sin 2000.0)
    //return v

}
|> playSync 5.0<s>
