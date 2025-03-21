module Client.Common.Components.DemoMouse

open Imp
open Client.Common.ViewHelpers

let render key = 
    component (key = key) <| fun hooks ->
        let message = hooks.UseStateLazy <| fun _ -> [ "Nothing" ]

        view (x = 10, y = 10) {

            ui.Text () message.Current.Head
                
            // Try with an offset.
            view (
                key = "Outer",
                x = 0,
                y = 40,
                mouseEnter = (fun _ -> message.Update(fun message -> [ "Outer" ] @ message )),
                mouseLeave = (fun _ -> message.Update(fun message -> match message with _ :: rest -> rest | _ -> [])),
                height = 100,
                width = 118) {

                ui.Text () "Outer"

                view (
                    key = "Inner",
                    x = 0,
                    y = 40,
                    mouseEnter = (fun _ -> message.Update(fun message -> [ "Inner" ] @ message )),
                    mouseLeave = (fun _ -> message.Update(fun message -> match message with _ :: rest -> rest | _ -> [])),
                    height = 19,
                    width = 118) {
                            
                    ui.Text () "Inner"
                }
            }
        }