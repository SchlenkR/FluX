# Synopsis

FLooping is a signal processing library written in F#. It is designed for audio synthesis and effect processing, but can also be used for other purposes like simulation or control tasks (PLC). FLooping processes sample by sample, thus enabling to build low-level DSP structures such as filters, oscillators and other DSP building blocks. You connect those modules in a way as if they were pure functions (even if they are state based). FLooping handles the instanciation, the state and the evaluation of these functions for you.

# Code Examples

## Play noise for 2 seconds

```
#r "binaries\FSiren.dll"
#r "binaries\NAudio.dll"
#r "binaries\FSiren.NAudio.dll"

open FSiren
open FSiren.Playback

// modulate the frequency of a sinus oscillator 
// with an LFO (low freq oscillator)

circuit {
    let! m = sin 5.0
    let! s = sin (1000.0 * (1.0 - m * 0.01))
    return s
}
|> playSync 20.0<s>
```
 