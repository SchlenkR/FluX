// Origin: "DAS IST ES - Ma_a_Mb_Mc (2).fsx"

module FSiren

type M<'v, 's, 'g> =
    | M of ('s option -> 'g -> 'v * 's)
let run m = match m with | M x -> x

type CircuitBuilder() =

    member this.Bind (
                      m: M<'a, 'AState, 'g>, 
                      f: ('a -> M<'b, 'BState, 'g>))  
            : M<'b, ('AState * 'BState), 'g> =
        
        let stateFunc localState globalState =
            let prevAState, prevBState = match localState with
                                            | None -> (None, None)
                                            | Some (a,b) -> (Some a, Some b)
            let aValue,aState = run m prevAState globalState
            let fRes = f aValue
            let bValue,bState = run fRes prevBState globalState
            bValue,(aState,bState)
        M stateFunc

    member this.Return x = M(fun _ b -> (x,()))
let circuit = CircuitBuilder()


///
// Convenience
///
type Cont<'v, 'g> =
    | Cont of ('g -> 'v * Cont<'v, 'g>)
    member x.Eval = match x with Cont b -> b
let start (m:M<_,_,_>) =
    let rec evalInternal oldState globalState = 
        let v,newS = run m oldState globalState
        (v, Cont(fun g -> evalInternal (Some newS) g))
    evalInternal None


///
// Helper
///

type F<'v, 's, 'g> = ('s -> 'g -> ('v * 's))

// a : value as state
// b : no global state
// c : read only global state
let lift_a (f:('a * 'b) -> 'g -> ('a * 'b)) =
    fun prev globalState ->
        let fVal = f prev globalState
        (fVal,fVal)
let lift_ab (f:'v -> 'v) =
    fun prev globalState ->
        let fVal = f prev
        (fVal,fVal)
let lift_ac (f:'s -> 'g -> 'v) =
    fun prev globalState ->
        let fVal = f prev globalState
        (fVal,fVal)
let lift_b (f:'s -> ('v * 's)) =
    fun prev globalState ->
        let fVal,fState = f prev
        (fVal,fState)
let lift_c (f:'s -> 'g -> ('v * 's)) =
    fun prev globalState ->
        let fVal,fState = f prev globalState
        (fVal,fState)

// Block builder for functions with 1 past value.
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
    let f prev = prev + inc
    let lifted = f |> lift_ab
    t1 lifted seed
let toggle seed =
    let f prev = if (prev = true) then (0,false) else (1, true)
    let lifted = f |> lift_b
    t1 lifted seed
 