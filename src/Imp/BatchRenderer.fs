[<AutoOpen>]
module Imp.BatchRenderere

open Silk.NET.OpenGL
open Imp
open Imp.GL
open Imp.Shaders

let private arrayPool =
    Pool<_>(
        (fun () -> ResizeArray<float32>()),
        (fun arr -> arr.Clear())
    )

type private BatchRenderer(gl, view) =
    inherit RendererBase(gl, view)

    let mutable renderResult = ValueNone

    override this.OnInit (props : RenderProperties) = 
        this.Clean ()

        let posArray = arrayPool.Borrow()
        let depthArray = arrayPool.Borrow()
        let texturePosArray = arrayPool.Borrow()
        let textureLayerArray = arrayPool.Borrow()

        let inline appendSpritePosition ox oy (sprite : ElementSprite) (arr : ResizeArray<_>) =
            arr.Add(float32 (sprite.X+ox))
            arr.Add(float32 (sprite.Y+oy))
            arr.Add(float32 sprite.Width)
            arr.Add(float32 sprite.Height)

        let inline appendSpriteTexturePosition (sprite : ElementSprite) (arr : ResizeArray<_>) =
            arr.Add(float32 sprite.TextureX)
            arr.Add(float32 sprite.TextureY)
            arr.Add(float32 sprite.TextureWidth)
            arr.Add(float32 sprite.TextureHeight)

        let inline appendSpriteTextureLayer (sprite : ElementSprite) (arr : ResizeArray<_>) =
            match props.TextureLayer sprite.Texture with
            | ValueSome layer ->
                arr.Add(float32 layer)
            | _ -> ()

        let inline appendSpriteDepth oz (sprite : ElementSprite) (arr : ResizeArray<_>) =
            arr.Add(float32(sprite.Z + oz))

        view.Sprites <| fun struct(ox,oy,oz) sprite ->
          if sprite.MetaData.IsNone then

            posArray |> appendSpritePosition ox oy sprite
            texturePosArray |> appendSpriteTexturePosition sprite
            textureLayerArray |> appendSpriteTextureLayer sprite
            depthArray |> appendSpriteDepth oz sprite

        renderResult <-
            ValueSome <|
            struct {|
                PosBuffer = gl |> createBuffer posArray
                TextureBuffer = gl |> createBuffer texturePosArray
                TextureLayer = gl |> createBuffer textureLayerArray
                DepthBuffer = gl |> createBuffer depthArray
                TotalSprites = posArray.Count / 4
            |}

        // World Items
        arrayPool.Return(posArray)
        arrayPool.Return(texturePosArray)
        arrayPool.Return(textureLayerArray)
        arrayPool.Return(depthArray)

    override _.OnRender (props : RenderProperties) = 
        let renderResult = renderResult.Value
                
        gl.Viewport(0, 0, uint props.Resolution.Width, uint props.Resolution.Height)
        gl.Enable(EnableCap.DepthTest)
        gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        gl.BlendEquationSeparate(BlendEquationModeEXT.FuncAdd, BlendEquationModeEXT.FuncAdd)
        gl.Enable(EnableCap.Blend);
        gl.DepthFunc(DepthFunction.Lequal)


        let shader = 
            Shaders.Batch.reuse gl <| fun shader ->
                shader.SetVertex(props.VertexBuffer)

        //let z = 
        //    let minDepth, maxDepth = props.ZRange
        //    let r = maxDepth |> float32
        //    float32 ((props.Offset.Z) - minDepth) / r

        shader.SetOffset(float32 props.Offset.X, float32 props.Offset.Y, float32 props.Offset.Z)
        let minDepth, maxDepth = props.ZRange
        shader.SetDepthRange(float32 minDepth , float32 maxDepth)
        
        shader.SetOutputResolution(float32 props.Resolution.Width, float32 props.Resolution.Height) 
        shader.SetTextureResolution(float32 props.TextureDetails.Width, float32 props.TextureDetails.Height)
        shader.SetDiffuseTexture(props.TextureDetails.Texture)

        shader.SetTextureLayer(renderResult.TextureLayer, 1u)
        shader.SetPosition(renderResult.PosBuffer, 1u)
        shader.SetTexturePosition(renderResult.TextureBuffer, 1u)
        shader.SetDepth(renderResult.DepthBuffer, 1u)

        gl.DrawArraysInstanced(PrimitiveType.Triangles, 0, 6u, uint renderResult.TotalSprites)

    override this.OnDistory() =
        this.Clean()

    member private _.Clean () =
        renderResult |> ValueOption.iter(fun renderResult -> 
            bufferPool.Return(renderResult.PosBuffer)
            bufferPool.Return(renderResult.TextureBuffer)
            bufferPool.Return(renderResult.TextureLayer)
            bufferPool.Return(renderResult.DepthBuffer)
        )
        renderResult <- ValueNone

type BatchRenderBuilder(name) =
    inherit ElementBuilder()
    member 
        this.Run f =
        let items = this.Borrow()
        do f items
        { ElementRenderer.Key = name
          Create = fun g f -> BatchRenderer(g, f)
          ViewItems = items }

[<AbstractClass; Sealed; AutoOpen>]
type RendererFunctions private () =
    static member batchRenderer(
        ?key) =
        BatchRenderBuilder(key |> Option.defaultValue "")