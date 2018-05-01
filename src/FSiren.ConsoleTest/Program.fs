// Learn more about F# at http://fsharp.org

open System
open FSiren

type GlobalState = {data:string}

[<EntryPoint>]
let main argv =

    let comp = circuit {
        let! x = counter 11 1
        //let! y = counter 11 0
        //let! z = counter 21 0
        //return sprintf "x=%A y=%A z=%A" x y z
        return sprintf "x=%A " x
    }

    let constantGlobalState = {data = "Some global state"}

    //// Alternative 1
    //let v1,s1 = (run comp) None constantGlobalState
    //let v2,s2 = (run comp) (Some s1) constantGlobalState
    //let v3,s3 = (run comp) (Some s2) constantGlobalState
    //let v4,s4 = (run comp) (Some s3) constantGlobalState

    // Convenient
    let v1,cont1 = (start comp) constantGlobalState
    let v2,cont2 = cont1.Eval constantGlobalState
    let v3,cont3 = cont2.Eval constantGlobalState
    let v4,cont4 = cont3.Eval constantGlobalState

    printfn "v1: %A" v1
    printfn "v2: %A" v2
    printfn "v3: %A" v3
    printfn "v4: %A" v4

    0
