// Origin: "DAS IST ES - Ma_a_Mb_Mc (2).fsx"

module FSiren

type M<'a, 'TState, 'TGlobalState> =
    | M of ('TState option -> 'TGlobalState -> 'a * 'TState * 'TGlobalState)
let run m = match m with | M x -> x

type CircuitBuilder() =

    member this.Bind (
                      m: M<'a, 'AState, 'TGlobalState>, 
                      f: ('a -> M<'b, 'BState, 'TGlobalState>))  
            : M<'b, ('AState * 'BState), 'TGlobalState> =
        
        let stateFunc localState globalState =
            let prevAState, prevBState = match localState with
                                            | None -> (None, None)
                                            | Some (a,b) -> (Some a, Some b)
            let aValue,aState,mGlobalState = run m prevAState globalState
            let fRes = f aValue
            let bValue,bState,fGlobalState = run fRes prevBState mGlobalState
            bValue,(aState,bState),fGlobalState
        M stateFunc

    member this.Return x = M(fun _ b -> (x,(),b))
let circuit = CircuitBuilder()


/////
//// Convenience
/////
//type Cont<'v, 'g> =
//    | Cont of (unit -> 'v * 'g * Cont<'v, 'g>)
//    member x.Eval = match x with Cont b -> b
//let start (m:M<_,_,_>) =
//    let rec evalInternal oldState globalState = 
//        let v,newS,newG = run m oldState globalState
//        (v, newG, Cont(fun() -> evalInternal (Some newS) newG))
//    evalInternal None


///
// Helper
///

type F<'v, 's, 'g> = ('s -> 'g -> ('v * 's * 'g))

// a : value as state
// b : no global state
// c : read only global state
let lift_a (f:('a * 'b) -> 'g -> ('a * 'b)) =
    fun prev globalState ->
        let fVal,newGlobalState = f prev globalState
        (fVal,fVal,newGlobalState)
let lift_ab (f:'v -> 'v) =
    fun prev globalState ->
        let fVal = f prev
        (fVal,fVal,globalState)
let lift_ac (f:'s -> 'g -> 'v) =
    fun prev globalState ->
        let fVal = f prev globalState
        (fVal,fVal,globalState)
let lift_b (f:'s -> ('v * 's)) =
    fun prev globalState ->
        let fVal,fState = f prev
        (fVal,fState,globalState)
let lift_c (f:'s -> 'g -> ('v * 's)) =
    fun prev globalState ->
        let fVal,fState = f prev globalState
        (fVal,fState,globalState)

// delayed value
let t1 acc seed  =
    M(
        fun prev globalState ->
            let x = match prev with
                    | Some previousState -> previousState
                    | None -> seed
            acc x globalState
    )

// Usage
let inline counter inc seed =
    let f = ((fun prev -> prev + inc) |> lift_ab)
    t1 f seed
let toggle seed = 
        t1 
        <| ((fun prev -> if (prev = true) then (0,false) else (1, true)) |> lift_b)
        <| seed
 