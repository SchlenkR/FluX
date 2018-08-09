#r "System.Runtime.Extensions"
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
|> Convert.iter 20 (printfn "%f")


// play a sin wave (5kHz) for 5 seconds
loop {
    let! x = Osc.sin 5000.0
    return x
}
|> playSync 2.5<s>




// modulate the frequence of a sawtooth wave with an LFO (low frequency oscillator)
loop {
    let amount = 0.05
    let! modulator = Osc.sin 5.0
    let! s = Osc.saw (1000.0 * (1.0 - modulator * amount))
    return s
}
|> playSync 2.5<s>

// Alternative: Inline the modulating oscillator (<*>) and using map (<!>)
loop {
    let amount = 0.05
    let! s = !Osc.saw <**> (Osc.sin 5.0 <!> (fun modulator -> 1000.0 * (1.0 - modulator * amount)))
    return s
}
|> playSync 2.5<s>

// Alternative: Inline the modulating oscillator (<*>) and using arithmetic operators
loop {
    let amount = 0.05
    let! s = !Osc.saw <**> (1000.0 * (1.0 - (Osc.sin 5.0) * amount))
    return s
}
|> playSync 2.5<s>



// "tatü-tataa": switch the waveform every 1/2 second
loop {
    // get environment
    let! e = read()
    let! v =
        match (float e.samplePos / float e.sampleRate) % 1.0 > 0.5 with
        | true -> tri 2000.0 
        | false -> sin 2000.0
    return v
}
|> playSync 2.5<s>



// A feedback loop: Feed the value of a counter back to the next evaluation.
1.0 <-> fun last -> loop {
    let current = last + 0.1
    return {out=current; feedback=current}
}
|> Convert.iter 20 (printfn "%f")



// Demo: Map operator
// noise with low pass filter
// TODO: { Filter.lowPassDef with frq=frqS; } sieht scheiße aus
loop {
    let! frqS = (Osc.sin 5.0) + 1.0 * 1500.0
    let! f = !Filter.lowPass <*> Osc.noise() <**> !{ Filter.lowPassDef with frq=frqS; }
    return f
}
|> playSync 3.0<s>





// TODO: doc
loop {
    let! env = read()
    let! res = flp false (env.samplePos = 3)
    return (res,env.samplePos)
}
|> Convert.iter 20 (printfn "%A")

// TODO: verschachtelte Loops / Auslagern in wiederverwendbare Einheiten

// TODO: In Demo einarbeiten
loop {
    let! c = !counter <*> !0.0 <**> !1.0
    return c
}
|> Convert.iter 20 (printfn "%f")
