module FSiren.Core

type M<'a, 'g> = 
    | M of ((Option<'a> * 'g) -> ('a * 'g))
    member x.Run = match x with M b -> b

// Builder
type CircuitBuilder() =
    member this.Bind(m:M<'a, 'g>, f:('a -> M<'b, 'g>)) : M<('a * 'b), 'g> =
        M(fun wholeState ->
            let aOpt, globalState = wholeState
            let a', b' =
                match aOpt with
                | None -> None, None
                | Some (a,b) -> Some a, Some b

            let mResValue, mResGlobalState = m.Run (a', globalState)
            let mF = f mResValue
            let fResValue, fResGlobalState = mF.Run (b', mResGlobalState)
            (mResValue, fResValue), fResGlobalState
        )
    member this.Return x = M(fun (_, globalState) -> (x, globalState))
let circuit = CircuitBuilder()


// helper
let t1 seed acc =
    M(function
        | None, globalState -> seed, globalState
        | Some localStateState, globalState -> acc localStateState, globalState)

// domain instances
let counter seed inc = t1 seed (fun prev -> prev + inc)
