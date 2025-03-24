module Client.Common.Components.DemoWindow

open Imp
open Client.Common.ViewHelpers
open Imp.SpriteHelpers

let render key = 
    component (key = key) <| fun hooks ->
        ui.Window (x = 10, y = 150, width = 300, height = 200) <| fun _ -> 
            view (x = 10, y = 10) {
                let clicked = hooks.UseState false

                ui.Button (
                    label = "Click",
                    width = 80,
                    height = 32,
                    mouseUp = (fun _ -> clicked.Update(fun x -> not x))
                )

                if clicked.Current then
                    ui.Text (y = 80) "ON"
            }

let testText key =
    ui.TextInput(
        key = key,
        x = 0,
        y = 400,
        width = 100,
        height = 100) "SOME TEXT"