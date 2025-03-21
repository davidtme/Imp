module Client.Common.Views

open Imp
open Imp.Setup
open Imp.SpriteHelpers
open Client.Common.ViewHelpers
open Client.Common.Components

let private rootView () =
    view (z = 1) {
        context (fun ctx -> { ctx with ZRange = 0, 10 }) {
            sceneRenderer (
                backgroundColor = { R = 0; G = 130; B = 130 }) {

                batchRenderer () {
                    FrameRate.render "FrameRate"
                    DemoWindow.render "DemoWindow"
                    DemoMouse.render "DemoMouse"
                    DemoWindow.textInput "DemoMouse"
                }

                //batchRenderer () {
                //    DemoPlayers.render "Players"
                //}

                SmallWorld.render "SmallWorld"
            }
        }
    }

let private onReader() = 
    Components.FrameRate.frameCounter <- Components.FrameRate.frameCounter + 1

let init (dataManager : DataManager) (display : Display) =
    dataManager.PopulateSprites "sprites.json" ui.Sprites <| fun _ ->

    display.AttachInput <| fun input -> 
        input.AttachKeyboard()
        input.AttachGamepad()
        input.AttachMouse()

    display.AttachView
        dataManager
        (rootView())
        (Some onReader)

    display.Run()