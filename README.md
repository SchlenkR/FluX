# Synopsis

FSiren is a library that helps describing signal processing computations in a convenient way.
It can be used to:

* Generate / modify audio signals
* Simulation
* PLC (programmable logic controller) / automation tasks

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
 