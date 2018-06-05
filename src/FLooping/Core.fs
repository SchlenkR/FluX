namespace FLooping

[<AutoOpen>]
module Core =

    type L<'v,'s,'r> =
        | L of ('s option -> 'r -> ('v * 's))
    let run m = match m with | L x -> x

    //type RetVal<'a,'b> =
    //    | V of 'a
    //    | VS of ('a * 'b)

    let bind (m:L<'a,'sa,'r>) (f:'a -> L<'b,'sb,'r>) : L<'b, ('sa * 'sb), 'r> =
        let stateFunc localState readerState =
            let prevAState, prevBState = match localState with
                                         | None -> (None, None)
                                         | Some (a,b) -> (Some a, Some b)
            let aValue,aState = run m prevAState readerState
            let fRes = f aValue
            let bValue,bState = run fRes prevBState readerState
            bValue,(aState,bState)
        L stateFunc
    let ret x = L (fun _ b -> (x,()))
    //let ret x = match x with
    //            | v -> L (fun _ b -> (x,()))
    //            | (v,s) -> L (fun _ b -> (v,s))
    let retFrom l = l

    // computation builder
    type CircuitBuilder() =
        member this.Bind (m, f) = bind m f
        member this.Return x = ret x
        member this.ReturnFrom l = retFrom l
    let circuit = CircuitBuilder()


    (*
        Lifting functions  
    *)

    /// Lifts a function who's value is fed into the next cycle as state.
    let liftV (f:('a * 'b) -> 'r -> ('a * 'b)) =
        fun prev readerState ->
            let fVal = f prev readerState
            (fVal,fVal)
    
    /// Lifts a function that doesn't use global reader state.
    let liftR (f:'s -> ('v * 's)) =
        fun prev readerState ->
            let fVal,fState = f prev
            (fVal,fState)

    /// Lifts a function that doesn't use global reader state
    /// and who's value is fed into the next cycle as state.
    let liftRV (f:'v -> 'v) =
        fun prev readerState ->
            let fVal = f prev
            (fVal,fVal)

    /// Lifts a function that has no internal state.
    let liftPure (f:'r -> 'v) = fun p r -> (f r,())

    /// Lifts a function with an initial value.
    let liftSeed seed statefulFunc =
        let f prev readerState =
            let x = match prev with
                    | Some previousState -> previousState
                    | None -> seed
            statefulFunc x readerState
        L f

    // TODO: Naming for wraps

    /// Wraps a value into a looping function.
    let wrap1 v = L(fun p r -> (v,v))

    /// Wraps a value and a feedback state into a looping function.
    let wrap2 v s = L(fun p r -> (v,s))
 
    /// Gets the global reader state.
    let getEnv() = L(fun p r -> (r,()))
 
    /// Gets the local state.
    let getPrev() = L(fun p r -> (p,()))

    /// Converts a looping monad into a sequence.
    /// The getReaderState function is called for each evaluation.
    let toSequence getReaderState (l:L<_,_,_>) =
        let mutable lastState : 'a option = None
        Seq.initInfinite (fun i ->
            let value,newState = (run l) lastState (getReaderState i)
            lastState <- Some newState
            value
        )
