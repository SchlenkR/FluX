
// nuget: Suave

#r "../FLooping/bin/Debug/netcoreapp2.0/FLooping.dll"
#r "../../packages/Suave/lib/netstandard2.0/Suave.dll"

open System
open System.IO
open System.Threading
open System.Diagnostics

open FLooping
open FLooping.Blocks

open Suave
open Suave.Filters
open Suave.Utils.Collections
open Suave.Operators
open Suave.Successful



let startSuaveAudioServer() =
    let app =
        GET >=> path "/"
        >=> request (fun r -> 
            ok (
                let random = new Random()
                let count = Int32.Parse(defaultArg (Option.ofChoice (r.query ^^ "count")) "0")
                [|
                    for _ in 0 .. count*2 do
                    yield (random.NextDouble())
                |]
                |> Array.map BitConverter.GetBytes
                |> Array.collect id
            ))

    let cts = new CancellationTokenSource()
    let config = { defaultConfig with
                    cancellationToken = cts.Token;
                    bindings = [ HttpBinding.createSimple HTTP "127.0.0.1" 50300 ] 
                 }
    let listening, server = startWebServerAsync config app
    Async.Start(server, cts.Token)

    fun() ->
        cts.Cancel()
        cts.Dispose()

let stopAudioServer = startSuaveAudioServer()

// stopAudioServer()



let startNodeAudioRenderer() =
    let appJsPath = Path.Combine(__SOURCE_DIRECTORY__, "nodeServerApp", "app.js")

    let nodeStartInfo = new ProcessStartInfo("node.exe", appJsPath)
    nodeStartInfo.UseShellExecute <- false
    let node = Process.Start(nodeStartInfo)

    // TODO: Hack
    Threading.Thread.Sleep 1000

    if node.HasExited then
        failwith "node server has exited."
    else
        printfn "node server running."

    fun () ->
        node.Kill()
        printfn "node server stopped."

let stop = startNodeAudioRenderer()

// stop()

