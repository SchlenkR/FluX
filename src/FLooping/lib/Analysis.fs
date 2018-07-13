#if !INTERACTIVE
namespace FLooping
#endif

open System
open System.Threading

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
