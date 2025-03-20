[<AutoOpen>]
module Imp.GL.CommonGLHelpers

#if !FABLE_COMPILER
open Silk.NET.OpenGL
#else
open Fable.Core
#endif

open Imp
open Imp.GL

let checkShader shader (gl: GL) =
#if !FABLE_COMPILER
    let code = gl.GetShader(shader, GLEnum.CompileStatus)
    if code <> int GLEnum.True then
      let info = gl.GetShaderInfoLog(shader)
      failwith (sprintf "Error compiling shader %s" info)
#else
    ()
    let compiled = gl.getShaderParameter(shader, gl.COMPILE_STATUS)
    Browser.Dom.console.log("Shader compiled successfully", compiled); 

    let compilationLog = gl.getShaderInfoLog(shader);
    Browser.Dom.console.log("Shader compiler log: ", compilationLog);
#endif

let checkProgram program (gl: GL)  =
#if !FABLE_COMPILER
    let code = gl.GetProgram(program, GLEnum.LinkStatus)
    if code <> int GLEnum.True then
      let info = gl.GetProgramInfoLog(program)
      failwith (sprintf "Error linking program %s" info)
#else
    let log = gl.getProgramInfoLog(program)
    Browser.Dom.console.log("getProgramInfoLog: ", log);

    ()
#endif

let compileShader (shader) (gl: GL) =
    gl.CompileShader(shader)
    gl |> checkShader shader

let createShader vertexShaderSource fragmentShaderSource (gl: GL) =
    let vertexShader = gl.CreateShader(ShaderType.VertexShader)
    gl.ShaderSource(vertexShader, vertexShaderSource)
    gl |> compileShader vertexShader
    
    let fragmentShader = gl.CreateShader(ShaderType.FragmentShader)
    gl.ShaderSource(fragmentShader, fragmentShaderSource)
    gl |> compileShader fragmentShader

    let program = gl.CreateProgram()
    gl.AttachShader(program, vertexShader)
    gl.AttachShader(program, fragmentShader)
    gl.LinkProgram(program)

    gl |> checkProgram program

    gl.DetachShader(program, vertexShader)
    gl.DetachShader(program, fragmentShader)
    gl.DeleteShader(vertexShader)
    gl.DeleteShader(fragmentShader)

    program

let createTexture (info : TextureInfo) (gl : GL) =
    let texture = gl.GenTexture();
    gl.ActiveTexture(TextureUnit.Texture0);
    gl.BindTexture(TextureTarget.Texture2D, texture);

    gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba, PixelFormat.Rgba, PixelType.UnsignedByte, info)

    gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, int TextureWrapMode.Repeat)
    gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, int TextureWrapMode.Repeat)
    gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, int TextureMinFilter.Nearest)
    gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, int TextureMagFilter.Nearest)
    //gl.GenerateMipmap(TextureTarget.Texture2D) // Needed if not power of 2

    gl.BindTexture(TextureTarget.Texture2D, 0u)

    texture

#nowarn "9"

//let createDepthTexture (width : uint32, height : uint32) (gl : GL) =
//    let texture = gl.GenTexture();
//    gl.ActiveTexture(TextureUnit.Texture0)
//    gl.BindTexture(TextureTarget.Texture2D, texture)

//    gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, int TextureWrapMode.ClampToEdge)
//    gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, int TextureWrapMode.ClampToEdge)
//    gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, int TextureMinFilter.Nearest)
//    gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, int TextureMagFilter.Nearest)

//    let voidPtr = 0n.ToPointer()

//    gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.DepthComponent24, width, height, 0, PixelFormat.DepthComponent, PixelType.UnsignedByte, voidPtr)
//    texture

