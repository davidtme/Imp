[<AutoOpen>]
module Imp.Dsl

let elementCollectionPool = Pool<_>((fun () -> ElementCollection()), (fun s -> s.Clear()))

// for if the path#line number comes back
//let createKey (key : string) (line : int) =
//    if key.EndsWith(".fs") then
//        key + "#" + line.ToString()
//    else
//        key

[<AbstractClass>]
type ElementBuilder () =
    member _.Borrow() = elementCollectionPool.Borrow()

    member inline _.Yield (item: ElementSprite) = fun (b: ElementCollection) -> b.Add (Element.Sprite item)
    member inline _.Yield (item: ElementView) = fun (b: ElementCollection) -> b.Add (Element.View item)
    member inline _.Yield (item: ElementRenderer) = fun (b: ElementCollection) -> b.Add (Element.Renderer item)
    member inline _.Yield (item: ElementContext) = fun (b: ElementCollection) -> b.Add (Element.Context item)
    member inline _.Yield (item: ElementComponent) = fun (b: ElementCollection) -> b.Add (Element.Component item)

    member inline _.Yield (items: ElementCollection) = fun (b: ElementCollection) -> 
        b.AddRange(items)
        elementCollectionPool.Return(items)

    member inline _.YieldFrom (items: ElementSprite seq) = fun (b: ElementCollection) -> 
        for item in items do
            b.Add (Element.Sprite item)

    member inline _.Combine ([<InlineIfLambda>]f, [<InlineIfLambda>]g) = fun (b: ElementCollection) -> f b; g b
    member inline _.Delay ([<InlineIfLambda>]f) = fun (b: ElementCollection) -> do (f()) b
    member inline _.Zero () = fun (b: ElementCollection) -> ()
        
    member inline _.For (xs, [<InlineIfLambda>]f: _ -> _) = fun (b: ElementCollection) ->
        xs |> Seq.iter (fun e -> f e b)

[<Sealed>]
type ViewBuilder (
    key, x, y, z,
    width, height,
    mouseMove, mouseEnter, mouseLeave, mouseUp, blur
    ) =
    inherit ElementBuilder()
    member 
        this.Run fn =
        let items = this.Borrow()
        do fn items
        { ElementView.Key = key
          Elements = items
          X = x
          Y = y
          Z = z
          MouseMove = mouseMove
          MouseEnter = mouseEnter
          MouseLeave = mouseLeave
          MouseUp = mouseUp
          Width = width
          Height = height
          Blur = blur
          }

[<Sealed>]
type ContextBuilder (name, func) =
    inherit ElementBuilder()
    member 
        this.Run fn =
        let items = this.Borrow()
        do fn items
        { ElementContext.Key = name
          ViewItems = items
          Func = func }


[<AbstractClass; Sealed; AutoOpen>]
type SpriteFunctions private () =
    static member sprite(
        x,
        y,
        z,
        width,
        height,
        texture,
        textureX,
        textureY,
        textureHeight,
        textureWidth
        ) =
        let sprite = ElementSprite.Pool.Borrow()

        sprite.X <- x
        sprite.Y <- y
        sprite.Z <- z
        sprite.Width <- width
        sprite.Height <- height
        sprite.Texture <- texture
        sprite.TextureX <- textureX
        sprite.TextureY <- textureY
        sprite.TextureWidth <- textureWidth
        sprite.TextureHeight <- textureHeight
        sprite.MetaData <- ValueNone
        sprite

let equal a b =
    a = b

[<AbstractClass; Sealed; AutoOpen>]
type ComponentFunctions private () =
    // if ever needed
    // [<CallerFilePath; Optional; DefaultParameterValue("")>]key : string,
    // [<CallerLineNumber; Optional; DefaultParameterValue(0)>]line : int,

    static member inline component(
        prop : 'a,
        ?key,
        ?compare : 'a -> 'a -> bool
    ) = fun fn ->
        let compare = compare |> Option.defaultValue equal

        { Key = key |> Option.defaultValue ""
          Prop = prop
          Compare =
            (fun current next ->
                try
                    let current = (unbox current)
                    let next = (unbox next)
                    compare current next
                with e ->
                    raise e
                    )
          Build =
            (fun h p -> 
                let a : 'a = unbox p
                fn h a)
        }

    static member inline component(
        ?key
    ) = fun fn -> component(prop = (), ?key = key) (fun hooks _ -> fn hooks)

    static member inline view(
        ?key, ?x, ?y, ?z, ?width, ?height, ?mouseMove, ?mouseEnter, ?mouseLeave, ?mouseUp, ?blur
    ) = ViewBuilder (
            key = (key |> Option.defaultValue ""),
            x = (x |> Option.defaultValue 0),
            y = (y |> Option.defaultValue 0),
            z = (z |> Option.defaultValue 0),
            width = (width |> Option.defaultValue 0),
            height = (height |> Option.defaultValue 0),
            mouseMove = (match mouseMove with Some x -> ValueSome x | _ -> ValueNone),
            mouseEnter = (match mouseEnter with Some x -> ValueSome x | _ -> ValueNone),
            mouseLeave  = (match mouseLeave with Some x -> ValueSome x | _ -> ValueNone),
            mouseUp = (match mouseUp with Some x -> ValueSome x | _ -> ValueNone),
            blur = (match blur with Some x -> ValueSome x | _ -> ValueNone)
        )

    static member inline context(func, ?key) =
        ContextBuilder(key |> Option.defaultValue "", func)