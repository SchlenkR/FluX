namespace FLooping.IO

open CSCore

module CsCoreInterop =

    type StereoSampleSource<'a> (sequenceFactory: float -> float seq) =
        
        let channels = 2
        let sampleRate = 44100
        let sequence = (float sampleRate) |> sequenceFactory
        let enumerator = sequence.GetEnumerator()

        interface CSCore.ISampleSource with
            member val CanSeek = false with get
            member val Length = 0L with get
            member val Position = 0L with get, set
            member val WaveFormat : WaveFormat = 
                new WaveFormat(sampleRate, 32, channels, AudioEncoding.IeeeFloat) with get

            member this.Dispose() = ()
            
            member this.Read(buffer, offset, count) =
                for i in offset..((count - 1) / channels) do
                    enumerator.MoveNext() |> ignore
                    let value = float32 enumerator.Current
                    
                    Array.set buffer (i * channels) value
                    Array.set buffer (i * channels + 1) value
                    ()
                count

