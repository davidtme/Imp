[<AutoOpen>]
module Imp.SingleRenderer

open Silk.NET.OpenGL
open Imp
open Imp.GL
open Imp.Shaders

type private SingleRenderer(gl, view) =
    inherit RendererBase(gl, view)

    override _.OnRender (props : RenderProperties) = 
        let shader = 
            Shaders.Single.reuse gl <| fun shader ->
                ()        

        shader.Use()

        gl.Viewport(0, 0, uint props.Resolution.Width, uint props.Resolution.Height)
        gl.Enable(EnableCap.DepthTest)
        gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        gl.BlendEquationSeparate(BlendEquationModeEXT.FuncAdd, BlendEquationModeEXT.FuncAdd)
        gl.Enable(EnableCap.Blend);
        gl.DepthFunc(DepthFunction.Lequal)

        shader.SetVertex(props.VertexBuffer)

        //shader.SetOffset(props.Offset.X |> float32, props.Offset.Y |> float32) 
        shader.SetOutputResolution(float32 props.Resolution.Width, float32 props.Resolution.Height) 
        shader.SetTextureResolution(float32 props.TextureDetails.Width, float32 props.TextureDetails.Height)
        shader.SetDiffuseTexture(props.TextureDetails.Texture)

        view.Sprites(fun struct(ox, oy, oz) sprite ->
            match props.TextureLayer sprite.Texture with
            | ValueSome layer ->
                shader.SetTextureLayer(float32 layer)

                shader.SetPosition(
                    float32 (props.Offset.X + sprite.X + ox),
                    float32 (props.Offset.Y + sprite.Y + oy),
                    float32 sprite.Width,
                    float32 sprite.Height)

                shader.SetTexturePosition(
                    float32 sprite.TextureX,
                    float32 sprite.TextureY,
                    float32 sprite.TextureWidth,
                    float32 sprite.TextureHeight)

                let minDepth, maxDepth = props.ZRange
                let d = float32 ((props.Offset.Z + sprite.Z + oz) - minDepth) / (float32 (maxDepth - minDepth))

                shader.SetDepth(1.0f - d)
            
                gl.DrawArraysInstanced(PrimitiveType.Triangles, 0, 6u, uint 1)

            | _ ->
                ()

        )

type SingleRenderBuilder(name) =
    inherit ElementBuilder()
    member 
        this.Run f =
        let items = this.Borrow()
        do f items
        { ElementRenderer.Key = name
          Create = fun g f -> SingleRenderer(g, f)
          ViewItems = items }

[<AbstractClass; Sealed; AutoOpen>]
type RendererFunctions private () =
    static member singleRenderer(
        ?key) =
        SingleRenderBuilder(key |> Option.defaultValue "")

