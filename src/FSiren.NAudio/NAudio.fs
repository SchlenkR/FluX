namespace FSiren

module Playback =
    
    open NAudio.Wave
    open System.Threading

    type SampleProvider<'a> (gen:Gen<float,'a,Env>) =
        let mutable samplePos = 0;
        let env = { Env.samplePos = samplePos; Env.sampleRate = 44100<Hz> }
        let mutable lastState : 'a option = None
        
        interface NAudio.Wave.ISampleProvider with
            
            member val WaveFormat : WaveFormat 
                = WaveFormat.CreateIeeeFloatWaveFormat(44100, 2) with get
            
            member this.Read(buffer, offset, count) =
                for i in offset..count do
                    let value,state = (run gen) lastState { env with samplePos = samplePos }
                    Array.set buffer i (float32 value)

                    lastState <- Some state
                    samplePos <- samplePos + 1
                    
                    ()
                count

    let playSync (gen:Gen<float,'a,Env>) (duration:float<s>) =
        let sampleProvider = new SampleProvider<'a>(gen)
        let waveOut = new WaveOut()
        waveOut.Init(sampleProvider)
        waveOut.Play()
        let d = match duration with
                | 0.0<s> -> System.TimeSpan.MaxValue
                | v -> System.TimeSpan.FromSeconds (float v)
        Thread.Sleep d
        waveOut.Stop()
        ()
