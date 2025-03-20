[<AutoOpen>]
module Imp.Flow

open System
open Imp

#if FABLE_COMPILER
open Fable.Core.JsInterop
#endif

let mutable private componentCounter = 0l


let private applyProps (renderView : RenderView) (view : ElementView) =
    renderView.X <- view.X   
    renderView.Y <- view.Y
    renderView.Z <- view.Z
    renderView.Width <- view.Width
    renderView.Height <- view.Height
    renderView.MouseMove <- view.MouseMove
    renderView.MouseEnter <- view.MouseEnter
    renderView.MouseLeave <- view.MouseLeave
    renderView.MouseUp <- view.MouseUp
    renderView.Blur <- view.Blur

let private bubbleComponentEvent  (parentComponent : RenderComponent voption) (view : RenderView) =
    parentComponent |> ValueOption.iter(fun c ->
        c.HasMouseEvents <- 
            c.HasMouseEvents || 
            view.MouseEnter.IsSome ||
            view.MouseLeave.IsSome ||
            view.MouseMove.IsSome ||
            view.MouseUp.IsSome ||
            view.Blur.IsSome

        c.HasInputEvents <-
            c.HasInputEvents ||
            c.InputHandler.IsSome
    )

let rec private applyViewItem isStale (parentComponent : RenderComponent voption) gl (parent : RenderView) (item : Element) =
    let inline tryPickFromParent fn =
        let mutable found = ValueNone
        let mutable index = 0
        while found.IsNone && index < parent.Last.Count do 
            match fn (parent.Last.[index]) with
            | ValueSome x -> 
                parent.Last.[index] <- ValueNone
                found <- ValueSome (x)
            | _ -> ()

            index <- index + 1

        found

    match item with
    | Element.Sprite sprite ->
        parent.Current.Add(RenderItem.Sprite sprite |> ValueSome)
        parent.HasSprites <- true

    | Element.View view ->
        let renderView = 
            match tryPickFromParent (function ValueSome (RenderItem.View f) when f.Key = view.Key -> ValueSome f | _ -> ValueNone) with
            | ValueSome found ->
                found

            | _ -> 
                let found = createRenderView view.Key                  
                found
           
        renderView.Parent <- ValueSome parent   
        applyProps renderView view
        applyViewItemCollection isStale parentComponent gl renderView view.Elements
        
        parent.HasSprites <- parent.HasSprites || renderView.HasSprites
        parent.HasRenderers <- parent.HasRenderers || renderView.HasRenderers
        parent.Current.Add(ValueSome (RenderItem.View renderView))

        bubbleComponentEvent parentComponent renderView

    | Element.Component viewComponent ->
        let found = 
            match tryPickFromParent (function ValueSome (RenderItem.Component x) when x.Key = viewComponent.Key -> ValueSome x | _ -> ValueNone) with
            | ValueSome found ->
                let same = 
                    try
                        viewComponent.Compare found.CurrentProp viewComponent.Prop
                    with e ->
                        reraise()

                found.Stale <- found.Stale || not same
                found.CurrentProp <- box viewComponent.Prop
                found

            | _ ->
                let renderView = createRenderView viewComponent.Key

                componentCounter <- componentCounter + 1

                let created : RenderComponent =
                    { Key = viewComponent.Key
                      Hooks = ResizeArray()
                      CurrentHook = 0
                      Stale = true
                      RenderView = renderView
                      CurrentProp = viewComponent.Prop
                      Build = viewComponent.Build
                      Removed = false
                      InputHandler = ValueNone
                      HasMouseEvents = false
                      HasInputEvents = false
                      Id = componentCounter
                      }

                created


        registerComponentForUpdate found
        found.RenderView.Parent <- ValueSome parent

        parent.HasRenderers <- true
        parent.HasSprites <- true
        //parent.HasComponents <- true

        parentComponent |> ValueOption.iter(fun c -> 
            c.HasMouseEvents <- c.HasMouseEvents || found.HasMouseEvents
            c.HasInputEvents <- c.HasInputEvents || found.HasInputEvents
            )

        parent.Current.Add(ValueSome (RenderItem.Component found))

    | Element.Renderer viewRenderer -> 
        let create() =
            let renderView = createRenderView viewRenderer.Key
            renderView.Parent <- ValueSome parent
            applyViewItemCollection isStale parentComponent gl renderView viewRenderer.ViewItems

            let found = 
                { Key = viewRenderer.Key
                  Renderer = viewRenderer.Create gl renderView
                  RenderView = renderView  }

            parent.Current.Add(RenderItem.Renderer found |> ValueSome)
        
        if isStale then
            create()
        else
            match tryPickFromParent (function ValueSome (RenderItem.Renderer r) when r.Key = viewRenderer.Key -> ValueSome r | _ -> ValueNone) with
            | ValueSome found ->
                found.RenderView.Parent <- ValueSome parent
                applyViewItemCollection isStale parentComponent gl found.RenderView viewRenderer.ViewItems
                parent.Current.Add(RenderItem.Renderer found |> ValueSome)

            | _ ->
                create()

        parent.HasRenderers <- true
        parent.HasSprites <- true


    | Element.Context (viewContext) -> 
        let renderContext =
            match tryPickFromParent (function ValueSome (RenderItem.Context c) when c.Key = viewContext.Key -> ValueSome c | _ -> ValueNone) with
            | ValueSome found ->
                parent.Current.Add(RenderItem.Context found |> ValueSome)
                found

            | _ ->
                let found = 
                    { Key = viewContext.Key
                      RenderView = createRenderView viewContext.Key
                      Callback = viewContext.Func  }
                parent.Current.Add(RenderItem.Context found |> ValueSome)
                found


        applyViewItemCollection isStale parentComponent gl renderContext.RenderView viewContext.ViewItems
        
        parent.HasRenderers <- true
        parent.HasSprites <- true


