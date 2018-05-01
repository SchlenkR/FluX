// Origin: "DAS IST ES - Ma_a_Mb_Mc (2).fsx"

namespace FSiren

[<AutoOpen>]
module Core =

    type M<'v, 's, 'r> =
        | M of ('s option -> 'r -> 'v * 's)
    let run m = match m with | M x -> x

    type CircuitBuilder() =
        member this.Bind (m:M<'a,'sa,'r>, f:('a -> M<'b,'sb,'r>)) : M<'b,('sa*'sb),'r> =
            let stateFunc localState readerState =
                let prevAState, prevBState = 
                    match localState with
                    | None -> (None, None)
                    | Some (a,b) -> (Some a, Some b)
                let aValue,aState = run m prevAState readerState
                let fRes = f aValue
                let bValue,bState = run fRes prevBState readerState
                bValue,(aState,bState)
            M stateFunc
        member this.Return x = M(fun _ b -> (x,()))
    let circuit = CircuitBuilder()


    // Convenience
    type Cont<'v, 'r> =
        | Cont of ('r -> 'v * Cont<'v, 'r>)
        member x.Eval = match x with Cont b -> b
    let start (m:M<_,_,_>) =
        let rec evalInternal oldState readerState = 
            let v,newS = run m oldState readerState
            (v, Cont(fun g -> evalInternal (Some newS) g))
        evalInternal None


    ///
    // Helper
    ///

    // a : value as state
    // b : no reader state
    let lift_a (f:('a * 'b) -> 'r -> ('a * 'b)) =
        fun prev readerState ->
            let fVal = f prev readerState
            (fVal,fVal)
    let lift_ab (f:'v -> 'v) =
        fun prev readerState ->
            let fVal = f prev
            (fVal,fVal)
    let lift_b (f:'s -> ('v * 's)) =
        fun prev readerState ->
            let fVal,fState = f prev
            (fVal,fState)

    // Block builder for functions with 1 past value.
    let t1 acc seed  =
        M(
            fun prev readerState ->
                let x = match prev with
                        | Some previousState -> previousState
                        | None -> seed
                acc x readerState
        )
 