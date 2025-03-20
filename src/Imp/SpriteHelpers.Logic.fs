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
    member _.SpriteDetails name = sprites.[name]
    member _.DefaultFont with get () = defaultFont and set value = defaultFont <- value


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

    member ui.Button(label, ?mouseUp, ?key, ?x, ?y, ?width, ?height) =
        let width = width |> Option.defaultValue 32
        let height = height |> Option.defaultValue 32

        view (?x=x, ?y=y, ?key = key, ?mouseUp = mouseUp, width = width, height = height) {
            ui.NineSlice 0 0 width height "Button"
            ui.Text (x = 10, y = 10) label
        }
    