[<AutoOpenAttribute>]
module Imp.GL.WebGLHelpers

#if FABLE_COMPILER

open Browser.Types
open Fable.Core
open Fable.Core.JsInterop
open Imp.GL

type ClearBufferMask =
    | None = 0
    | DepthBufferBit = 256
    | StencilBufferBit = 1024
    | ColorBufferBit = 16384
    | CoverageBufferBitNV = 32768

type ShaderType =
    | FragmentShader = 35632
    | FragmentShaderArb = 35632
    | VertexShader = 35633
    | VertexShaderArb = 35633
    | GeometryShader = 36313
    | TessEvaluationShader = 36487
    | TessControlShader = 36488
    | ComputeShader = 37305

type BufferTargetARB =
    | ParameterBuffer = 33006
    | ArrayBuffer = 34962
    | ElementArrayBuffer = 34963
    | PixelPackBuffer = 35051
    | PixelUnpackBuffer = 35052
    | UniformBuffer = 35345
    | TextureBuffer = 35882
    | TransformFeedbackBuffer = 35982
    | CopyReadBuffer = 36662
    | CopyWriteBuffer = 36663
    | DrawIndirectBuffer = 36671
    | ShaderStorageBuffer = 37074
    | DispatchIndirectBuffer = 37102
    | QueryBuffer = 37266
    | AtomicCounterBuffer = 37568

type BufferUsageARB =
    | StreamDraw = 35040
    | StreamRead = 35041
    | StreamCopy = 35042
    | StaticDraw = 35044
    | StaticRead = 35045
    | StaticCopy = 35046
    | DynamicDraw = 35048
    | DynamicRead = 35049
    | DynamicCopy = 35050

type VertexAttribPointerType =
    | Byte = 5120
    | UnsignedByte = 5121
    | Short = 5122
    | UnsignedShort = 5123
    | Int = 5124
    | UnsignedInt = 5125
    | Float = 5126
    | Double = 5130
    | HalfFloat = 5131
    | Fixed = 5132
    | Int64Arb = 5134
    | Int64NV = 5134
    | UnsignedInt64Arb = 5135
    | UnsignedInt64NV = 5135
    | UnsignedInt2101010Rev = 33640
    | UnsignedInt2101010RevExt = 33640
    | UnsignedInt10f11f11fRev = 35899
    | Int2101010Rev = 36255

type PrimitiveType =
    | Points = 0
    | Lines = 1
    | LineLoop = 2
    | LineStrip = 3
    | Triangles = 4
    | TriangleStrip = 5
    | TriangleFan = 6
    | Quads = 7
    | QuadsExt = 7
    | LinesAdjacency = 10
    | LinesAdjacencyArb = 10
    | LinesAdjacencyExt = 10
    | LineStripAdjacency = 11
    | LineStripAdjacencyArb = 11
    | LineStripAdjacencyExt = 11
    | TrianglesAdjacency = 12
    | TrianglesAdjacencyArb = 12
    | TrianglesAdjacencyExt = 12
    | TriangleStripAdjacency = 13
    | TriangleStripAdjacencyArb = 13
    | TriangleStripAdjacencyExt = 13
    | Patches = 14
    | PatchesExt = 14

type TextureUnit =
    | Texture0 = 33984
    | Texture1 = 33985
    | Texture2 = 33986
    | Texture3 = 33987
    | Texture4 = 33988
    | Texture5 = 33989
    | Texture6 = 33990
    | Texture7 = 33991
    | Texture8 = 33992
    | Texture9 = 33993
    | Texture10 = 33994
    | Texture11 = 33995
    | Texture12 = 33996
    | Texture13 = 33997
    | Texture14 = 33998
    | Texture15 = 33999
    | Texture16 = 34000
    | Texture17 = 34001
    | Texture18 = 34002
    | Texture19 = 34003
    | Texture20 = 34004
    | Texture21 = 34005
    | Texture22 = 34006
    | Texture23 = 34007
    | Texture24 = 34008
    | Texture25 = 34009
    | Texture26 = 34010
    | Texture27 = 34011
    | Texture28 = 34012
    | Texture29 = 34013
    | Texture30 = 34014
    | Texture31 = 34015

