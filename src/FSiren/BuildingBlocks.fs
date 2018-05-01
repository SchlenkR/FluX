namespace FSiren

[<AutoOpen>]
module BuildingBlocks =

    let inline counter seed inc =
        let f prev = prev + inc
        let lifted = f |> lift_ab
        t1 lifted seed

    let toggle seed =
        let f prev = if (prev = true) then (0,false) else (1, true)
        let lifted = f |> lift_b
        t1 lifted seed
