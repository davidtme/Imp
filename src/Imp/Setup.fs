module Imp.Setup

open System
open Imp.GL
type private MKey = Imp.DisplayModels.Key

[<AbstractClass>]
type DataManager() =
    let textureNames = System.Collections.Generic.HashSet<string>()

    member _.TextureNames = textureNames
    member _.AddTextureName name = textureNames.Add(name) |> ignore

    abstract member LoadTexture<'a>: string -> (TextureInfo -> unit) -> unit
    abstract member LoadString<'a>: string -> (string -> unit) -> unit

type Input =
    abstract member AttachKeyboard : unit -> unit
    abstract member AttachMouse : unit -> unit
    abstract member AttachGamepad : unit -> unit

[<AbstractClass>]
type Display() =
    abstract member AttachInput: (Input -> unit) -> unit
    abstract member AttachView : (DataManager) -> (ElementView) -> ((unit -> unit) option) -> unit
    abstract member Run: unit -> unit

#if !FABLE_COMPILER

open Silk.NET.Windowing
open Silk.NET.Maths
open Silk.NET.Input

type ResourceDataManager(assembly : System.Reflection.Assembly, name) =
    inherit DataManager()

    let readTexture raw =
        let result = StbImageSharp.ImageResult.FromMemory(raw, StbImageSharp.ColorComponents.RedGreenBlueAlpha)
        { Pixels = result.Data; Width = uint result.Width; Height = uint result.Height }

    let assName = assembly.FullName.Substring(0, assembly.FullName.IndexOf(','))
    let manager = System.Resources.ResourceManager($"{assName}.{name}", assembly)

    override _.LoadTexture name resolve =
        let bytes : byte[] = manager.GetObject(name) |> unbox
        let texture = readTexture bytes
        resolve texture

    override _.LoadString name resolve =
        let bytes : byte[] = manager.GetObject(name) |> unbox
        let s = System.Text.Encoding.UTF8.GetString(bytes)
        resolve s

#endif

open Fable.Core
open Fetch

type WebDataManager (baseUrl) =
    inherit DataManager()
 
    override _.LoadTexture name resolve =
        #if FABLE_COMPILER
        let image = Browser.Dom.document.createElement("img") :?> Browser.Types.HTMLImageElement
        image.onload <- fun _ ->
            resolve { Pixels = image; Width = uint image.width; Height = uint image.height }

        image.src <- $"%s{baseUrl}/%s{name}"
        #else
        invalidOp "Fable Only"
        #endif

    override _.LoadString name resolve =
        promise {
            let! response = fetch $"%s{baseUrl}/%s{name}" []
            let! json = response.text()
            resolve json
        }
        |> Async.AwaitPromise
        |> ignore

let private attachView 
    (gl : GL) (dataManager: DataManager) view onReader complete =

    let textrueData = ResizeArray()
    let textureNames = dataManager.TextureNames |> Seq.toList

    let rec loop () =
        if textrueData.Count < List.length textureNames then
            let name = textureNames[textrueData.Count]

            dataManager.LoadTexture $"{name}.png" (fun a ->
                textrueData.Add((name, a))
                loop ()
            )
                
        else
            let textrueData =  textrueData |> Seq.toList

            let (texture, size, layers) = gl |> createLayeredTexture textrueData
            let textureDetails = { Texture = texture; Width = fst size; Height = snd size }

            let textureLayerLookup =
                let mutable lastTexture = ValueNone
                fun name ->
                match lastTexture with
                | ValueSome (n,v) when n = name ->
                    ValueSome v
                | _ ->
                    match layers.TryGetValue name with
                    | true, layer -> 
                        lastTexture <- ValueSome (name, layer)
                        ValueSome layer
                    | _ -> ValueNone        
        
            let vertexBuffer = gl |> createBuffer vertices
            let startTime = DateTime.UtcNow

            let rootRender = createRenderView "Root"
            applyView rootRender gl view

            let renderCallback = fun resolution ->
                onReader |> Option.iter(fun x -> x())

                let time = (DateTime.UtcNow - startTime).TotalSeconds |> float32

                rootRender.Renderers
                    { Offset = { X = 0; Y = 0; Z = 0 }
                      Time = time
                      Resolution = resolution
                      VertexBuffer = vertexBuffer
                      TextureDetails = textureDetails
                      TextureLayer = textureLayerLookup
                      //Items = Map.empty
                      ZRange = 0,1 }


            complete renderCallback (fun _ -> updateComponents rootRender gl)

    loop()

#if !FABLE_COMPILER

type private SKey = Silk.NET.Input.Key