type TextureTarget =
    | Texture1D = 3552
    | Texture2D = 3553
    | ProxyTexture1D = 32867
    | ProxyTexture1DExt = 32867
    | ProxyTexture2D = 32868
    | ProxyTexture2DExt = 32868
    | Texture3D = 32879
    | Texture3DExt = 32879
    | Texture3DOes = 32879
    | ProxyTexture3D = 32880
    | ProxyTexture3DExt = 32880
    | DetailTexture2DSgis = 32917
    | Texture4DSgis = 33076
    | ProxyTexture4DSgis = 33077
    | TextureRectangle = 34037
    | TextureRectangleArb = 34037
    | TextureRectangleNV = 34037
    | ProxyTextureRectangle = 34039
    | ProxyTextureRectangleArb = 34039
    | ProxyTextureRectangleNV = 34039
    | TextureCubeMap = 34067
    | TextureCubeMapArb = 34067
    | TextureCubeMapExt = 34067
    | TextureCubeMapOes = 34067
    | TextureCubeMapPositiveX = 34069
    | TextureCubeMapPositiveXArb = 34069
    | TextureCubeMapPositiveXExt = 34069
    | TextureCubeMapPositiveXOes = 34069
    | TextureCubeMapNegativeX = 34070
    | TextureCubeMapNegativeXArb = 34070
    | TextureCubeMapNegativeXExt = 34070
    | TextureCubeMapNegativeXOes = 34070
    | TextureCubeMapPositiveY = 34071
    | TextureCubeMapPositiveYArb = 34071
    | TextureCubeMapPositiveYExt = 34071
    | TextureCubeMapPositiveYOes = 34071
    | TextureCubeMapNegativeY = 34072
    | TextureCubeMapNegativeYArb = 34072
    | TextureCubeMapNegativeYExt = 34072
    | TextureCubeMapNegativeYOes = 34072
    | TextureCubeMapPositiveZ = 34073
    | TextureCubeMapPositiveZArb = 34073
    | TextureCubeMapPositiveZExt = 34073
    | TextureCubeMapPositiveZOes = 34073
    | TextureCubeMapNegativeZ = 34074
    | TextureCubeMapNegativeZArb = 34074
    | TextureCubeMapNegativeZExt = 34074
    | TextureCubeMapNegativeZOes = 34074
    | ProxyTextureCubeMap = 34075
    | ProxyTextureCubeMapArb = 34075
    | ProxyTextureCubeMapExt = 34075
    | Texture1DArray = 35864
    | ProxyTexture1DArray = 35865
    | ProxyTexture1DArrayExt = 35865
    | Texture2DArray = 35866
    | ProxyTexture2DArray = 35867
    | ProxyTexture2DArrayExt = 35867
    | TextureBuffer = 35882
    | Renderbuffer = 36161
    | TextureCubeMapArray = 36873
    | TextureCubeMapArrayArb = 36873
    | TextureCubeMapArrayExt = 36873
    | TextureCubeMapArrayOes = 36873
    | ProxyTextureCubeMapArray = 36875
    | ProxyTextureCubeMapArrayArb = 36875
    | Texture2DMultisample = 37120
    | ProxyTexture2DMultisample = 37121
    | Texture2DMultisampleArray = 37122
    | ProxyTexture2DMultisampleArray = 37123

type InternalFormat =
    | Rgba = 6408
    | DepthComponent24 = 33190


