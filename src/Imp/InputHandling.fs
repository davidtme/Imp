[<AutoOpen>]
module Imp.InputHandling

#if !FABLE_COMPILER

open Silk.NET.Input
open Imp

type private SKey = Silk.NET.Input.Key
type private MKey = Imp.DisplayModels.Key

let private keyMapping s =
    match s with
    | SKey.W -> ValueSome MKey.W
    | SKey.A -> ValueSome MKey.A
    | SKey.S -> ValueSome MKey.S
    | SKey.D -> ValueSome MKey.D

    | _ -> 
        ValueNone

let private buttonMap s =
    match s with
    | ButtonName.DPadUp -> ValueSome ControllerButton.DPadUp
    | ButtonName.DPadDown -> ValueSome ControllerButton.DPadDown
    | ButtonName.DPadLeft -> ValueSome ControllerButton.DPadLeft
    | ButtonName.DPadRight -> ValueSome ControllerButton.DPadRight
    | _ -> ValueNone
    

//type IInputContext with
//    member input.AttachKeyboard () =
//        for keyboard in input.Keyboards do
//            keyboard.add_KeyDown(fun _ k _ ->
//                match keyMapping k with
//                | ValueSome mapped -> inputEvents.Enqueue(InputEvent.Key(true, mapped))
//                | _ -> ()
//            )

//            keyboard.add_KeyUp(fun _ k _ ->
//                match keyMapping k with
//                | ValueSome mapped -> inputEvents.Enqueue(InputEvent.Key(false, mapped))
//                | _ -> ()
//            )

//    member input.AttachMouse () =
//        for mouse in input.Mice do
//            mouse.add_MouseMove(fun mouse v -> 
//                inputEvents.Enqueue(InputEvent.MouseMove(int v.X, int v.Y))
//            )
            
//            mouse.add_MouseUp(fun mouse v -> 
//                inputEvents.Enqueue(InputEvent.MouseButton(true, int mouse.Position.X, int mouse.Position.Y))
//            )

//    member input.AttachGamepad () =
//        let attach (gamepad : IGamepad) =
//            gamepad.Deadzone <- Deadzone(0.0f, DeadzoneMethod.Traditional)

//            gamepad.add_ThumbstickMoved(fun p b ->
//                if b.Index = 0 then
//                    let x = if abs b.X < 0.02f then 0.0f else b.X
//                    let y = if abs b.Y < 0.02f then 0.0f else b.Y
//                    inputEvents.Enqueue(InputEvent.ControllerThumbstick(float x,float y))
//            )

//            gamepad.add_ButtonDown(fun p b ->
//                if p.Index = 0 then
//                    match buttonMap b.Name with
//                    | ValueSome mapped -> inputEvents.Enqueue(InputEvent.ControllerButton(true, mapped))
//                    | _ -> ())  

//            gamepad.add_ButtonUp(fun p b ->
//                if p.Index = 0 then
//                    match buttonMap b.Name with
//                    | ValueSome mapped -> inputEvents.Enqueue(InputEvent.ControllerButton(false, mapped))
//                    | _ -> ())  


//        input.add_ConnectionChanged (fun device attached -> 
//            if attached then
//                match device with
//                | :? IGamepad as gamepad -> 
//                    attach gamepad
//                | _ -> ()
//        )

//        for gamepad in input.Gamepads do
//            attach gamepad

type Browser.Types.Window with
    member window.AttachKeyboard () = invalidOp "Fable Only"
    member window.AttachMouse () = invalidOp "Fable Only"

#else

//open Fable.Core.JsInterop

//let private keyMapping s =
//    match s with
//    | "KeyW" -> ValueSome Key.W
//    | "KeyA" -> ValueSome Key.A
//    | "KeyS" -> ValueSome Key.S
//    | "KeyD" -> ValueSome Key.D
//    | _ -> 
//        printfn $"Key: {s}"
//        ValueNone

//type Input() = 
//    member _.AttachKeyboard () =
//        Browser.Dom.window.addEventListener("keydown", fun e -> 
//            match keyMapping e?code with
//            | ValueSome mapped -> inputEvents.Enqueue(InputEvent.Key(true, mapped))
//            | _ -> ()
//        )

//        Browser.Dom.window.addEventListener("keyup", fun e -> 
//            match keyMapping e?code with
//            | ValueSome mapped -> inputEvents.Enqueue(InputEvent.Key(false, mapped))
//            | _ -> ()
//        )

//    member _.AttachMouse () =
//        Browser.Dom.window.addEventListener("mousemove", fun e -> 
//            inputEvents.Enqueue(InputEvent.MouseMove(int e?clientX , int e?clientY))

//        )

//        Browser.Dom.window.addEventListener("mouseup", fun e -> 
//            inputEvents.Enqueue(InputEvent.MouseButton(true, int e?clientX, int e?clientY))
//        )

//    member _.AttachGamepad () = 
//        ()

#endif