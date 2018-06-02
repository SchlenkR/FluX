namespace FLooping

[<AutoOpen>]
module Core =

    type Block<'v,'s,'r> =
        | Block of ('s option -> 'r -> ('v * 's))
    let run m = match m with | Block x -> x

    let bind (m:Block<'a,'sa,'r>) (f:'a -> Block<'b,'sb,'r>) : Block<'b, ('sa * 'sb), 'r> =
        let stateFunc localState readerState =
            let prevAState, prevBState = match localState with
                                            | None -> (None, None)
                                            | Some (a,b) -> (Some a, Some b)
            let aValue,aState = run m prevAState readerState
            let fRes = f aValue
            let bValue,bState = run fRes prevBState readerState
            bValue,(aState,bState)
        Block stateFunc
    let ret x = Block (fun _ b -> (x,()))

    // computation builder
    type BlockBuilder() =
        member this.Bind (m, f) = bind m f
        member this.Return x = ret x
    let block = BlockBuilder()


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
    let liftPure (f:'r -> 'v) =
        fun prev readerState ->
            let fVal = f readerState
            (fVal,())

    /// Lifts a function with an initial value.
    let liftSeed seed statefulFunc =
        let f prev readerState =
            let x = match prev with
                    | Some previousState -> previousState
                    | None -> seed
            statefulFunc x readerState
        Block f
 
    /// Gets the global reader state.
    let getState() =
        let f prev readerState = (readerState,())
        Block f

    /// Converts a looping monad into a sequence.
    /// The getReaderState function is called for each evaluation.
    let toSequence getReaderState (block:Block<_,_,_>) =
        let mutable lastState : 'a option = None
        Seq.initInfinite (fun i ->
            let value,newState = (run block) lastState (getReaderState i)
            lastState <- Some newState
            value
        )
