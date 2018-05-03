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

circuit {
    let! s = sin 500.0<Hz> 0.0<Deg>
    return s
}
|> playSync 10.0<s>
```
 