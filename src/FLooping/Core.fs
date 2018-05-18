namespace FLooping

[<AutoOpen>]
module Core =

    // the gen monad
    type Gen<'v,'s,'r> =
        | Gen of ('s option -> 'r -> ('v * 's))
    let run m = match m with | Gen x -> x

    let bind (m:Gen<'a,'sa,'r>) (f:'a -> Gen<'b,'sb,'r>) : Gen<'b, ('sa * 'sb), 'r> =
        let stateFunc localState readerState =
            let prevAState, prevBState = match localState with
                                            | None -> (None, None)
                                            | Some (a,b) -> (Some a, Some b)
            let aValue,aState = run m prevAState readerState
            let fRes = f aValue
            let bValue,bState = run fRes prevBState readerState
            bValue,(aState,bState)
        Gen stateFunc
    let ret x = Gen (fun _ b -> (x,()))

    // computation builder
    type LoopBuilder() =
        member this.Bind (m, f) = bind m f
        member this.Return x = ret x
    let loop = LoopBuilder()

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
    let liftSeed statefulFunc seed  =
        let f prev readerState =
            let x = match prev with
                    | Some previousState -> previousState
                    | None -> seed
            statefulFunc x readerState
        Gen f
 
    let getState() =
        let f prev readerState = (readerState,())
        Gen f

    // TODO: Ist das noch sinnvoll? getState() ist mächtiger.
    let interceptState (f: 'r -> Gen<_,_,_>) =
        let g prev readerState =
            let gen = f readerState
            let innerGen = (run gen)
            innerGen prev readerState
        Gen g
    
    let toSequence prepareState (gen:Gen<_,_,_>) =
        let mutable lastState : 'a option = None
        Seq.initInfinite (fun i ->
            let value,newState = (run gen) lastState (prepareState i)
            lastState <- Some newState
            value
        )
