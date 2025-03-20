[<AutoOpen>]
module rec Imp.DisplayModels

open System
open Imp.GL.Models

#nowarn "40"

let internal renderViewPool = Pool<_>(
    (fun () -> RenderView()),
    (fun (renderView : RenderView) -> 
        renderView.Current.Clear()
        renderView.Last.Clear()
    )
)

let createRenderView key =
    let r = renderViewPool.Borrow()
    r.Key <- key
    r


[<Struct>]
type TextureDetails =
    { Texture : GLTexture
      Width : uint
      Height : uint
     }


        
[<Struct>]
type RenderProperties =
    { Time : float32
      Resolution : Size
      Offset : Point3
      VertexBuffer : GLBuffer
      //Items : Map<string, obj>
      TextureDetails : TextureDetails
      TextureLayer : string -> uint voption
      ZRange : int * int }

// Elements

type ElementCollection = ResizeArray<Element>

[<Sealed>]
type ElementSprite() = 
    let mutable x = 0
    let mutable y = 0
    let mutable z = 0
    let mutable width = 0
    let mutable height = 0
    let mutable texture = ""
    let mutable textureX = 0
    let mutable textureY = 0
    let mutable textureWidth = 0
    let mutable textureHeight = 0
    let mutable metaData = ValueNone

    static member internal Pool = Pool<_>((fun () -> ElementSprite()), (fun s -> ()))
    
    member _.X with get() = x
    member internal _.X with set value = x <- value

    member _.Y with get() = y
    member internal _.Y with set value = y <- value

    member _.Z with get() = z
    member internal _.Z with set value = z <- value

    member _.Width with get() = width
    member internal _.Width with set value = width <- value

    member _.Height with get() = height
    member internal _.Height with set value = height <- value

    member _.Texture with get() = texture
    member internal _.Texture with set value = texture <- value

    member _.TextureX with get() = textureX
    member internal _.TextureX with set value = textureX <- value

    member _.TextureY with get() = textureY
    member internal _.TextureY with set value = textureY <- value

    member _.TextureWidth with get() = textureWidth
    member internal _.TextureWidth with set value = textureWidth <- value

    member _.TextureHeight with get() = textureHeight
    member internal _.TextureHeight with set value = textureHeight <- value

    member _.MetaData with get() = metaData
    member internal _.MetaData with set value = metaData <- value

    member this.Return() = ElementSprite.Pool.Return(this)


type [<Struct>] ElementView =
    { Key : string
      Elements : ElementCollection
      X : int
      Y : int  
      Z : int
      MouseMove : (Point -> unit) voption
      MouseEnter : (Point -> unit) voption
      MouseLeave : (Point -> unit) voption
      MouseUp : (Point -> unit) voption
      Width : int
      Height : int
      Blur : (Point -> unit) voption }

type [<Struct>] ElementRenderer =
    { Key : string
      Create : (GL -> RenderView -> RendererBase) 
      ViewItems : ElementCollection }

type [<Struct>] ElementContext =
    { Key : string
      Func : RenderProperties -> RenderProperties
      ViewItems : ElementCollection }

type [<Struct>] ElementComponent = 
    { Key : string
      Prop : obj
      Compare : obj -> obj -> bool
      Build : Hooks -> obj -> ElementView

      }

type [<Struct>] [<RequireQualifiedAccess>] Element = 
    | View of view : ElementView
    | Sprite of sprite : ElementSprite
    | Renderer of renderer : ElementRenderer
    | Context of context : ElementContext
    | Component of com : ElementComponent

// Render level

