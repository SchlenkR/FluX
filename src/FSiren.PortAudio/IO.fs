namespace FSiren.PortAudio

[<AutoOpen>]
module IO =
        
    let play() =
        let random = new System.Random()

        let getSignal bufferSize =
            let randomChannel = [
                for i in 0..bufferSize do
                yield random.Next() |> float
            ]
            let arr = randomChannel |> List.toArray
            (arr,arr)
        
        let defaultDevices = FSiren.PortAudioAdapter.PortAudioExtensions.GetDefaultDevices()
        let paWrapper = new FSiren.PortAudioAdapter.PortAudioWrapper(
                            44100, 
                            4096, 
                            defaultDevices.DefaultInputDevice, 
                            defaultDevices.DefaultOutputDevice,
                            FSiren.PortAudioAdapter.ProcessAudio(getSignal))
        
        fun() -> paWrapper.Dispose()
