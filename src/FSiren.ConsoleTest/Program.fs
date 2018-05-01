// Learn more about F# at http://fsharp.org

open System
open FSiren

type GlobalState = {data:string}

[<EntryPoint>]
let main argv =

    let comp = circuit {
        let! x = counter 1 1
        let! y = counter 11 1
        return sprintf "x=%A y=%A" x y
    }

    let v1,s1,g1 = (run comp) None {data = "Some global state"}
    let v2,s2,g2 = (run comp) (Some s1) g1
    let v3,s3,g3 = (run comp) (Some s2) g2
    let v4,s4,g4 = (run comp) (Some s3) g3

    printfn "v1: %A" v1
    printfn "v2: %A" v2
    printfn "v3: %A" v3
    printfn "v4: %A" v4

    0
