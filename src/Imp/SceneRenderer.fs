[<AutoOpen>]
module Imp.SceneRenderer

open Silk.NET.OpenGL
open Imp
open Imp.GL

type RGB = 
    { R : int
      G : int
      B : int }

type private Renderer(backgroundColor : RGB, gl, view) =
    inherit RendererBase(gl, view)

    override _.OnRender (props : RenderProperties) = 
        gl.Disable(EnableCap.Dither)
        gl.Enable(EnableCap.CullFace)
        gl.CullFace(TriangleFace.Back)
        gl.FrontFace(FrontFaceDirection.CW)

        gl.Viewport(0,0, props.Resolution.Width |> uint, props.Resolution.Height |> uint) 
        gl.ClearColor(
            (float32 backgroundColor.R) /255.0f,
            (float32 backgroundColor.G) /255.0f,
            (float32 backgroundColor.B) /255.0f,
            1.0f)

        gl.ClearDepth(1.0f)
        gl.Clear(ClearBufferMask.ColorBufferBit ||| ClearBufferMask.DepthBufferBit)

        view.Renderers(props)

type RenderBuilder(backgroundColor, key) =
    inherit ElementBuilder()
    member  this.Run fn =
        let items = this.Borrow()
        do fn items
        { ElementRenderer.Key = key
          Create = fun gl view -> 
            Renderer(
                backgroundColor = backgroundColor,
                gl = gl,
                view = view)
          ViewItems = items }

[<AbstractClass; Sealed; AutoOpen>]
type RendererFunctions private () =
    static member sceneRenderer(
        backgroundColor,
        ?key
        ) =
        RenderBuilder(
            backgroundColor = backgroundColor,
            key = (key |> Option.defaultValue "")
        )

