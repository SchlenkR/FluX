open FSiren
open FSiren.Playback

[<EntryPoint>]
let main argv =

    // modulate the frequency of a sinus oscillator with an LFO (low freq oscillator)
    circuit {
        let! m = sin 5.0
        let! s = sin (1000.0 * (1.0 - m * 0.01))
        return s
    }
    |> playSync 20.0<s>

    0
