
[<Struct>]
type StatefulResult<'a,'b> = { value: 'a; state: 'b }

type L<'v,'s,'r> =
    | L of ('s option -> 'r -> StatefulResult<'v, 's>)
let run m = match m with | L x -> x

// [<Struct>]
// type WrappedState<'a,'b> = 

let bind (m:L<'a,'sa,'r>) (f:'a -> L<'b,'sb,'r>) : L<'b, ('sa * 'sb), 'r> =
    let stateFunc localState readerState =
        let prevAState,prevBState = match localState with
                                    | None       -> (None, None)
                                    | Some (a,b) -> (Some a, Some b)
        let a = run m prevAState readerState
        let fRes = f a.value
        let b = run fRes prevBState readerState
        { value = b.value; state = a.state,b.state }
    L stateFunc
let ret x = L (fun _ _ -> {value=x; state=()} )
let retFrom l = l

// computation builder
type LoopBuilder() =
    member __.Bind (m, f) = bind m f
    member __.Return x = ret x
    member __.ReturnFrom l = retFrom l
let loop = LoopBuilder()



[<Struct>]
type FeedbackResult<'a,'b> = { feedback:'a; out:'b }

/// Feedback
let (-=>) seed (f: 'a -> L<FeedbackResult<'a, 'v>,'s,'r>) =
    let f1 = fun prev r ->
        let myPrev,innerPrev = match prev with
                                | None            -> seed,None
                                | Some (my,inner) -> my,inner
        let lRes = run (f myPrev) innerPrev r
        let feed = lRes.value
        let innerState = lRes.state
        { value = feed.out; state = feed.feedback,Some innerState }
    L f1


(*
    Lifting functions  
*)

// TODO: seems wrong...
// /// Lifts a function who's value is fed into the next cycle as state.
// let liftV (f:('v * 's) -> 'r -> 'v) =
//     fun p r ->
//         let fVal = f p r
//         {value=fVal; state=fVal}

/// Lifts a function that doesn't use global reader state.
let liftR (f:'s -> StatefulResult<'v,'s>) =
    fun p r -> f p

/// Lifts a function that doesn't use global reader state
/// and who's value is fed into the next cycle as state.
let liftRV (f:'v -> 'v) =
    fun p r ->
        let fVal = f p
        {value=fVal; state=fVal}

/// Lifts a function with an initial value.
let liftSeed seed l =
    fun p r ->
        let x = match p with
                | Some previousState -> previousState
                | None -> seed
        l x r

/// TODO
let delay seed current =
    let f prev = {value=prev; state=current}
    liftR f |> liftSeed seed |> L



/// Gets the global reader state.
let read() = L(fun p r -> {value=r; state=()})




/// Converts a looping monad into a sequence.
/// The getReaderState function is called for each evaluation.
let toSequence getReaderState (l:L<_,_,_>) =
    let mutable lastState : 'a option = None
    Seq.initInfinite (fun i ->
        let res = (run l) lastState (getReaderState i)
        lastState <- Some res.state
        res.value
    )
