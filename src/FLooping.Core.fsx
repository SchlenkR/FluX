open System
open System.Threading

[<AutoOpen>]
module Monad =

    [<Struct>]
    type StatefulResult<'a,'b> = { value: 'a; state: 'b }

    type L<'v,'s,'r> =
        | L of ('s option -> 'r -> StatefulResult<'v, 's>)
    let run m = match m with | L x -> x

    [<Struct>]
    type WrappedState<'a,'b> = { mine:'a; other:'b }

    let bind (m:L<'a,'sa,'r>) (f:'a -> L<'b,'sb,'r>) : L<'b, WrappedState<'sa,'sb>, 'r> =
        let stateFunc localState readerState =
            let { mine=prevAState; other=prevBState } = 
                match localState with
                | None   -> {mine=None; other=None}
                | Some v -> {mine=Some v.mine; other=Some v.other}
            let a = run m prevAState readerState
            let fRes = f a.value
            let b = run fRes prevBState readerState
            { value = b.value; state = {mine=a.state; other=b.state} }
        L stateFunc
    
    let ret x = L (fun _ _ -> {value=x; state=()} )
    
    let retFrom l = l

    // computation builder
    type LoopBuilder() =
        member __.Bind (m, f) = bind m f
        member __.Return x = ret x
        member __.ReturnFrom l = retFrom l

    let loop = LoopBuilder()


[<AutoOpen>]
module Feedback =

    [<Struct>]
    type FeedbackResult<'a,'b> = { feedback:'a; out:'b }

    /// Feedback
    let (-=>) seed (f: 'a -> L<FeedbackResult<'a, 'v>,'s,'r>) =
        let f1 = fun prev r ->
            let myPrev,innerPrev = 
                match prev with
                | None            -> seed,None
                | Some (my,inner) -> my,inner
            let lRes = run (f myPrev) innerPrev r
            let feed = lRes.value
            let innerState = lRes.state
            { value = feed.out; state = feed.feedback,Some innerState }
        L f1


[<AutoOpen>]
module Lifting =

    (*
        Lifting functions  
    *)

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


[<AutoOpen>]
module Helper =

    /// Reads the global state that is passed around to every loop function.
    let read() = L(fun p r -> {value=r; state=()})


[<AutoOpen>]
module Seq =

    /// Converts a looping monad into a sequence.
    /// The getReaderState function is called for each evaluation.
    let toReaderSequence getReaderState (l:L<_,_,_>) =
        let mutable lastState : 'a option = None
        Seq.initInfinite (fun i ->
            let res = (run l) lastState (getReaderState i)
            lastState <- Some res.state
            res.value
        )

    let toIdSequence (l:L<'a,_,_>) : seq<'a> = toReaderSequence id l


[<AutoOpen>]
module BuildingBlocks =

    /// Delays a given value by 1 cycle.
    let delay seed current =
        let f prev = {value=prev; state=current}
        liftR f |> liftSeed seed |> L

    let counter (seed:float) (inc:float) =
        let f prev = prev + inc
        let lifted = liftRV f 
        lifted |> liftSeed seed |> L

    let counterAlt (seed:float) (inc:float) = 
        seed -=> fun last -> loop {
            let value = last + inc
            return {out=value; feedback=value}
    }

    let toggle seed =
        let f prev = if prev
                     then {value=0.0; state=false}
                     else {value=1.0; state=true}
        liftR f |> liftSeed seed |> L

    let noise() =
        let f (prev:Random) =
            let v = prev.NextDouble()
            {value=v; state=prev}
        liftR f |> liftSeed (new Random()) |> L



[<AutoOpen>]
module Analysis =

    let measure (time:TimeSpan) (s:seq<_>) =
        let enumerator = s.GetEnumerator()
        let mutable count = 0
        let mutable run = true
        let proc = fun _ ->
            while run do
                count <- count + 1
                enumerator.MoveNext() |> ignore
                enumerator.Current |> ignore
                ()
        let thread = new Thread (ThreadStart proc)
        thread.Start()
        
        Thread.Sleep time
        run <- false

        count

    // let measureSeq (time:TimeSpan) (s:seq<_>) =
    //     let startTime = DateTime.Now
    //     let enumerator = s.GetEnumerator()
    //     let mutable evaluations = 0
    //     while (DateTime.Now - startTime) < time do
    //         let max = 10000
    //         for _ in 1..max do
    //             enumerator.MoveNext() |> ignore
    //             enumerator.Current |> ignore
    //             ()
    //         evaluations <- evaluations + max
    //     (float evaluations) / time.TotalSeconds

    // type Measurable<'a,'b,'c> =
    //     | S of seq<'a>
    //     | L of L<'a,'b,'c>

    let compare (time:TimeSpan) (ls:seq<_> list) =
        let measured = ls |> List.map (fun l -> measure time l)
        match measured with
        | [] -> []
        | h::t -> (h,1.0) :: (t |> List.map (fun x -> (x,(float x) / (float h))))
