namespace FSiren

module Playback =
    
    open System.Threading
    
    open CSCore
    open CSCore.SoundOut
    open CSCore.Streams.SampleConverter

    type StereoSampleSource<'a> (gen:Gen<float,'a,Env>) =
        
        let channels = 2
        let sampleRate = 44100
        let mutable samplePos = 0.0;
        let env = { Env.samplePos = samplePos; Env.sampleRate = (float sampleRate) }
        let mutable lastState : 'a option = None

        interface CSCore.ISampleSource with
            member val CanSeek = false with get
            member val Length = 0L with get
            member val Position = 0L with get, set
            member val WaveFormat : WaveFormat = 
                new WaveFormat(sampleRate, 32, channels, AudioEncoding.IeeeFloat) with get

            member this.Dispose() = ()
            
            member this.Read(buffer, offset, count) =
                for i in offset..((count - 1) / channels) do
                    let value,state = (run gen) lastState { env with samplePos = samplePos }
                    
                    Array.set buffer (i * channels) (float32 value)
                    //Array.set buffer (i * channels + 1) (float32 value)

                    lastState <- Some state
                    samplePos <- samplePos + 1.0
                    ()

                count

    let playSync (duration:float<s>) (gen:Gen<float,'a,Env>) =
        use waveOut = new WasapiOut()
        let sampleSource = new StereoSampleSource<_>(gen)
        waveOut.Initialize(new SampleToIeeeFloat32(sampleSource));
        waveOut.Play()
        
        let d = match duration with
                | 0.0<s> -> System.TimeSpan.MaxValue
                | v -> System.TimeSpan.FromSeconds (float v)
        Thread.Sleep d
        waveOut.Stop()

        ()
