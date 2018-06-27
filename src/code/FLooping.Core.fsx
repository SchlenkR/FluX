
type L<'v,'s,'r> =
    | L of ('s option -> 'r -> ('v * 's))
let run m = match m with | L x -> x

let bind (m:L<'a,'sa,'r>) (f:'a -> L<'b,'sb,'r>) : L<'b, ('sa * 'sb), 'r> =
    let stateFunc localState readerState =
        let prevAState,prevBState = match localState with
                                     | None       -> (None, None)
                                     | Some (a,b) -> (Some a, Some b)
        let aValue,aState = run m prevAState readerState
        let fRes = f aValue
        let bValue,bState = run fRes prevBState readerState
        bValue,(aState,bState)
    L stateFunc
let ret x = L (fun _ _ -> (x,()))
let retFrom l = l

// computation builder
type LoopBuilder() =
    member __.Bind (m, f) = bind m f
    member __.Return x = ret x
    member __.ReturnFrom l = retFrom l
let loop = LoopBuilder()



type Feed<'a, 'b> = { back:'a; out:'b }
/// Feedback
let (-=>) seed (f: 'a -> L<Feed<'a, 'v>,'s,'r>) =
    let f1 = fun prev r ->
        let myPrev,innerPrev = match prev with
                                | None            -> seed,None
                                | Some (my,inner) -> my,inner
        let l = f myPrev
        let monad = (run l)
        let feed,innerState = monad innerPrev r
        let res = (feed.out, (feed.back, Some innerState))
        res
    L f1


(*
    Lifting functions  
*)

/// Lifts a function who's value is fed into the next cycle as state.
let liftV (f:('a * 'b) -> 'r -> ('a * 'b)) =
    fun p r ->
        let fVal = f p r
        (fVal,fVal)

/// Lifts a function that doesn't use global reader state.
let liftR (f:'s -> ('v * 's)) =
    fun p r ->
        let fVal,fState = f p
        (fVal,fState)

/// Lifts a function that doesn't use global reader state
/// and who's value is fed into the next cycle as state.
let liftRV (f:'v -> 'v) =
    fun p r ->
        let fVal = f p
        (fVal,fVal)

/// Lifts a function that has no internal state.
let liftPure (f:'r -> 'v) = fun p r -> (f r,())

/// Lifts a function with an initial value.
let liftSeed seed l =
    fun p r ->
        let x = match p with
                | Some previousState -> previousState
                | None -> seed
        l x r

/// TODO
let delay seed current =
    let f prev = (prev,current)
    liftR f |> liftSeed seed |> L



/// Gets the global reader state.
let read() = L(fun p r -> (r,()))




/// Converts a looping monad into a sequence.
/// The getReaderState function is called for each evaluation.
let toSequence getReaderState (l:L<_,_,_>) =
    let mutable lastState : 'a option = None
    Seq.initInfinite (fun i ->
        let value,newState = (run l) lastState (getReaderState i)
        lastState <- Some newState
        value
    )