type PixelFormat =
      | UnsignedShort = 5123
      | UnsignedInt = 5125
      | StencilIndex = 6401
      | DepthComponent = 6402
      | Red = 6403
      | RedExt = 6403
      | Green = 6404
      | Blue = 6405
      | Alpha = 6406
      | Rgb = 6407
      | Rgba = 6408
      | AbgrExt = 32768
      | CmykExt = 32780
      | CmykaExt = 32781
      | Bgr = 32992
      | BgrExt = 32992
      | Bgra = 32993
      | BgraExt = 32993
      | BgraImg = 32993
      | Ycrcb422Sgix = 33211
      | Ycrcb444Sgix = 33212
      | RG = 33319
      | RGInteger = 33320
      | DepthStencil = 34041
      | RedInteger = 36244
      | GreenInteger = 36245
      | BlueInteger = 36246
      | RgbInteger = 36248
      | RgbaInteger = 36249
      | BgrInteger = 36250
      | BgraInteger = 36251

type PixelType =
    | UnsignedByte = 5121
    | UnsignedInt = 5125

type TextureParameterName =
    | TextureWidth = 4096
    | TextureHeight = 4097
    | TextureInternalFormat = 4099
    | TextureBorderColor = 4100
    | TextureBorderColorNV = 4100
    | TextureMagFilter = 10240
    | TextureMinFilter = 10241
    | TextureWrapS = 10242
    | TextureWrapT = 10243
    | TextureRedSize = 32860
    | TextureGreenSize = 32861
    | TextureBlueSize = 32862
    | TextureAlphaSize = 32863
    | TexturePriorityExt = 32870
    | TextureDepthExt = 32881
    | TextureWrapR = 32882
    | TextureWrapRExt = 32882
    | TextureWrapROes = 32882
    | DetailTextureLevelSgis = 32922
    | DetailTextureModeSgis = 32923
    | DetailTextureFuncPointsSgis = 32924
    | SharpenTextureFuncPointsSgis = 32944
    | ShadowAmbientSgix = 32959
    | DualTextureSelectSgis = 33060
    | QuadTextureSelectSgis = 33061
    | Texture4DsizeSgis = 33078
    | TextureWrapQSgis = 33079
    | TextureMinLod = 33082
    | TextureMinLodSgis = 33082
    | TextureMaxLod = 33083
    | TextureMaxLodSgis = 33083
    | TextureBaseLevel = 33084
    | TextureBaseLevelSgis = 33084
    | TextureMaxLevel = 33085
    | TextureMaxLevelSgis = 33085
    | TextureFilter4SizeSgis = 33095
    | TextureClipmapCenterSgix = 33137
    | TextureClipmapFrameSgix = 33138
    | TextureClipmapOffsetSgix = 33139
    | TextureClipmapVirtualDepthSgix = 33140
    | TextureClipmapLodOffsetSgix = 33141
    | TextureClipmapDepthSgix = 33142
    | PostTextureFilterBiasSgix = 33145
    | PostTextureFilterScaleSgix = 33146
    | TextureLodBiasSSgix = 33166
    | TextureLodBiasTSgix = 33167
    | TextureLodBiasRSgix = 33168
    | GenerateMipmapSgis = 33169
    | TextureCompareSgix = 33178
    | TextureCompareOperatorSgix = 33179
    | TextureLequalRSgix = 33180
    | TextureGequalRSgix = 33181
    | TextureMaxClampSSgix = 33641
    | TextureMaxClampTSgix = 33642
    | TextureMaxClampRSgix = 33643
    | TextureMemoryLayoutIntel = 33791
    | TextureMaxAnisotropy = 34046
    | TextureLodBias = 34049
    | TextureCompareMode = 34892
    | TextureCompareFunc = 34893
    | TextureSwizzleR = 36418
    | TextureSwizzleG = 36419
    | TextureSwizzleB = 36420
    | TextureSwizzleA = 36421
    | TextureSwizzleRgba = 36422
    | TextureUnnormalizedCoordinatesArm = 36714
    | DepthStencilTextureMode = 37098
    | TextureTilingExt = 38272
    | TextureFoveatedCutoffDensityQCom = 38560