type OpenGLDisplay(width, height, tickRate, title) =
    inherit Display()

    let window =
        let mutable options = WindowOptions.Default
        options.Size <- Vector2D(width, height);
        options.Title <- title
        options.WindowBorder <- WindowBorder.Fixed
        options.UpdatesPerSecond <- 1000.0 / float tickRate
        options.FramesPerSecond <- 60
        options.VSync <- true
    
        Window.Create(options)

    let keyMapping (s : SKey) =
        let i = int s
        if i <> -1 then
            ValueSome (enum<MKey>(i))
        else
            ValueNone

    let buttonMap s =
        match s with
        | ButtonName.DPadUp -> ValueSome ControllerButton.DPadUp
        | ButtonName.DPadDown -> ValueSome ControllerButton.DPadDown
        | ButtonName.DPadLeft -> ValueSome ControllerButton.DPadLeft
        | ButtonName.DPadRight -> ValueSome ControllerButton.DPadRight
        | _ -> ValueNone

    override _.AttachInput(fn) =
        window.add_Load(fun _ -> 
            let input = window.CreateInput()
            let inter = 
                { new Input with 
                    member _.AttachKeyboard() = 
                        for keyboard in input.Keyboards do
                            keyboard.add_KeyDown(fun _ k _ ->
                                match keyMapping k with
                                | ValueSome mapped -> inputEvents.Enqueue(InputEvent.Key(true, mapped))
                                | _ -> ()
                            )

                            keyboard.add_KeyUp(fun _ k _ ->
                                match keyMapping k with
                                | ValueSome mapped -> inputEvents.Enqueue(InputEvent.Key(false, mapped))
                                | _ -> ()
                            )

                    member _.AttachMouse () =
                        for mouse in input.Mice do
                            mouse.add_MouseMove(fun mouse v -> 
                                inputEvents.Enqueue(InputEvent.MouseMove(int v.X, int v.Y))
                            )
            
                            mouse.add_MouseUp(fun mouse v -> 
                                inputEvents.Enqueue(InputEvent.MouseButton(true, int mouse.Position.X, int mouse.Position.Y))
                            )

                    member _.AttachGamepad () =
                        let attach (gamepad : IGamepad) =
                            gamepad.Deadzone <- Deadzone(0.0f, DeadzoneMethod.Traditional)

                            gamepad.add_ThumbstickMoved(fun p b ->
                                if b.Index = 0 then
                                    let x = if abs b.X < 0.02f then 0.0f else b.X
                                    let y = if abs b.Y < 0.02f then 0.0f else b.Y
                                    inputEvents.Enqueue(InputEvent.ControllerThumbstick(float x,float y))
                            )

                            gamepad.add_ButtonDown(fun p b ->
                                if p.Index = 0 then
                                    match buttonMap b.Name with
                                    | ValueSome mapped -> inputEvents.Enqueue(InputEvent.ControllerButton(true, mapped))
                                    | _ -> ())  

                            gamepad.add_ButtonUp(fun p b ->
                                if p.Index = 0 then
                                    match buttonMap b.Name with
                                    | ValueSome mapped -> inputEvents.Enqueue(InputEvent.ControllerButton(false, mapped))
                                    | _ -> ())  


                        input.add_ConnectionChanged (fun device attached -> 
                            if attached then
                                match device with
                                | :? IGamepad as gamepad -> 
                                    attach gamepad
                                | _ -> ()
                        )

                        for gamepad in input.Gamepads do
                            attach gamepad
                    
                }
            fn inter
        )

    override _.AttachView(dataManager: DataManager) (view) (onReader) =
        window.add_Load(fun _ -> 
            let gl = GL.GetApi(window)
            attachView gl dataManager view onReader <| fun render update ->
            window.add_Render(fun _ -> render { Width = window.Size.X; Height = window.Size.Y })
            window.add_Update(update)
        )

    override _.Run() =
        window.Run()

#endif

open Fable.Core.JsInterop

