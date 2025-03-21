module Client.Common.Components.FrameRate

open Imp
open Client.Common.ViewHelpers

let mutable frameCounter = 0

let rec frameCounterLogic (hooks : Hooks) (fps : HookState<_>) _ =
    fps.Update(fun _ -> frameCounter)
    frameCounter <- 0
    hooks.RegisterDelay 1000 (frameCounterLogic hooks fps)

let render key = 
    component (key = key) <| fun hooks -> 
        view (x = 400) {
            let fps = hooks.UseState 0
            hooks.UseEffect () (frameCounterLogic hooks fps)
            ui.Text (x=10,y=10) $"FPS: %d{fps.Current}"
        }