type TextureMinFilter =
    | Nearest = 9728
    | Linear = 9729
    | NearestMipmapNearest = 9984
    | LinearMipmapNearest = 9985
    | NearestMipmapLinear = 9986
    | LinearMipmapLinear = 9987
    | Filter4Sgis = 33094
    | LinearClipmapLinearSgix = 33136
    | PixelTexGenQCeilingSgix = 33156
    | PixelTexGenQRoundSgix = 33157
    | PixelTexGenQFloorSgix = 33158
    | NearestClipmapNearestSgix = 33869
    | NearestClipmapLinearSgix = 33870
    | LinearClipmapNearestSgix = 33871

type TextureWrapMode =
    | LinearMipmapLinear = 9987
    | Repeat = 10497
    | ClampToBorder = 33069
    | ClampToBorderArb = 33069
    | ClampToBorderNV = 33069
    | ClampToBorderSgis = 33069
    | ClampToEdge = 33071
    | ClampToEdgeSgis = 33071
    | MirroredRepeat = 33648

type TextureMagFilter =
    | Nearest = 9728
    | Linear = 9729
    | LinearDetailSgis = 32919
    | LinearDetailAlphaSgis = 32920
    | LinearDetailColorSgis = 32921
    | LinearSharpenSgis = 32941
    | LinearSharpenAlphaSgis = 32942
    | LinearSharpenColorSgis = 32943
    | Filter4Sgis = 33094
    | PixelTexGenQCeilingSgix = 33156
    | PixelTexGenQRoundSgix = 33157
    | PixelTexGenQFloorSgix = 33158

type EnableCap =
    | CullFace = 2884
    | Blend = 3042
    | Dither = 3024
    //| MultisampleSgis = 32925
    | DepthTest = 2929

type TriangleFace =
    | Front = 1028
    | Back = 1029
    | FrontAndBack = 1032

type FrontFaceDirection =
    | CW = 2304
    | Ccw = 2305


type BlendingFactor =
    | Zero = 0
    | One = 1
    | SrcColor = 768
    | OneMinusSrcColor = 769
    | SrcAlpha = 770
    | OneMinusSrcAlpha = 771
    | DstAlpha = 772
    | OneMinusDstAlpha = 773
    | DstColor = 774
    | OneMinusDstColor = 775
    | SrcAlphaSaturate = 776
    | ConstantColor = 32769
    | OneMinusConstantColor = 32770
    | ConstantAlpha = 32771
    | OneMinusConstantAlpha = 32772
    | Src1Alpha = 34185
    | Src1Color = 35065
    | OneMinusSrc1Color = 35066
    | OneMinusSrc1Alpha = 35067

type DepthFunction =
    | Never = 512
    | Less = 513
    | Equal = 514
    | Lequal = 515
    | Greater = 516
    | Notequal = 517
    | Gequal = 518
    | Always = 519

type SizedInternalFormat =
    | Rgba8 = 32856

type FramebufferTarget =
    | ReadFramebuffer = 36008
    | DrawFramebuffer = 36009
    | Framebuffer = 36160
    | FramebufferOes = 36160

type GLEnum =
    | CurrentProgram = 35725
    | FramebufferBinding = 36006

