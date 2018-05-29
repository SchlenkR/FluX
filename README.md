# Synopsis

[![Join the chat at https://gitter.im/FLooping/Lobby](https://badges.gitter.im/FLooping/Lobby.svg)](https://gitter.im/FLooping/Lobby?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

FLooping is a signal processing library written in F#. It is designed for audio synthesis and effect processing, but can also be used for other purposes like simulation or control tasks (PLC). FLooping processes sample by sample, thus enabling to build low-level DSP structures such as filters, oscillators and other DSP building blocks.

FLooping simplifies the way in that state based functions are composed. Unlike in imperative languages where instanciation and evaluation of components have to be handled in user code, FLooping handles per-component state and evaluation of components for you. This leads to a way of describing signal flows where you can define components in-line and treat them as if they were pure functions.

# Code Examples

## How to Execute the Samples

* Clone or download the source.
* The sample code below is a copy of the code in `./Demo.fsx`
* Open that file and send it to F# Interactive by selecting the code and press Alt+Enter.

```fsharp
#r @"./dist/FLooping.dll"
#r @"./dist/CSCore.dll"
#r @"./dist/FLooping.IO.dll"

open FLooping
open FLooping.IO


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
```
 