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

    // Lifts a function with a seed value to a Block function.
    let liftSeed seed statefulFunc =
        let f prev readerState =
            let x = match prev with
                    | Some previousState -> previousState
                    | None -> seed
            statefulFunc x readerState
        Block f
 
    let getState() =
        let f prev readerState = (readerState,())
        Block f

    // let interceptState (f: 'r -> Block<_,_,_>) =
    //     let g prev readerState =
    //         let block = f readerState
    //         let innerBlock = (run block)
    //         innerBlock prev readerState
    //     Block g
    
    let toSequence prepareState (block:Block<_,_,_>) =
        let mutable lastState : 'a option = None
        Seq.initInfinite (fun i ->
            let value,newState = (run block) lastState (prepareState i)
            lastState <- Some newState
            value
        )