type RenderView() =
    let mutable current = ResizeArray<RenderItem voption>()
    let mutable last = ResizeArray<RenderItem voption>()

    let mutable key = ""
    let mutable hasSprites = false
    let mutable hasRenderers = false
    let mutable parent : RenderView voption = ValueNone
    let mutable x = 0
    let mutable y = 0
    let mutable z = 0
    let mutable width = 0
    let mutable height = 0
    let mutable mouseMove : (Point -> unit) voption = ValueNone
    let mutable mouseOver = false
    let mutable mouseEnter : (Point -> unit) voption = ValueNone
    let mutable mouseLeave : (Point -> unit) voption = ValueNone
    let mutable mouseUp : (Point -> unit) voption = ValueNone
    let mutable blur : (Point -> unit) voption = ValueNone
    let mutable isActive = false

    member _.Key with get() = key
    member internal _.Key with set value = key <- value

    member _.HasSprites with get() = hasSprites
    member internal _.HasSprites with set value = hasSprites <- value
    
    member _.HasRenderers with get() = hasRenderers
    member internal _.HasRenderers with set value = hasRenderers <- value

    member _.Parent with get() = parent
    member internal _.Parent with set value = parent <- value

    member _.X with get() = x
    member internal _.X with set value = x <- value

    member _.Y with get() = y
    member internal _.Y with set value = y <- value

    member _.Z with get() = z
    member internal _.Z with set value = z <- value

    member _.Width with get() = width
    member internal _.Width with set value = width <- value

    member _.Height with get() = height
    member internal _.Height with set value = height <- value

    member _.MouseMove with get() = mouseMove
    member internal _.MouseMove with set value = mouseMove <- value

    member _.MouseOver with get() = mouseOver
    member internal _.MouseOver with set value = mouseOver <- value

    member _.MouseEnter with get() = mouseEnter
    member internal _.MouseEnter with set value = mouseEnter <- value

    member _.MouseLeave with get() = mouseLeave
    member internal _.MouseLeave with set value = mouseLeave <- value

    member _.MouseUp with get() = mouseUp
    member internal _.MouseUp with set value = mouseUp <- value

    member _.Blur with get() = blur
    member internal _.Blur with set value = blur <- value

    member _.IsActive with get() = isActive
    member internal _.IsActive with set value = isActive <- value

    member internal _.Current : ResizeArray<RenderItem voption> = current
    member internal _.Last : ResizeArray<RenderItem voption> = last

    member _.Swap () =
        let l = last
        last <- current
        current <- l

    member this.Distory useLast =
        let items = if useLast then this.Last else this.Current
        for i = 0 to items.Count - 1 do
            let item = items.[i]
            match item with
            | ValueSome item ->
                match item with
                | RenderItem.Renderer r ->
                    r.Renderer.Distory ()
                | RenderItem.View v ->
                    v.Distory false
                | RenderItem.Sprite s ->
                    s.Return()
                | RenderItem.Context c ->
                    c.RenderView.Distory false
                | RenderItem.Component c ->
                    c.Removed <- true
                    c.RenderView.Distory false
            | _ -> ()

        if not useLast then
            renderViewPool.Return this

type [<AbstractClass>] RendererBase(g : GL, f : RenderView) =
    let mutable isStale = true

    member this.Recreate() =
        isStale <- true

    abstract member OnInit: RenderProperties -> unit
    default this.OnInit(rp) = ()

    member this.Render rp = 
        if isStale then 
            this.OnInit rp

        this.OnRender rp
        isStale <- false

    abstract member OnRender: RenderProperties -> unit

    member this.Distory unit = 
        f.Distory false
        this.OnDistory()

    abstract member OnDistory: unit -> unit
    default this.OnDistory() = ()

type [<Struct>] RenderRenderer =
    { Key : string
      Renderer : RendererBase
      RenderView : RenderView }

type [<Struct>] RenderContext =
    { Key : string
      RenderView : RenderView
      Callback : RenderProperties -> RenderProperties }

