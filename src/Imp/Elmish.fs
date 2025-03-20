module Imp.Elmish

module Cmd =
    let inline ofMsg msg = [ fun dispatch -> do dispatch msg ]
    let none = List.empty

    let inline batch (cmds) =
        cmds |> List.concat

    let inline ofFunc a =
        [
            fun dispatch -> a dispatch 
        ]

    let inline ofAsync a =
        [
            fun dispatch ->
                a dispatch
                |> Async.StartImmediate 
        ]

    let inline delay milliseconds callback =
        ofFunc (fun dispatch -> 
            Hooks.RegisterDelay milliseconds <| fun _ -> 
                callback () |> dispatch
        )

let reducer key init update view =
    component (key = key) <| fun hooks ->
        let modelState = hooks.UseStateLazy <| fun _ -> init() |> Choice1Of2
        let dispatchState = hooks.UseState ignore

        let dispatch msg = dispatchState.Current msg

        hooks.UseEffect () <| fun _ -> 
            let cmd = 
                match modelState.Current with
                | Choice1Of2 (initModel, cmd) -> 
                    modelState.Update(fun _ -> Choice2Of2 initModel)
                    cmd
                | _ -> invalidOp ""
                        

            dispatchState.Update(fun _ -> 
                fun message ->
                    match modelState.Current with
                    | Choice2Of2 (model) -> 
                        let model', cmd = update message model
                        modelState.Update (fun _ -> Choice2Of2 model')

                        for cmd in cmd do
                            cmd dispatch

                    | _ ->
                        invalidOp ""
                )

            for cmd in cmd do
                cmd dispatch


        match modelState.Current with
        | Choice2Of2 (model) -> 
            view model dispatch

        | _ -> invalidOp ""