type WebGLDisplay(elementId, width, height) =
    inherit Display()
    let mutable resolution = { Width = 0; Height = 0 }
    let mutable tick = ignore

    let canvas = 
        let canvas = Browser.Dom.document.createElement("canvas") :?> Browser.Types.HTMLCanvasElement
        let holder = Browser.Dom.document.getElementById(elementId)
        holder.appendChild(canvas) |> ignore

        let resized _ =
            let width = width//holder.clientWidth
            let height = height//holder.clientHeight
            resolution <- { Width = int width; Height = int height }
        
            canvas.width <- width
            canvas.height <- height

        resized()
        canvas

    let keyMapping s =
        match s with
        | "KeyA" -> ValueSome MKey.A
        | "KeyB" -> ValueSome MKey.B
        | "KeyC" -> ValueSome MKey.C
        | "KeyD" -> ValueSome MKey.D
        | "KeyE" -> ValueSome MKey.E
        | "KeyF" -> ValueSome MKey.F
        | "KeyG" -> ValueSome MKey.G
        | "KeyH" -> ValueSome MKey.H
        | "KeyI" -> ValueSome MKey.I
        | "KeyJ" -> ValueSome MKey.J
        | "KeyK" -> ValueSome MKey.K
        | "KeyL" -> ValueSome MKey.L
        | "KeyM" -> ValueSome MKey.M
        | "KeyN" -> ValueSome MKey.N
        | "KeyO" -> ValueSome MKey.O
        | "KeyP" -> ValueSome MKey.P
        | "KeyQ" -> ValueSome MKey.Q
        | "KeyR" -> ValueSome MKey.R
        | "KeyS" -> ValueSome MKey.S
        | "KeyT" -> ValueSome MKey.T
        | "KeyU" -> ValueSome MKey.U
        | "KeyV" -> ValueSome MKey.V
        | "KeyW" -> ValueSome MKey.W
        | "KeyX" -> ValueSome MKey.X
        | "KeyY" -> ValueSome MKey.Y
        | "KeyZ" -> ValueSome MKey.Z
        | "Digit0" -> ValueSome MKey.Number0
        | "Digit1" -> ValueSome MKey.Number1
        | "Digit2" -> ValueSome MKey.Number2
        | "Digit3" -> ValueSome MKey.Number3
        | "Digit4" -> ValueSome MKey.Number4
        | "Digit5" -> ValueSome MKey.Number5
        | "Digit6" -> ValueSome MKey.Number6
        | "Digit7" -> ValueSome MKey.Number7
        | "Digit8" -> ValueSome MKey.Number8
        | "Digit9" -> ValueSome MKey.Number9
        | "Space" -> ValueSome MKey.Space
        | "ShiftLeft" -> ValueSome MKey.ShiftLeft
        | "ShiftRight" -> ValueSome MKey.ShiftRight
        | "Backspace" -> ValueSome MKey.Backspace
        | "Delete" -> ValueSome MKey.Delete
        | "Semicolon" -> ValueSome MKey.Semicolon
        | "ArrowLeft" -> ValueSome MKey.Left
        | "ArrowRight" -> ValueSome MKey.Right
        | "ArrowDown" -> ValueSome MKey.Down
        | "ArrowUp" -> ValueSome MKey.Up
        | "Backquote" -> ValueSome MKey.GraveAccent
        | "Minus" -> ValueSome MKey.Minus
        | "Equal" -> ValueSome MKey.Equal
        | "BracketLeft" -> ValueSome MKey.LeftBracket
        | "BracketRight" -> ValueSome MKey.RightBracket
        | "Backslash" -> ValueSome MKey.BackSlash
        | "Quote" -> ValueSome MKey.Apostrophe
        | "Comma" -> ValueSome MKey.Comma
        | "Period" -> ValueSome MKey.Period
        | "Slash" -> ValueSome MKey.Slash
        | "CapsLock" -> ValueSome MKey.CapsLock
        | "Tab" -> ValueSome MKey.Tab
        | "Enter" -> ValueSome MKey.Enter
        | "Escape" -> ValueSome MKey.Escape
        | "ControlLeft" -> ValueSome MKey.ControlLeft
        | "ControlRight" -> ValueSome MKey.ControlRight
        | "AltLeft" -> ValueSome MKey.AltLeft
        | "AltRight" -> ValueSome MKey.AltRight

        | _ -> 
            printfn $"Key: {s}"
            ValueNone

    override _.AttachView(dataManager: DataManager) (view) (onReader) =
        #if FABLE_COMPILER

        let gl : Browser.Types.WebGLRenderingContext = canvas.getContext("webgl2", {| premultipliedAlpha = false |}) |> unbox
        attachView gl dataManager view onReader <| fun render update ->
            tick <- 
                fun _ ->
                    update()
                    render resolution
        #else
        invalidOp "Fable only"
        #endif

    override _.AttachInput(fn) =
        let inter = 
            { new Input with 
                member _.AttachKeyboard () =
                        Browser.Dom.window.addEventListener("keydown", fun e -> 
                            match keyMapping e?code with
                            | ValueSome mapped -> inputEvents.Enqueue(InputEvent.Key(true, mapped))
                            | _ -> ()
                        )

                        Browser.Dom.window.addEventListener("keyup", fun e -> 
                            match keyMapping e?code with
                            | ValueSome mapped -> inputEvents.Enqueue(InputEvent.Key(false, mapped))
                            | _ -> ()
                        )

                    member _.AttachMouse () =
                        Browser.Dom.window.addEventListener("mousemove", fun e -> 
                            inputEvents.Enqueue(InputEvent.MouseMove(int e?clientX , int e?clientY))

                        )

                        Browser.Dom.window.addEventListener("mouseup", fun e -> 
                            inputEvents.Enqueue(InputEvent.MouseButton(true, int e?clientX, int e?clientY))
                        )

                    member _.AttachGamepad () = 
                        ()
            }

        fn inter


    override _.Run() =
        let rec loop _ =
            Browser.Dom.window.requestAnimationFrame(fun _ -> 
                tick()
                loop()
            ) |> ignore

        loop ()

