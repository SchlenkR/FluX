#load @"./src/FLooping.fsx"

open Microsoft.FSharp.Data.UnitSystems.SI.UnitSymbols

open FLooping.Core
open FLooping.Audio
open FLooping.Audio.IO


///
/// Evaluate all code lines until here and start playing with the code below.
///



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
    let! e = env()
    let! v =
        if (e.samplePos / e.sampleRate) % 1.0 > 0.5 
        then tri 2000.0 
        else sin 2000.0
    return v
}
|> playSync 5.0<s>


// A feedback loop: Feed the value of a counter back to the next evaluation.
1.0 -=> fun last -> loop {
    let current = last + 0.1
    return { out=current; feedback=current }
}
|> toList 5
|> List.iter (printfn "%f")
