// Learn more about F# at http://fsharp.org

open System
open FSiren.Core

[<EntryPoint>]
let main argv =
    
    let comp = circuit {
        let! x = counter 1 1
        let! y = counter 11 1
        return sprintf "x=%A y=%A" x y
    }

    let c1 = comp.Run None
    let c2 = comp.Run (Some c1)
    let c3 = comp.Run (Some c2)
    let c4 = comp.Run (Some c3)

    0