and private applyViewItemCollection isStale parentComponent gl (renderView : RenderView) (items : ElementCollection) =
    renderView.Swap()
    renderView.Current.Clear()
    renderView.HasSprites <- false
    renderView.HasRenderers <- false

    for index = 0 to items.Count - 1 do
        applyViewItem isStale parentComponent gl renderView items.[index]

    elementCollectionPool.Return(items)

    renderView.Distory true
    renderView.Last.Clear()

    bubbleComponentEvent parentComponent renderView

let mutable private mouseBlur = ValueNone

type RenderView with
    member this.Sprites (fn) =
        let rec loop (current : ResizeArray<_>) x y z =
            for i = 0 to current.Count - 1 do
                let item = current.[i]
                match item with
                | ValueSome item ->
                    match item with
                    | RenderItem.Sprite sprite ->
                        fn (struct(x,y,z)) sprite

                    | RenderItem.View group  ->
                        if group.HasSprites then
                            loop group.Current (x + group.X) (y + group.Y) (z + group.Z)

                    | RenderItem.Context context ->
                        loop context.RenderView.Current x y z

                    | RenderItem.Component comp ->
                        loop comp.RenderView.Current (comp.RenderView.X + x) (comp.RenderView.Y + y) (comp.RenderView.Z + z)

                    | RenderItem.Renderer _ ->
                        ()


                | _ -> ()


        loop this.Current this.X this.Y this.Z

    member this.Renderers ctx =
        let ctx =
            if this.X <> 0 || this.Y <> 0 || this.Z <> 0 then
                { ctx with
                    Offset =
                        { X = ctx.Offset.X + this.X
                          Y = ctx.Offset.Y + this.Y
                          Z = ctx.Offset.Z + this.Z } }
            else
                ctx

        for i = 0 to this.Current.Count - 1 do
            let item = this.Current.[i]
            match item with
            | ValueSome item ->
                match item with
                | RenderItem.Renderer r ->
                    r.Renderer.Render ctx

                | RenderItem.View view ->
                    if view.HasRenderers then
                        view.Renderers ctx

                | RenderItem.Context context ->
                    let ctx' = context.Callback ctx
                    context.RenderView.Renderers ctx'

                | RenderItem.Component component ->
                    component.RenderView.Renderers ctx

                | RenderItem.Sprite _ -> ()

            | ValueNone -> ()

let internal applyView (this : RenderView) (gl : GL.Models.GL) (view : ElementView) =
    this.Swap()
    this.Current.Clear()
    this.HasSprites <- false
    this.HasRenderers <- false

    applyProps this view
    applyViewItem false ValueNone gl this (Element.View view)

    this.Distory true
    this.Last.Clear()

let rec private changed gl (item : RenderView voption) =
    match item with
    | ValueSome item ->
        for i = 0 to item.Current.Count - 1 do
            let renderItem = item.Current.[i]
            match renderItem with
            | ValueSome renderItem ->
                match renderItem with
                | RenderItem.Renderer renderer ->   
                    renderer.Renderer.Recreate ()
                    
                | _ -> ()

            | _ -> ()

        changed gl item.Parent
    | _ -> ()

