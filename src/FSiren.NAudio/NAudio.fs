namespace FSiren

module Playback =
    
    open NAudio.Wave
    open System.Threading

    type SampleProvider<'a> (gen:Gen<float,'a,Env>) =
        let mutable samplePos = 0.0;
        let env = { Env.samplePos = samplePos; Env.sampleRate = 44100.0 }
        let mutable lastState : 'a option = None
        
        interface NAudio.Wave.ISampleProvider with
            
            member val WaveFormat : WaveFormat = 
                WaveFormat.CreateIeeeFloatWaveFormat(44100, 2) with get
            
            member this.Read(buffer, offset, count) =
                for i in offset..count do
                    let value,state = (run gen) lastState { env with samplePos = samplePos }
                    Array.set buffer i (float32 value)

                    lastState <- Some state
                    samplePos <- samplePos + 1.0
                    ()

                count

    let playSync (duration:float<s>) (gen:Gen<float,'a,Env>) =
        let sampleProvider = new SampleProvider<'a>(gen)
        use waveOut = new WaveOut()
        waveOut.DesiredLatency <- 1000
        waveOut.PlaybackStopped.Add (fun _ -> printfn "Playback stopped.")
        waveOut.Init(sampleProvider)
        waveOut.Play()
        let d = match duration with
                | 0.0<s> -> System.TimeSpan.MaxValue
                | v -> System.TimeSpan.FromSeconds (float v)
        Thread.Sleep d
        waveOut.Stop()
        ()
