[<AutoOpen>]
module Imp.GL.Models

#if !FABLE_COMPILER

type GLBuffer = uint32
type GLShader = uint32
type GLProgram = uint32
type GLVertexArray = uint32
type GLTexture = uint32
type GLTexturePixels = byte[]
type GLFramebuffer = uint
type GL = Silk.NET.OpenGL.GL

#else

open Browser.Types

type GLBuffer = WebGLBuffer
type GLShader = WebGLShader
type GLProgram = WebGLProgram
type GLTexture = WebGLTexture
type GLUniformLocation = WebGLUniformLocation
type GLAttribLocation = float
type GLVertexArray = WebGLVertexArrayObject
type GLTexturePixels = HTMLImageElement
type GL = WebGLRenderingContext
type GLFramebuffer = WebGLFramebuffer

#endif

[<Struct>]
type TextureInfo =
    { Pixels : GLTexturePixels
      Width : uint 
      Height : uint }

