#r "./FLooping/bin/Debug/netcoreapp2.0/FLooping.dll"
#load "./FLooping.AudioPlayback.fsx"

open Microsoft.FSharp.Data.UnitSystems.SI.UnitSymbols
open FLooping
open FLooping.Blocks
open FLooping.AudioPlayback


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
        match (float e.samplePos / float e.sampleRate) % 1.0 > 0.5 with
        | true -> tri 2000.0 
        | false -> sin 2000.0
    return v
}
|> playSync 5.0<s>


// A feedback loop: Feed the value of a counter back to the next evaluation.
1.0 =-> fun last -> loop {
    let current = last + 0.1
    return { out=current; feedback=current }
}
|> toList 5
|> List.iter (printfn "%f")


// noise with low pass filter
loop {
    let! frqS = sin 5.0 <!> (fun x -> (x + 1.0) * 1500.0)
    let! n = noise()
    let! f = lowPass n { lowPassDef with frq=frqS; }
    return f
}
|> playSync 3.0<s>
