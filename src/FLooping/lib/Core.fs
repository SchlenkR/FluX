namespace FLooping

[<AutoOpen>]
module Core =

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
    type LoopBaseBuilder() =
        member __.Bind (m, f) = bind m f
        member __.Return x = ret x
        member __.ReturnFrom l = retFrom l
    let loopBase = LoopBaseBuilder()


    // computation builder
    type LoopGenBuilder<'a>() =
        member __.Bind (m:L<_,_,'a>, f) = bind m f
        member __.Return x = ret x
        member __.ReturnFrom l = retFrom l
    let loopGen<'a> = LoopGenBuilder<'a>()


[<AutoOpen>]
module Helper =

    /// Reads the global state that is passed around to every loop function.
    let read() = L(fun p r -> { value=r; state=()} )

    /// Lifts a function with an initial value.
    let liftSeed seed l =
        fun p r ->
            let x = match p with
                    | Some previousState -> previousState
                    | None -> seed
            l x r

[<AutoOpen>]
module Arithmetic =

    let map l f =
        let f1 = fun p r ->
            let res = run l p r
            let mappedRes = f res.value
            { value=mappedRes; state=res.state }
        L f1
    let (<!>) = map

    let (<*>) f l =
        loopBase {
            let! r1 = l
            let! r2 = f r1
            return r2
        }
    
    // type L<'v,'s,'r> with
    //     static member (+) (first: L<float,'s,'r>, second: L<float,'s,'r>) : ('v) =
    //         loopBase {
    //             let! r1 = first
    //             let! r2 = second
    //             return r1 + r2
    //         }

    let inline private binOpBoth left right f =
        loopBase {
            let! r1 = left
            let! r2 = right
            return f r1 r2
        }
    let inline private binOpLeft left right f =
        loopBase {
            let! r1 = left
            return f r1 right
        }
    let inline private binOpRight left right f =
        loopBase {
            let! r2 = right
            return f left r2
        }
    let inline (.+.) left right = binOpBoth left right (+)
    let inline (.+) left right = binOpLeft left right (+)
    let inline (+.) left right = binOpRight left right (+)

    let inline (.-.) left right = binOpBoth left right (-)
    let inline (.-) left right = binOpLeft left right (-)
    let inline (-.) left right = binOpRight left right (-)

    let inline (.*.) left right = binOpBoth left right (+)
    let inline (.*) left right = binOpLeft left right (*)
    let inline ( *.) left right = binOpRight left right (*)

    let inline (./.) left right = binOpBoth left right (/)
    let inline (./) left right = binOpLeft left right (/)
    let inline (/.) left right = binOpRight left right (/)

    let inline (.^.) left right = binOpBoth left right (fun l r -> System.Math.Pow(l, r))
    let inline (.^) left right = binOpLeft left right (^^)
    let inline (^.) left right = binOpRight left right (^^)


[<AutoOpen>]
module Feedback =

    [<Struct>]
    type Fbd<'a,'b> = { feedback:'a; out:'b }

    /// Feedback with reader state
    let (<=>) seed (f:'a -> 'r -> L<Fbd<'a,'v>,'s,'r>) =
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

    /// Feedback without reader state
    let (<->) seed f = (<=>) seed (fun p _ -> f p)