type FramebufferAttachment =
    | DepthStencilAttachment = 33306
    | ColorAttachment0 = 36064
    | ColorAttachment1 = 36065
    | ColorAttachment2 = 36066
    | ColorAttachment3 = 36067
    | ColorAttachment4 = 36068
    | ColorAttachment5 = 36069
    | ColorAttachment6 = 36070
    | ColorAttachment7 = 36071
    | ColorAttachment8 = 36072
    | ColorAttachment9 = 36073
    | ColorAttachment10 = 36074
    | ColorAttachment11 = 36075
    | ColorAttachment12 = 36076
    | ColorAttachment13 = 36077
    | ColorAttachment14 = 36078
    | ColorAttachment15 = 36079
    | ColorAttachment16 = 36080
    | ColorAttachment17 = 36081
    | ColorAttachment18 = 36082
    | ColorAttachment19 = 36083
    | ColorAttachment20 = 36084
    | ColorAttachment21 = 36085
    | ColorAttachment22 = 36086
    | ColorAttachment23 = 36087
    | ColorAttachment24 = 36088
    | ColorAttachment25 = 36089
    | ColorAttachment26 = 36090
    | ColorAttachment27 = 36091
    | ColorAttachment28 = 36092
    | ColorAttachment29 = 36093
    | ColorAttachment30 = 36094
    | ColorAttachment31 = 36095
    | DepthAttachment = 36096
    | StencilAttachment = 36128
    | ShadingRateAttachmentExt = 38609

type DrawBufferMode =
    | None = 0
    | NoneOes = 0
    | FrontLeft = 1024
    | FrontRight = 1025
    | BackLeft = 1026
    | BackRight = 1027
    | Front = 1028
    | Back = 1029
    | Left = 1030
    | Right = 1031
    | FrontAndBack = 1032
    | ColorAttachment0 = 36064
    | ColorAttachment0NV = 36064
    | ColorAttachment1 = 36065
    | ColorAttachment1NV = 36065
    | ColorAttachment2 = 36066
    | ColorAttachment2NV = 36066
    | ColorAttachment3 = 36067
    | ColorAttachment3NV = 36067
    | ColorAttachment4 = 36068
    | ColorAttachment4NV = 36068
    | ColorAttachment5 = 36069
    | ColorAttachment5NV = 36069
    | ColorAttachment6 = 36070
    | ColorAttachment6NV = 36070
    | ColorAttachment7 = 36071
    | ColorAttachment7NV = 36071
    | ColorAttachment8 = 36072
    | ColorAttachment8NV = 36072
    | ColorAttachment9 = 36073
    | ColorAttachment9NV = 36073
    | ColorAttachment10 = 36074
    | ColorAttachment10NV = 36074
    | ColorAttachment11 = 36075
    | ColorAttachment11NV = 36075
    | ColorAttachment12 = 36076
    | ColorAttachment12NV = 36076
    | ColorAttachment13 = 36077
    | ColorAttachment13NV = 36077
    | ColorAttachment14 = 36078
    | ColorAttachment14NV = 36078
    | ColorAttachment15 = 36079
    | ColorAttachment15NV = 36079
    | ColorAttachment16 = 36080
    | ColorAttachment17 = 36081
    | ColorAttachment18 = 36082
    | ColorAttachment19 = 36083
    | ColorAttachment20 = 36084
    | ColorAttachment21 = 36085
    | ColorAttachment22 = 36086
    | ColorAttachment23 = 36087
    | ColorAttachment24 = 36088
    | ColorAttachment25 = 36089
    | ColorAttachment26 = 36090
    | ColorAttachment27 = 36091
    | ColorAttachment28 = 36092
    | ColorAttachment29 = 36093
    | ColorAttachment30 = 36094
    | ColorAttachment31 = 36095

type BlendEquationModeEXT =
  | FuncAdd = 32774
  | FuncAddExt = 32774
  | Min = 32775
  | MinExt = 32775
  | Max = 32776
  | MaxExt = 32776
  | FuncSubtract = 32778
  | FuncSubtractExt = 32778
  | FuncReverseSubtract = 32779
  | FuncReverseSubtractExt = 32779
  | AlphaMinSgix = 33568
  | AlphaMaxSgix = 33569

let [<Literal>] Float32Size = 4
let inline pointerOffset (offset : int) size = offset * size

let emptyPointer : byte[] = null