[<RequireQualifiedAccess>]
type Key = 
    //| Unknown = -1
    | Space = 32
    | Apostrophe = 39
    | Comma = 44
    | Minus = 45
    | Period = 46
    | Slash = 47
    | Number0 = 48
    | D0 = 48
    | Number1 = 49
    | Number2 = 50
    | Number3 = 51
    | Number4 = 52
    | Number5 = 53
    | Number6 = 54
    | Number7 = 55
    | Number8 = 56
    | Number9 = 57
    | Semicolon = 59
    | Equal = 61
    | A = 65
    | B = 66
    | C = 67
    | D = 68
    | E = 69
    | F = 70
    | G = 71
    | H = 72
    | I = 73
    | J = 74
    | K = 75
    | L = 76
    | M = 77
    | N = 78
    | O = 79
    | P = 80
    | Q = 81
    | R = 82
    | S = 83
    | T = 84
    | U = 85
    | V = 86
    | W = 87
    | X = 88
    | Y = 89
    | Z = 90
    | LeftBracket = 91
    | BackSlash = 92
    | RightBracket = 93
    | GraveAccent = 96
    | World1 = 161
    | World2 = 162
    | Escape = 256
    | Enter = 257
    | Tab = 258
    | Backspace = 259
    | Insert = 260
    | Delete = 261
    | Right = 262
    | Left = 263
    | Down = 264
    | Up = 265
    | PageUp = 266
    | PageDown = 267
    | Home = 268
    | End = 269
    | CapsLock = 280
    | ScrollLock = 281
    | NumLock = 282
    | PrintScreen = 283
    | Pause = 284
    | F1 = 290
    | F2 = 291
    | F3 = 292
    | F4 = 293
    | F5 = 294
    | F6 = 295
    | F7 = 296
    | F8 = 297
    | F9 = 298
    | F10 = 299
    | F11 = 300
    | F12 = 301
    | F13 = 302
    | F14 = 303
    | F15 = 304
    | F16 = 305
    | F17 = 306
    | F18 = 307
    | F19 = 308
    | F20 = 309
    | F21 = 310
    | F22 = 311
    | F23 = 312
    | F24 = 313
    | F25 = 314
    | Keypad0 = 320
    | Keypad1 = 321
    | Keypad2 = 322
    | Keypad3 = 323
    | Keypad4 = 324
    | Keypad5 = 325
    | Keypad6 = 326
    | Keypad7 = 327
    | Keypad8 = 328
    | Keypad9 = 329
    | KeypadDecimal = 330
    | KeypadDivide = 331
    | KeypadMultiply = 332
    | KeypadSubtract = 333
    | KeypadAdd = 334
    | KeypadEnter = 335
    | KeypadEqual = 336
    | ShiftLeft = 340
    | ControlLeft = 341
    | AltLeft = 342
    | SuperLeft = 343
    | ShiftRight = 344
    | ControlRight = 345
    | AltRight = 346
    | SuperRight = 347
    | Menu = 348

[<RequireQualifiedAccess>]
type ControllerButton = 
    //| A
    | DPadUp
    | DPadDown
    | DPadLeft
    | DPadRight

type InputEvent = 
    | MouseMove of int * int
    | MouseButton of bool * int * int
    | SyntheticMouseButton of bool * int64
    | Key of bool * Key
    | ControllerThumbstick of float * float
    | ControllerButton of bool * ControllerButton

type InputHandlerMessage =
    | ControllerThumbstick of float * float
    | ControllerButton of bool * ControllerButton
    | Key of pressed : bool * key : Key


type RenderComponent = 
    { Key : string
      Hooks : ResizeArray<obj>
      mutable CurrentHook : int
      mutable Stale : bool
      RenderView : RenderView
      mutable CurrentProp : obj
      Build : Hooks -> obj -> ElementView
      mutable Removed : bool

      mutable InputHandler : (InputHandlerMessage -> unit) voption

      mutable HasMouseEvents : bool
      mutable HasInputEvents : bool
      mutable Id : int64
      }

and [<Struct;RequireQualifiedAccess>] RenderItem =
    | View of view : RenderView
    | Sprite of sprite : ElementSprite
    | Renderer of renderer : RenderRenderer
    | Context of context : RenderContext
    | Component of component : RenderComponent

type [<Struct>] Hooks = 
    { Component : RenderComponent }

#if !FABLE_COMPILER
type ConcurrentStack<'a> = System.Collections.Concurrent.ConcurrentStack<'a>
#else
type ConcurrentStack<'a> = System.Collections.Generic.Stack<'a>
    //let stack = new ResizeArray<'a>();
    //member _.Push(item) =
    //    stack.Insert(0, item);

    //member 
#endif

let internal updatedComponents = ConcurrentStack<_>()

let internal registerComponentForUpdate currentComponent =
    updatedComponents.Push(currentComponent)

type [<Sealed>] HookState<'T>(currentComponent : RenderComponent, hookIndex, initValue : 'T) =
    do
        currentComponent.Hooks.[hookIndex] <- initValue

    member _.Current  with get () : 'T = unbox currentComponent.Hooks.[hookIndex]
    member _.Update(fn : 'T -> 'T) =
        let current =  currentComponent.Hooks.[hookIndex] |> unbox |> fn
        currentComponent.Hooks.[hookIndex] <- current
        currentComponent.Stale <- true

        registerComponentForUpdate(currentComponent)

type [<Struct>] Callback = 
    { Time : DateTime
      Call : unit -> unit
      Component : RenderComponent voption }

type [<RequireQualifiedAccess;Struct>] MouseEvent = 
    | Move
    | Up
    | SyntheticUp of int64