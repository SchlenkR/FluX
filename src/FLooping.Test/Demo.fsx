#r @"./../../bin/FLooping.dll"
#r @"./../../bin/CSCore.dll"
#r @"./../../bin/FLooping.IO.dll"

open FLooping
open FLooping.IO
open Microsoft.FSharp.Data.UnitSystems.SI.UnitSymbols


// --------> evaluate all code lines until here and start playing with the code below


// increment a counter by 1, starting with 0 and print it to the output.
loop {
    let! x = counter 0.0 1.0
    return x
}
|> toList 20
|> List.iter (printfn "%f")


// play a sin wave (5kHz) for 5 seconds
loop {
    let! x = sin 5000.0
    return x
}
|> playSync 5.0<s>


// modulate the frequence of a sawtooth wave with an LFO (low frequency oscillator)
loop {
    let! modulator = sin 5.0
    let amount = 0.05
    let! s = saw (1000.0 * (1.0 - modulator * amount))
    return s
}
|> playSync 5.0<s>


// "tatü-tataa": switch the waveform every 1/2 second
loop {
    // get environment
    let! env = getState()
    let! v =
        if (env.samplePos / env.sampleRate) % 1.0 > 0.5 
        then tri 2000.0 
        else sin 2000.0
    return v
}
|> playSync 5.0<s>
