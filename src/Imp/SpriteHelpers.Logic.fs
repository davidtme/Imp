[<AutoOpen>]
module Imp.SpriteHelpers.Logic

open Imp

let inline renderSpriteLayerSized (layer : Layer<_>) (x,y) (width, height) =
    sprite(
        x = (x + layer.Offset.X),
        y = (y + layer.Offset.Y),
        z = 0,
        width = width,
        height = height,
        texture = layer.Texture,
        textureX = layer.Rect.X,
        textureY = layer.Rect.Y,
        textureWidth = layer.Rect.Width,
        textureHeight = layer.Rect.Height
    )

let inline renderSpriteLayer (layer : Layer<_>) (x,y) =
    renderSpriteLayerSized layer (x,y) (layer.Rect.Width, layer.Rect.Height)


let inline renderSpriteFrame (frame : Frame<_>) (x,y) =
    let items = elementCollectionPool.Borrow()
    let length = frame.Layers.Length - 1
    for i = 0 to length do
        let layer = frame.Layers[i]

        items.Add( 
            renderSpriteLayer layer (x,y) |> Element.Sprite
        )

    items

let inline spriteFrame frame (info : Models.SpriteInfo<_>) = 
    info.Frames.[frame - 1] 


type Setup.DataManager with
    member inline this.PopulateSprites name (sprites : System.Collections.Generic.Dictionary<_,SpriteInfo<'a>>) callback =
        this.LoadString name (fun (spiteJson : string) ->
            let (spriteRoot : _ list) = Json.deserialize spiteJson
        
            for s in spriteRoot do
                sprites.[s.Name] <- s

                for f in s.Frames do
                    for l in f.Layers do
                        this.TextureNames.Add(l.Texture) |> ignore

            callback ()
        )

type UI<'a>() = 
    let mutable defaultFont = "Font Small"
    let sprites = System.Collections.Generic.Dictionary<string,SpriteInfo<'a>>()

    member _.Sprites = sprites
    member _.SpriteDetails name = 
        match sprites.TryGetValue(name) with
        | true, s -> s
        | _ ->
            invalidOp $"Can't find name %s{name}"
    
    member _.DefaultFont with get () = defaultFont and set value = defaultFont <- value

type TextInputState =
    { Value : string
      Position : int
      Active : bool
      Shift : bool }

type UI<'a> with
    member ui.RenderSprite name frame xy =
        renderSpriteFrame (ui.SpriteDetails name |> spriteFrame frame) xy

    member ui.FirstSpriteLayer name =
        let info = ui.SpriteDetails name
        info.Frames.[0].Layers.[0]

    member ui.NineSlice  y x width height name =
        let firstSprite slice = 
            let info = ui.SpriteDetails (name + "-" + slice)
            info.Frames.[0].Layers.[0]

        view (x = x, y = y) {
            let topLeftLayer = firstSprite "TopLeft"
            let topLayer = firstSprite "Top"
            let topRightLayer = firstSprite "TopRight"

            let leftLayer = firstSprite "Left"
            let middleLayer = firstSprite "Middle"
            let rightLayer = firstSprite "Right"

            let bottomLeftLayer = firstSprite "BottomLeft"
            let bottomLayer = firstSprite "Bottom"
            let bottomRightLayer = firstSprite "BottomRight"

            // Top
            let sectionWidth = width - topLeftLayer.Rect.Width - topRightLayer.Rect.Width
            let sectionHeight = topLayer.Rect.Height
            let yOffset = 0

            renderSpriteLayer topLeftLayer (0,yOffset)
            renderSpriteLayerSized topLayer (topLeftLayer.Rect.Width, yOffset) (sectionWidth, sectionHeight)
            renderSpriteLayer topRightLayer (width - topRightLayer.Rect.Width,yOffset)

            // Middle

            let sectionWidth = width - leftLayer.Rect.Width - rightLayer.Rect.Width
            let sectionHeight = height - topLayer.Rect.Height - bottomLayer.Rect.Height
            let yOffset = topLeftLayer.Rect.Height

            renderSpriteLayerSized leftLayer (0, yOffset) (leftLayer.Rect.Width, sectionHeight)
            renderSpriteLayerSized middleLayer (topLeftLayer.Rect.Width, yOffset) (sectionWidth, sectionHeight)
            renderSpriteLayerSized rightLayer (width - topRightLayer.Rect.Width, yOffset) (rightLayer.Rect.Width, sectionHeight)

            // Bottom

            let sectionWidth = width - bottomLeftLayer.Rect.Width - bottomRightLayer.Rect.Width
            let sectionHeight = bottomLayer.Rect.Height
            let yOffset = height - bottomLayer.Rect.Height

            renderSpriteLayer bottomLeftLayer (0,yOffset)
            renderSpriteLayerSized bottomLayer (bottomLeftLayer.Rect.Width, yOffset) (sectionWidth, sectionHeight)
            renderSpriteLayer bottomRightLayer (width - topRightLayer.Rect.Width,yOffset)
        }

    member ui.Window (x, y, width, height) = fun (call : unit -> ElementView) ->
        view  (x = x, y = y) {
            ui.NineSlice 0 0 width height "Window"

            let titleSprite = ui.FirstSpriteLayer "Window Title"

            renderSpriteLayerSized titleSprite (3,3) (width - 6, titleSprite.Rect.Height)

            view (x = 4, y = 24) {
                call()
            }
        }

    member ui.Text(?key, ?fontName, ?x, ?y) = fun (value : string) ->

        let fontName = fontName |> Option.defaultValue ui.DefaultFont
        let x = x |> Option.defaultValue 0
        let y = y |> Option.defaultValue 0

        let mutable offset = 0
        view (?key = key) {
            for c in value do
                if c = ' ' then
                    offset <- offset + 4
                else
                    let c =
                        match c with
                        | ':' -> ValueNone
                        | '-' -> ValueSome "Minus"
                        | x -> x.ToString().ToUpperInvariant() |> ValueSome

                    match c with 
                    | ValueSome c ->
                        let frame =  ui.SpriteDetails $"%s{fontName}-{c}" |> spriteFrame 1
                        renderSpriteFrame frame (x + offset, y)
                        offset <- offset + frame.Layers.[0].Rect.Width + 1
                    | _ -> ()

        }

    member ui.Button(label, width, height, ?mouseUp, ?key, ?x, ?y) =
        view (?x=x, ?y=y, ?key = key, ?mouseUp = mouseUp, width = width, height = height) {
            ui.NineSlice 0 0 width height "Button"
            ui.Text (x = 10, y = 10) label
        }
    
    member ui.TextInput(width, height, ?key, ?x, ?y) = fun (value : string) ->
        view (?key = key, ?x = x, ?y = y) {
            component () <| fun hooks ->
                let state =
                    hooks.UseStateLazy <| fun _ -> 
                        { TextInputState.Value = value
                          Position = value.Length
                          Active = false
                          Shift = false }

                let s = state.Current

                if s.Active then
                    hooks.HandleInput (fun m -> 
                        match m with
                        | InputHandlerMessage.Key (pressed, key) ->
                            match key with
                            | Key.Left ->
                                if pressed then
                                    state.Update(fun s -> { s with Position = max 0 (s.Position - 1) })

                            | Key.Right ->
                                if pressed then
                                    state.Update(fun s -> { s with Position = min (s.Value.Length) (s.Position + 1) })

                            | Key.Backspace ->
                                if pressed then
                                    if s.Position > 0 then
                                        let a = s.Value.Substring(0, s.Position - 1)
                                        let c = s.Value.Substring(s.Position)
                                        state.Update(fun s -> { s with Value = a+c; Position = s.Position - 1 })

                            | Key.Delete ->
                                if pressed then
                                    if s.Position < s.Value.Length then
                                        let a = s.Value.Substring(0, s.Position)
                                        let c = s.Value.Substring(s.Position + 1)
                                        state.Update(fun s -> { s with Value = a+c })

                            | Key.ShiftLeft
                            | Key.ShiftRight -> 
                                state.Update(fun s -> { s with Shift = pressed })
      
                            | _ -> 
                                if pressed then
                                    let letter (normal) (shifted) =
                                        let x = if s.Shift then shifted else normal

                                        let p = s.Position
                                        let v = 
                                            let a = s.Value.Substring(0, p)
                                            let c = s.Value.Substring(p)
                                            a + x + c

                                        state.Update(fun s -> { s with Value = v; Position = p + 1 })

                                    match key with
                                    | Key.Space -> letter " " " "

                                    | Key.Number1 -> letter "1" "!"
                                    | Key.Number2 -> letter "2" "\""
                                    | Key.Number3 -> letter "3" "£"
                                    | Key.Number4 -> letter "4" "$"
                                    | Key.Number5 -> letter "5" "%"
                                    | Key.Number6 -> letter "6" "^"
                                    | Key.Number7 -> letter "7" "&"
                                    | Key.Number8 -> letter "8" "*"
                                    | Key.Number9 -> letter "9" "("
                                    | Key.Number0 -> letter "0" ")"

                                    | Key.A -> letter "a" "A"
                                    | Key.B -> letter "b" "B"
                                    | Key.C -> letter "c" "C"
                                    | Key.D -> letter "d" "D"
                                    | Key.E -> letter "e" "E"
                                    | Key.F -> letter "f" "F"
                                    | Key.G -> letter "g" "G"
                                    | Key.H -> letter "h" "H"
                                    | Key.I -> letter "i" "I"
                                    | Key.J -> letter "j" "J"
                                    | Key.K -> letter "k" "K"
                                    | Key.L -> letter "l" "L"
                                    | Key.M -> letter "m" "M"
                                    | Key.N -> letter "n" "N"
                                    | Key.O -> letter "o" "O"
                                    | Key.P -> letter "p" "P"
                                    | Key.Q -> letter "q" "Q"
                                    | Key.R -> letter "r" "R"
                                    | Key.S -> letter "s" "S"
                                    | Key.T -> letter "t" "T"
                                    | Key.U -> letter "u" "U"
                                    | Key.V -> letter "v" "V"
                                    | Key.W -> letter "w" "W"
                                    | Key.X -> letter "x" "X"
                                    | Key.Y -> letter "y" "Y"
                                    | Key.Z -> letter "z" "Z"
                            

                                    | Key.Slash -> letter "/" "?"
                                    | Key.Period -> letter "." ">"
                                    | Key.Comma -> letter "," "<"
                                    | Key.Apostrophe -> letter "'" "@"
                                    | Key.Semicolon -> letter ";" ":"

                                    | _ -> ()


                        | _ ->
                            ()
                    )

                let fontName = ui.DefaultFont
                let mutable offset = 0

                view (
                    width = width,
                    height = height,
                    mouseUp = (fun _ -> 
                        state.Update(fun s -> { s with Active = true })
                    ),
                    blur = (fun _ -> 
                        state.Update(fun s -> { s with Active = false })
                    )
                    ) {
                        for i = 0 to s.Value.Length - 1 do
                            let c = s.Value.Chars i
                            if c = ' ' then
                                offset <- offset + 4
                            else
                                let c =
                                    match c with
                                    | ':' -> ValueNone
                                    | '-' -> ValueSome "Minus"
                                    | '@' -> ValueSome "At"
                                    | '?' -> ValueSome "Question Mark"
                                    | '\"' -> ValueSome "Double Quote"
                                    | '.' -> ValueSome "Full Stop"
                                    | ',' -> ValueSome "Comma"
                                    | x -> x.ToString().ToUpperInvariant() |> ValueSome

                                match c with 
                                | ValueSome c ->
                                    match ui.Sprites.TryGetValue $"%s{fontName}-{c}" with
                                    | true, s ->
                                        let frame =  s  |> spriteFrame 1
                                        renderSpriteFrame frame (offset, 0)
                                        offset <- offset + frame.Layers.[0].Rect.Width + 1

                                    | _ -> 
                                        ()

                                | _ -> ()

                            if s.Active && i = s.Position - 1 then
                                let frame =  ui.SpriteDetails $"%s{fontName}-Caret" |> spriteFrame 1
                                renderSpriteFrame frame (offset - 2, 0)
                        
                }
            }