# Synopsis

FLooping is a signal processing library written in F#. It is designed for audio synthesis and effect processing, but can also be used for other purposes like simulation or control tasks (PLC). FLooping processes sample by sample, thus enabling to build low-level DSP structures such as filters, oscillators and other DSP building blocks. You connect those modules in a way as if they were pure functions (even if they are state based). FLooping handles the instanciation, the state and the evaluation of these functions for you.

# Code Examples

## How to Execute the Samples
* Clone or download the source
* Build it with Visual Studio or VS Code
* A "bin" folder under the repository root is created with all necessary binaries. Currently, there is no nuget package available yet.
* Send the sample code to F# interactive by pressing Alt+Enter in Visual Studio.

```fsharp
#r @"./../../bin/FLooping.dll"
#r @"./../../bin/CSCore.dll"
#r @"./../../bin/FLooping.IO.dll"

open FLooping
open FLooping.IO


// increment a counter by 1, starting with 0 and print it to the output.
loop {
    let! x = counter 0.0 1.0
    return x
}
|> eval 20
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
```
 