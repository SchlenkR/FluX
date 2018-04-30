module FSiren

type M<'a, 'TState> = 
    | M of ((Option<'a> * 'TState) -> ('a * 'TState))
    member x.Run = match x with M b -> b

// Builder
type CircuitBuilder() =
    member this.Bind(m:M<'a, 'TState>, f:('a -> M<'b, 'TState>)) : M<('a * 'b), 'TState> =
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
let t_1 seed acc =
    M(function
        | None, globalState -> seed, globalState
        | Some localStateState, globalState -> acc localStateState, globalState)

// domain instances
let counter seed inc = t_1 seed (fun prev -> prev + inc)