let createTargetTexture (width : uint32, height : uint32) (internalFormat : InternalFormat) (pixelFormat : PixelFormat) (pixelType : PixelType) (gl : GL) =
    let texture = gl.GenTexture();
    gl.ActiveTexture(TextureUnit.Texture0)
    gl.BindTexture(TextureTarget.Texture2D, texture)

    gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, int TextureWrapMode.ClampToEdge)
    gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, int TextureWrapMode.ClampToEdge)
    gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, int TextureMinFilter.Nearest)
    gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, int TextureMagFilter.Nearest)
    
    gl.TexImage2D(TextureTarget.Texture2D, 0, internalFormat, width, height, 0, pixelFormat, pixelType, emptyPointer)
    texture

//#if !FABLE_COMPILER
//let loadTexture (manager : System.Resources.ResourceManager) name = 
//    async {
//        let path = System.IO.Path.Combine(System.Environment.CurrentDirectory, "Data", "Sprites", name + ".png")
//        let raw : byte[] = 
//            if System.IO.File.Exists(path) then
//                System.IO.File.ReadAllBytes(path)
//            else
//                manager.GetObject(name + ".png") |> unbox


//        let result = StbImageSharp.ImageResult.FromMemory(raw, StbImageSharp.ColorComponents.RedGreenBlueAlpha)
//        return { Pixels = result.Data; Width = uint result.Width; Height = uint result.Height }
//    }
//#else
//let loadTexture name : Async<TextureInfo> =
//    Fable.Core.JS.Constructors.Promise.Create (fun resolve _ ->
//        let image = Browser.Dom.document.createElement("img") :?> Browser.Types.HTMLImageElement
//        image.onload <- fun _ ->
//            resolve { Pixels = image; Width = uint image.width; Height = uint image.height }

//        image.src <- $"sprites/{name}.png"
//    ) |> Async.AwaitPromise
//#endif

let createLayeredTexture data (gl : GL) =
    let rec loop items i = 
        match items with
        | (_, info : TextureInfo) :: rest ->
            gl.TexSubImage3D(TextureTarget.Texture2DArray, 0, 0, 0, i, 1u, PixelFormat.Rgba, PixelType.UnsignedByte, info)
            loop rest (i + 1)
        | _ ->
            ()

    let texture = gl.GenTexture();
    gl.ActiveTexture(TextureUnit.Texture0);
    gl.BindTexture(TextureTarget.Texture2DArray, texture);

    let count = data |> List.length |> uint

    match data with
    | (_, info : TextureInfo) :: rest ->
        gl.TexStorage3D(TextureTarget.Texture2DArray, 1u, SizedInternalFormat.Rgba8, info.Width, info.Height, count)
        gl.TexSubImage3D(TextureTarget.Texture2DArray, 0, 0, 0, 0, 1u, PixelFormat.Rgba, PixelType.UnsignedByte, info)

        loop rest 1

        gl.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapS, int TextureWrapMode.Repeat)
        gl.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapT, int TextureWrapMode.Repeat)
        gl.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMinFilter, int TextureMinFilter.Nearest)
        gl.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMagFilter, int TextureMagFilter.Nearest)

        gl.BindTexture(TextureTarget.Texture2DArray, 0u)

        texture, (info.Width, info.Height), data |> List.mapi(fun i (name : string, _) -> name, uint i) |> readOnlyDict
    | _ -> 
        invalidOp ""



let createTextures (textures : (string * TextureInfo) list) (gl : GL) =
    let texture = gl.GenTexture();
    gl.ActiveTexture(TextureUnit.Texture0);
    gl.BindTexture(TextureTarget.Texture2DArray, texture);

    gl.TexStorage3D(TextureTarget.Texture2DArray, 1u, SizedInternalFormat.Rgba8, 512u, 512u, uint textures.Length)

    let layers =
        textures
        |> List.mapi(fun i (name, info) ->
            gl.TexSubImage3D(TextureTarget.Texture2DArray, 0, 0, 0, i, 1u, PixelFormat.Rgba, PixelType.UnsignedByte, info)
            name, uint i
        )
        |> Map.ofList   

    gl.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapS, int TextureWrapMode.Repeat)
    gl.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapT, int TextureWrapMode.Repeat)
    gl.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMinFilter, int TextureMinFilter.Nearest)
    gl.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMagFilter, int TextureMagFilter.Nearest)
    //gl.GenerateMipmap(TextureTarget.Texture2DArray) // Needed if not power of 2

    gl.BindTexture(TextureTarget.Texture2DArray, 0u)

    texture, layers