let mutable private callbacks = ResizeArray<Callback>()
let mutable private callbacksOther = ResizeArray<Callback>()

#if !FABLE_COMPILER
type ConcurrentQueue<'a> = System.Collections.Concurrent.ConcurrentQueue<'a>
#else
type ConcurrentQueue<'a> = System.Collections.Generic.Queue<'a>
    //let stack = new ResizeArray<'a>();
    //member _.Push(item) =
    //    stack.Insert(0, item);

    //member 
#endif

let internal inputEvents = ConcurrentQueue<InputEvent>()

[<RequireQualifiedAccess>]
type SyntheticEvent =
    | Up

type Hooks with
    member private this.nextHook() =
        let index = this.Component.CurrentHook

        this.Component.CurrentHook <- index + 1

        index,
        match this.Component.Hooks |> Seq.tryItem index with
        | Some x -> unbox x |> ValueSome
        | _ -> ValueNone

    member this.UseState value =
        let index, item = this.nextHook()

        let currentState =
            match item with
            | ValueSome s -> s
            | _ -> 
                this.Component.Hooks.Add(box value)
                value

        HookState(this.Component, index, currentState)

    member this.UseStateLazy fn =
        let index, item = this.nextHook()

        let currentState =
            match item with
            | ValueSome s -> s
            | _ -> 
                let s = fn ()
                this.Component.Hooks.Add(box s)
                s

        HookState(this.Component, index, currentState)

    member this.TriggerSyntheticEvent fn =
        let _, item = this.nextHook()

        match item with
        | ValueSome _ -> ()
        | _ -> 
            match fn () with
            | SyntheticEvent.Up ->
                inputEvents.Enqueue (InputEvent.SyntheticMouseButton (true, this.Component.Id))
            
            this.Component.Hooks.Add(true)

        this.Component.HasMouseEvents <- true

    member this.UseEffect (prop) fn =
        let index, item = this.nextHook()

        match item with
        | ValueSome lastProp ->   
            if lastProp <> prop then
                fn prop
                this.Component.Hooks.[index] <- box prop

        | _ -> 
            fn prop
            this.Component.Hooks.Add(prop)

    member this.HandleInput fn =
        this.Component.InputHandler <- ValueSome fn
        this.Component.HasInputEvents <- true
        ()

    member this.RegisterDelay milliseconds fn =
        callbacks.Add({ Time = DateTime.UtcNow.AddMilliseconds(milliseconds); Call = fn; Component = ValueSome this.Component })
        ()

    static member RegisterDelay milliseconds fn =
        callbacks.Add({ Time = DateTime.UtcNow.AddMilliseconds(milliseconds); Call = fn; Component = ValueNone })
        ()

    static member private ProcessCallbacks() = 
        let x = callbacks
        callbacks <- callbacksOther
        callbacksOther <- x

        let now = DateTime.UtcNow

        for item in callbacksOther do
            let ok () = 
                if now >= item.Time then
                    item.Call()
                else
                    callbacks.Add(item)

            match item.Component with
            | ValueNone -> ok()
            | ValueSome component when not component.Removed -> ok ()
            | _ -> ()

        callbacksOther.Clear()

