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
                    DemoWindow.testText "DemoMouse"
                }

                SmallWorld.render "SmallWorld"
            }
        }
    }
    

let private onReader() = 
    Components.FrameRate.frameCounter <- Components.FrameRate.frameCounter + 1

let sampleView () =
    view () {
        sceneRenderer (
            backgroundColor = { R = 30; G = 30; B = 30 }) {

            singleRenderer () {
                sprite (
                    x = 50,
                    y = 50,
                    z = 0,
                    width = 32,
                    height = 32,
                    texture = "mega",
                    textureX = 563,
                    textureY = 791,
                    textureWidth = 32,
                    textureHeight = 32
                )
            }
        }
    }

let init (dataManager : DataManager) (display : Display) =
    dataManager.PopulateSprites "sprites.json" ui.Sprites <| fun _ ->

    display.AttachInput <| fun input -> 
        input.AttachKeyboard()
        input.AttachGamepad()
        input.AttachMouse()

    display.AttachView
        dataManager
        (rootView ())
        (Some onReader)

    display.Run()