type WebGLRenderingContext with 
    member inline this.ClearColor(red : float32, green : float32, blue : float32, alpha : float32) = 
        this.clearColor(unbox red, unbox green, unbox blue, unbox alpha)

    member inline this.Clear(mask : ClearBufferMask) =
        this.clear(unbox mask)

    member inline this.BufferData(target : BufferTargetARB, vertices : float32[], usage : BufferUsageARB) =
        this.bufferData(unbox target, unbox vertices, unbox usage)

    member inline this.GenBuffer() : GLBuffer =
        this.createBuffer()

    member inline this.CompileShader(shader : GLShader) =
        this.compileShader(shader)

    member inline this.CreateShader(typ : ShaderType) =
        this.createShader(unbox typ)

    member inline this.ShaderSource(shader : GLShader, value) = 
        this.shaderSource(shader, value)

    member inline this.CreateProgram() = 
        this.createProgram()

    member inline this.AttachShader(program, shader) = 
        this.attachShader(program, shader)

    member inline this.LinkProgram(program : GLProgram) = 
        this.linkProgram(program)
    
    member inline this.UseProgram(program : GLProgram) = 
        this.useProgram(program)
    
    member inline this.DetachShader(program : GLProgram, shader) = 
        this.detachShader(program, shader)

    member inline this.DeleteShader(shader) = 
        this.deleteShader(shader)
    
    member inline this.GenVertexArray() : GLVertexArray =
        this?createVertexArray()

    member inline this.BindBuffer(target: BufferTargetARB, buffer: GLBuffer) =
        this.bindBuffer(unbox target, unbox buffer)

    member inline this.BindVertexArray(array : GLVertexArray) =
        this?bindVertexArray(unbox array)

    member inline this.VertexAttribPointer(index: uint32, size: int, typ : VertexAttribPointerType, normalized: bool, stride: uint32, pointer: int) =
        this.vertexAttribPointer(unbox index, unbox size, unbox typ, normalized, unbox stride, unbox pointer)

    member inline this.EnableVertexAttribArray(index : uint32) =
        this.enableVertexAttribArray(unbox index)

    member inline this.DrawArrays(mode : PrimitiveType, first: int, count: uint32) =
        this.drawArrays(unbox mode, first, unbox count)
    
    member inline this.GenTexture() =
        this.createTexture()

    member inline this.ActiveTexture(texture : TextureUnit) =
        this.activeTexture(unbox texture)
        
    member inline this.BindTexture (target: TextureTarget, texture: GLTexture) =
        this.bindTexture(unbox target, unbox texture)
 
    member inline this.BindTexture (target: TextureTarget, texture: uint) =
        () //this.bindTexture(unbox target, unbox texture)
 
    member inline this.TexImage2D(target : TextureTarget, level, internalformat : InternalFormat, format : PixelFormat, typ : PixelType, data) =
        this.texImage2D(unbox target, unbox level, unbox internalformat, unbox format, unbox typ, (^a : (member Pixels : GLTexturePixels) data))

    member inline this.TexImage2D(target : TextureTarget, level : int, internalformat : InternalFormat, width: uint32, height : uint32, border: int, format : PixelFormat, typ : PixelType, offset : byte[]) =
        this?texImage2D(target, level, internalformat, width, height, border, format, typ, offset)

    member inline this.TexStorage3D(target : TextureTarget, levels: uint32, internalformat: SizedInternalFormat, width: uint32, height: uint32, depth: uint32) =
        this?texStorage3D(target, levels, internalformat, width, height, depth)

    member inline this.TexSubImage3D(target : TextureTarget, level, xoffset, yoffset, zoffset, depth, format : PixelFormat, typ : PixelType, data) =
        this?texSubImage3D(target, level, xoffset, yoffset, zoffset, (^a : (member Width : uint) data), (^a : (member Height : uint) data), depth, format, typ, (^a : (member Pixels : GLTexturePixels) data))

    member inline this.TexParameter(texture : TextureTarget, pname: TextureParameterName, param: int) =
        this.texParameteri(unbox texture, unbox pname, unbox param)

    member inline this.GenerateMipmap(target : TextureTarget) =
        this.generateMipmap(unbox target)

    member inline this.GetUniformLocation(program, name) : GLUniformLocation =
        this.getUniformLocation(program, name)
    
    member inline this.GetAttribLocation(program, name) : GLAttribLocation =
        this.getAttribLocation(program, name)

    member inline this.Uniform1 (location : GLUniformLocation, v0: int) =
        this.uniform1i(location, v0)

    member inline this.Uniform1 (location : GLUniformLocation, v0: float32) =
        this.uniform1f(location, unbox v0)

    member inline this.Uniform2 (location : GLUniformLocation, v0: int, v1: int) =
        this.uniform2i(location, unbox v0, unbox v1)

    member inline this.Uniform2 (location : GLUniformLocation, v0: float32, v1: float32) =
        this.uniform2f(location, unbox v0, unbox v1)

    member inline this.Uniform3 (location : GLUniformLocation, v0: float32, v1: float32, v2: float32) =
        this.uniform3f(location, unbox v0, unbox v1, unbox v2)

    member inline this.Uniform4 (location : GLUniformLocation, v0: float32, v1: float32, v3: float32, v4: float32) =
        this.uniform4f(location, unbox v0, unbox v1, unbox v3, unbox v4)

    member inline this.Enable(cap: EnableCap) =
        this.enable(unbox cap)

    member inline this.BlendFunc(sfactor: BlendingFactor, dfactor: BlendingFactor) =
        this.blendFunc(unbox sfactor, unbox dfactor)

    member inline gl.BlendEquationSeparate(a : BlendEquationModeEXT, b : BlendEquationModeEXT) =
        gl.blendEquationSeparate(unbox a, unbox b)

    member inline this.VertexAttribDivisor(index: uint32, divisor: uint32) =
        this?vertexAttribDivisor(index, divisor)

    member inline this.Disable(cap : EnableCap) =
        this.disable(unbox cap)

    member inline gl.UnbindBuffer(target: BufferTargetARB) =
        gl.bindBuffer(unbox target, null)

    member inline gl.DrawArraysInstanced(mode: PrimitiveType, first: int, count: uint32, instancecount: uint32) =
        gl?drawArraysInstanced(mode, first, count, instancecount)

    member inline gl.CullFace(face : TriangleFace) = 
        gl.cullFace(unbox face)

    member inline gl.FrontFace(direction : FrontFaceDirection) = 
        gl.frontFace(unbox direction)

    member inline gl.DepthFunc(func: DepthFunction) =
        gl.depthFunc(unbox func);

    member inline gl.ClearDepth(d: float32) =
        gl.clearDepth(unbox d);

    member inline gl.DeleteBuffer(buffers : GLBuffer) =
        gl.deleteBuffer(buffers)
  
    member inline gl.GetCurrentFramebuffer() : GLFramebuffer =
        gl?getParameter(gl.FRAMEBUFFER_BINDING)

    member inline gl.GetCurrentProgram() : Browser.Types.WebGLProgram =
        gl?getParameter(gl.CURRENT_PROGRAM)

    member inline gl.GenFramebuffer() =
        gl.createFramebuffer()      
 
    member inline gl.BindFramebuffer(target : FramebufferTarget, framebuffer) =
        gl.bindFramebuffer(unbox target, framebuffer)   

    member inline gl.FramebufferTexture2D(target, attachment, textarget, texture, level) =
        gl.framebufferTexture2D(unbox target, unbox attachment, unbox textarget, unbox texture, unbox level)
        
    member inline gl.DrawBuffers(buffers) =
        gl?drawBuffers(buffers)

    member inline gl.Viewport(x, y, width, height) =
        gl.viewport(unbox x, unbox y, unbox width, unbox height)

    member inline gl.DeleteTexture(t) =
        gl.deleteTexture(t)

    member inline gl.DeleteFramebuffer(f) =
        gl.deleteFramebuffer(f)


#endif