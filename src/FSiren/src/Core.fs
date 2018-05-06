// Origin: "DAS IST ES - Ma_a_Mb_Mc (2).fsx"

namespace FSiren

open Fable.Core

[<AutoOpen>]
module Core =

    type Gen<'v,'s,'r> =
        | Gen of ('s option -> 'r -> ('v * 's))
    let run m = match m with | Gen x -> x

    type CircuitBuilder() =
        member this.Bind (m:Gen<'a,'sa,'r>, f:('a -> Gen<'b,'sb,'r>)) : Gen<'b, ('sa * 'sb), 'r> =
            let stateFunc localState readerState =
                let prevAState, prevBState = match localState with
                                             | None -> (None, None)
                                             | Some (a,b) -> (Some a, Some b)
                let aValue,aState = run m prevAState readerState
                let fRes = f aValue
                let bValue,bState = run fRes prevBState readerState
                bValue,(aState,bState)
            Gen stateFunc
        member this.Return x = Gen(fun _ b -> (x,()))
    let circuit = CircuitBuilder()


    // Convenience
    type Cont<'v, 'r> =
        | Cont of ('r -> 'v * Cont<'v, 'r>)
        member x.Eval = match x with Cont b -> b
    let start (m:Gen<_,_,_>) =
        let rec evalInternal oldState readerState = 
            let v,newS = run m oldState readerState
            (v, Cont(fun g -> evalInternal (Some newS) g))
        evalInternal None


    // Lifting:
    // v : value as state
    // r : no reader state
    // s : no internal state

    let lift_v (f:('a * 'b) -> 'r -> ('a * 'b)) =
        fun prev readerState ->
            let fVal = f prev readerState
            (fVal,fVal)
    let lift_r (f:'s -> ('v * 's)) =
        fun prev readerState ->
            let fVal,fState = f prev
            (fVal,fState)
    let lift_rv (f:'v -> 'v) =
        fun prev readerState ->
            let fVal = f prev
            (fVal,fVal)
    let lift_s (f:'r -> 'v) =
        fun prev readerState ->
            let fVal = f readerState
            (fVal,())

    // Gen builder for handling seed values.
    let build statefulFunc seed  =
        let genFunc prev readerState =
            let x = match prev with
                    | Some previousState -> previousState
                    | None -> seed
            statefulFunc x readerState
        Gen(genFunc)
 