let bufferPool =
    Pool<_,_>(
        (fun (gl : GL) -> gl.GenBuffer()),
        (fun arr -> ())
    )

let createBuffer (items : #seq<float32>) (gl : GL) =
    let buffer = bufferPool.Borrow(gl)
    let arr = items |> Array.ofSeq
    gl.BindBuffer(BufferTargetARB.ArrayBuffer, buffer)
    gl.BufferData(BufferTargetARB.ArrayBuffer, arr, BufferUsageARB.StaticDraw)
    gl.UnbindBuffer(BufferTargetARB.ArrayBuffer)
    buffer

let createFramebuffer (target : FramebufferTarget) (bufs: DrawBufferMode[]) (gl : GL) =
    let frameBuffer = gl.GenFramebuffer()
    gl.BindFramebuffer(target, frameBuffer)
    gl.DrawBuffers(bufs)
    frameBuffer

let rgbaFrameBufferTexture = InternalFormat.Rgba, PixelFormat.Rgba, PixelType.UnsignedByte
let depthFrameBufferTexture = InternalFormat.DepthComponent24, PixelFormat.DepthComponent, PixelType.UnsignedInt

let createFrameBufferTexture size (internalFormat: InternalFormat, pixelFormat : PixelFormat, pixelType : PixelType) (framebufferAttachment : FramebufferAttachment) (gl : GL) = 
    let texture = gl |> createTargetTexture size internalFormat pixelFormat pixelType
    gl.FramebufferTexture2D(FramebufferTarget.Framebuffer, framebufferAttachment, TextureTarget.Texture2D, texture, 0)
    texture

#if !FABLE_COMPILER

let getTextures (manager : System.Resources.ResourceManager) names fn =
    //manager
    //    .GetResourceSet(System.Globalization.CultureInfo.CurrentCulture, true, true)
    //    |> Seq.cast<DictionaryEntry>
    //    |> Seq.choose(fun x ->
    //        match x.Value with
    //        | :? (byte[]) as raw -> 
    //            let name = System.IO.Path.GetFileNameWithoutExtension(x.Key.ToString())
    //            let result = StbImageSharp.ImageResult.FromMemory(raw, StbImageSharp.ColorComponents.RedGreenBlueAlpha)
    //            Some (name, { Pixels = result.Data; Width = uint result.Width; Height = uint result.Height })
    //        | x ->
    //            None
    //    )
    //    |> Map.ofSeq
    //    |> fn

    names
    |> Seq.map(fun name -> 
        let raw : byte[] = manager.GetObject(name + ".png") |> unbox
        let result = StbImageSharp.ImageResult.FromMemory(raw, StbImageSharp.ColorComponents.RedGreenBlueAlpha)
        name, { Pixels = result.Data; Width = uint result.Width; Height = uint result.Height }
    )
    |> List.ofSeq
    //|> Map.ofSeq
    |> fn


#else

let getTextures names fn =
    let results = ResizeArray()

    names
    |> List.iteri(fun index name ->
        let image = Browser.Dom.document.createElement("img") :?> Browser.Types.HTMLImageElement
        image.onload <- fun _ ->
            results.Add(name, { Pixels = image; Width = uint image.width; Height = uint image.height })
            if results.Count = List.length names then  
                results |> List.ofSeq |> fn

        image.src <- $"sprites/{name}.png"
    )

#endif

let vertices = 
    let left   =  0.0f
    let right  =  1.0f
    let bottom =  0.0f 
    let top    =  1.0f 
    [| 
       left;  top;      left;  bottom
       right; top;      right; bottom
       right; bottom;   right; top
       
       left;  top;      left;  bottom
       right; bottom;   right; top
       left;  bottom;   left;  top 
    |]