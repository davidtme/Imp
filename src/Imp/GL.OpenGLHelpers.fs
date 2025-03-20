[<AutoOpen>]
module Imp.GL.OpenGLHelpers

#if !FABLE_COMPILER

#nowarn "9"

open System
open Silk.NET.OpenGL
open Microsoft.FSharp.NativeInterop
open Imp.GL

let Float32Size = sizeof<float32>
let pointerOffset (offset : int) size = IntPtr(offset * size).ToPointer()
let emptyPointer = 0n.ToPointer()

type Silk.NET.OpenGL.GL with
    member gl.BufferData(target : BufferTargetARB, vertices : float32[], usage : BufferUsageARB) =
        use floatPtr = fixed vertices
        let voidPtr = floatPtr |> NativePtr.toVoidPtr
        let size = (vertices.Length * sizeof<float32>) |> unativeint
        gl.BufferData(target, size, voidPtr, usage)

    member gl.TexImage2D(target : TextureTarget, level, internalformat : InternalFormat, format : PixelFormat, typ : PixelType, info : TextureInfo) =
        use ptr = fixed info.Pixels
        let voidPtr = ptr |> NativePtr.toVoidPtr
        gl.TexImage2D(target, level, internalformat, info.Width, info.Height, 0, format, typ, voidPtr)

    member gl.TexSubImage3D(target : TextureTarget, level, xoffset, yoffset, zoffset, depth, format : PixelFormat, typ : PixelType, info : TextureInfo) =
        use ptr = fixed info.Pixels
        let voidPtr = ptr |> NativePtr.toVoidPtr
        gl.TexSubImage3D(target, level, xoffset, yoffset, zoffset, info.Width, info.Height, depth, format, typ, voidPtr)

    member inline gl.UnbindBuffer(target: BufferTargetARB) =
        gl.BindBuffer(target, 0u)

    member inline gl.GetCurrentProgram() =
        gl.GetInteger(GLEnum.CurrentProgram) |> uint32

    member inline gl.GetCurrentFramebuffer() =
        gl.GetInteger(GLEnum.FramebufferBinding) |> uint32

#endif