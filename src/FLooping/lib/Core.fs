namespace FLooping

[<AutoOpen>]
module Monad =

    [<Struct>]
    type Res<'a,'b> = { value: 'a; state: 'b }

    type L<'v,'s,'r> =
        | L of ('s option -> 'r -> Res<'v, 's>)
    let run m = match m with | L x -> x

    [<Struct>]
    type S<'a,'b> = { mine:'a; other:'b }

    let bind (m:L<'a,'sa,'r>) (f:'a -> L<'b,'sb,'r>) : L<'b, S<'sa,'sb>, 'r> =
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
module Helper =

    /// Reads the global state that is passed around to every loop function.
    let read() = L(fun p r -> {value=r; state=()})

    /// Lifts a function with an initial value.
    let liftSeed seed l =
        fun p r ->
            let x = match p with
                    | Some previousState -> previousState
                    | None -> seed
            l x r


[<AutoOpen>]
module Feedback =

    [<Struct>]
    type F<'a,'b> = { feedback:'a; out:'b }

    /// Feedback with reader state
    let (=+>) seed (f:'a -> 'r -> L<F<'a,'v>,'s,'r>) =
        let f1 = fun prev r ->
            let myPrev,innerPrev = 
                match prev with
                | None            -> seed,None
                | Some (my,inner) -> my,inner
            let lRes = run (f myPrev r) innerPrev r
            let feed = lRes.value
            let innerState = lRes.state
            { value = feed.out; state = feed.feedback,Some innerState }
        L f1

    /// Feedback
    let (=->) seed (f:'a -> L<F<'a, 'v>,'s,'r>) = 
        (=+>) seed (fun p _ -> f p)

    let map (l:L<'v,'s,'r>) (f:'v->'x) : L<'x,'s,'r> =
        let f1 = fun p r ->
            let res = run l p r
            let mappedRes = f res.value
            { value=mappedRes; state=res.state }
        L f1
    let (<!>) = map