let internal updateComponents (this : RenderView) (gl) =
        Hooks.ProcessCallbacks ()

        let mouse mouseX mouseY event =
            let mutable cancel = false

            let rec loop id (current : ResizeArray<_>) offsetX offsetY =
                //if not cancel then
                    for i = 0 to current.Count - 1 do
                        let item = current.[i]
                        let loopView id (view : RenderView) =
                            // Do the childen first for overlaps
                            loop id view.Current (offsetX + view.X) (offsetY + view.Y)

                            //if not  then
                            let bounds () = 
                                { X = offsetX + view.X
                                  Y = offsetY + view.Y
                                  Width = view.Width
                                  Height = view.Height }

                            match event with
                            | MouseEvent.Move ->
                                if not cancel && (view.MouseMove.IsSome || view.MouseEnter.IsSome || view.MouseLeave.IsSome) then
                                    let bounds = bounds()
    
                                    if bounds.Inside(mouseX, mouseY) then
                                        if not view.MouseOver then
                                            view.MouseEnter
                                            |> ValueOption.iter(fun fn -> 
                                                fn { X = mouseX; Y = mouseY }
                                                view.MouseOver <- true
                                            )
                                    
                                        view.MouseMove |> ValueOption.iter(fun fn -> fn { X = mouseX; Y = mouseY })


                                    elif view.MouseOver then
                                        view.MouseLeave |> ValueOption.iter(fun fn -> fn { X = mouseX; Y = mouseY })
                                        view.MouseOver <- false

                            | MouseEvent.Up ->
                                if not cancel && view.MouseUp.IsSome then
                                    let bounds = bounds()
                                    if bounds.Inside(mouseX, mouseY) then 
                                        if mouseBlur.IsSome then
                                            mouseBlur.Value({ Point.X = mouseX; Y = mouseY })

                                        //if not view.IsActive then
                                        view.MouseUp.Value({ X = mouseX; Y = mouseY })
                                        cancel <- true
                                        //view.IsActive <- true

                                        mouseBlur <- view.Blur

                            | MouseEvent.SyntheticUp (id') ->
                                if id' = id then
                                    let bounds = bounds()
                                    inputEvents.Enqueue(InputEvent.MouseButton (true, bounds.X, bounds.Y))
                                    cancel <- true
                                

                        match item with
                        | ValueSome item ->
                            match item with
                            | RenderItem.Sprite _ ->
                                ()

                            | RenderItem.View view ->
                                loopView id view

                            | RenderItem.Component comp ->
                                if comp.HasMouseEvents then
                                    loopView comp.Id comp.RenderView

                            | RenderItem.Context context ->
                                loop id context.RenderView.Current offsetX offsetY


                            | RenderItem.Renderer renderer ->
                                loop id renderer.RenderView.Current (offsetX + renderer.RenderView.X) (offsetY + renderer.RenderView.Y)


                        | _ -> ()

            loop -1 this.Current 0 0

        let input state =
            let mutable cancel = false

            let rec loop (current : ResizeArray<_>) =
                if not cancel then
                    for i = 0 to current.Count - 1 do
                        let item = current.[i]
                        match item with
                        | ValueSome item ->
                            match item with
                            | RenderItem.Sprite _ ->
                                ()

                            | RenderItem.Component component ->
                                if component.HasInputEvents then
                                    // Do the childen first for overlaps
                                    loop component.RenderView.Current

                                    if not cancel then
                                        match component.InputHandler with
                                        | ValueSome fn ->
                                            fn state
                                            cancel <- true

                                        | _ ->
                                            ()

                            | RenderItem.View view -> 
                                loop view.Current

                            | RenderItem.Context context ->
                                loop context.RenderView.Current

                            | RenderItem.Renderer renderer ->
                                loop renderer.RenderView.Current


                        | _ -> ()

            loop this.Current

        let rec inputLoop () =
            match inputEvents.TryDequeue() with 
            | true, event ->
                match event with
                | InputEvent.MouseMove (x,y) -> mouse x y MouseEvent.Move
                | InputEvent.MouseButton (true, x, y) -> mouse x y MouseEvent.Up
                | InputEvent.MouseButton (false, x, y) -> () // this.Mouse(x, y, MouseEvent.Up)
                | InputEvent.SyntheticMouseButton (true, id) -> mouse 0 0 (MouseEvent.SyntheticUp id)
                | InputEvent.SyntheticMouseButton (false, _) -> ()
                
                | InputEvent.Key (down, key) -> input (InputHandlerMessage.Key (down, key))

                | InputEvent.ControllerThumbstick (x,y) -> input (InputHandlerMessage.ControllerThumbstick (x,y))
                | InputEvent.ControllerButton(down, button) -> input (InputHandlerMessage.ControllerButton (down, button))

                inputLoop()

            | _ -> ()

        inputLoop()


        let rec componentLoop () =
            match updatedComponents.TryPop() with
            | true, viewComponent ->
                if viewComponent.Stale then
                    viewComponent.Stale <- false
                    viewComponent.CurrentHook <- 0
                    viewComponent.InputHandler <- ValueNone
                    viewComponent.HasMouseEvents <- false
                    viewComponent.HasInputEvents <- false

                    let view = viewComponent.Build { Component = viewComponent } viewComponent.CurrentProp
                    applyProps viewComponent.RenderView view
                    applyViewItemCollection true (ValueSome viewComponent) gl viewComponent.RenderView view.Elements
                    changed gl viewComponent.RenderView.Parent

                componentLoop()
            | _ -> 
                ()

        componentLoop ()

