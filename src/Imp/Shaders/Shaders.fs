namespace Client.Shaders

#if !FABLE_COMPILER
open Silk.NET.OpenGL
#else
open Silk.NET.WebGL
#endif

open Silk.NET.Helpers

type BatchSprite(gl) = 
    let vertex = "#version 300 es\r\nprecision highp float;\r\n\r\nin vec4 vertex;\r\nin vec4 position;\r\nin vec4 texturePosition;\r\nin float textureLayer;\r\nin vec4 animation;\r\nin float palette;\r\n\r\nin float depth;\r\n\r\nuniform vec2 outputResolution;\r\nuniform vec2 textureResolution;\r\nuniform float time;\r\nuniform vec2 offset;\r\n\r\nout vec3 frag_texCoords;\r\nout float frag_depth;\r\nout float frag_palette;\r\n\r\nvoid main(void)\r\n{\r\n    float a = mod(time, animation.w);\r\n    if (animation.x > 0.0 && a >= animation.y && a < animation.z)\r\n    {\r\n        gl_Position = vec4(0.0);\r\n    }\r\n    else\r\n    {\r\n        // y might need to be flipped\r\n        frag_texCoords = vec3(mix(vec2(0.0, 0.0), vec2(1.0, 1.0), (texturePosition.xy + (texturePosition.zw * vertex.zw)) / textureResolution), textureLayer);\r\n\r\n        float x = mix(-1.0, 1.0, (offset.x + position.x + (position.z * vertex.x)) / outputResolution.x);\r\n        float y = mix(1.0, -1.0, (offset.y + position.y + (position.w * (1.0 - vertex.y))) / outputResolution.y);\r\n\r\n        gl_Position = vec4(x, y, 0.0, 1.0);\r\n        frag_depth = depth;\r\n        frag_palette = frag_palette;\r\n    }\r\n}"
    let fragment = "#version 300 es\r\nprecision highp float;\r\n\r\nin float frag_depth;\r\nin vec3 frag_texCoords;\r\nin float frag_palette;\r\nuniform mediump sampler2DArray diffuseTexture;\r\nuniform float depthOffset;\r\nuniform float maxDepth;\r\nuniform sampler2D paletteTexture;\r\n\r\nout vec4 outputColor;\r\n\r\nvoid main()\r\n{\r\n    vec4 textureColor = texture(diffuseTexture, frag_texCoords);\r\n    outputColor = texture(paletteTexture, vec2(frag_palette, textureColor.a)); // 1.0 - x need to be flip when the textures are?\r\n\r\n    gl_FragDepth = textureColor.a > 0.0 ? 1.0 - ((frag_depth + depthOffset + (textureColor.r * 255.0)) / maxDepth) : 1.0;\r\n}"

    let program = 
        let p = gl |> createShader vertex fragment
        gl.UseProgram(p)
        p


    let vertexLocation = gl.GetAttribLocation(program, "vertex") |> uint
                

    let positionLocation = gl.GetAttribLocation(program, "position") |> uint
                

    let texturePositionLocation = gl.GetAttribLocation(program, "texturePosition") |> uint
                

    let textureLayerLocation = gl.GetAttribLocation(program, "textureLayer") |> uint
                

    let animationLocation = gl.GetAttribLocation(program, "animation") |> uint
                

    let paletteLocation = gl.GetAttribLocation(program, "palette") |> uint
                

    let depthLocation = gl.GetAttribLocation(program, "depth") |> uint
                

    let outputResolutionLocation = gl.GetUniformLocation(program, "outputResolution")
                

    let textureResolutionLocation = gl.GetUniformLocation(program, "textureResolution")
                

    let timeLocation = gl.GetUniformLocation(program, "time")
                

    let offsetLocation = gl.GetUniformLocation(program, "offset")
                

    let diffuseTextureLocation = gl.GetUniformLocation(program, "diffuseTexture")
                

    let depthOffsetLocation = gl.GetUniformLocation(program, "depthOffset")
                

    let maxDepthLocation = gl.GetUniformLocation(program, "maxDepth")
                

    let paletteTextureLocation = gl.GetUniformLocation(program, "paletteTexture")
                

    member _.Use() =
        gl.UseProgram(program)


    member _.SetVertex(buffer, ?divisor) =
        let gl = gl
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, buffer)
        gl.EnableVertexAttribArray(vertexLocation)
        gl.VertexAttribPointer(vertexLocation, 4, VertexAttribPointerType.Float, false, uint32 (4 * Float32Size), pointerOffset 0 Float32Size)
        gl.VertexAttribDivisor(vertexLocation, divisor |> Option.defaultValue 0u)
            

    member _.SetPosition(buffer, ?divisor) =
        let gl = gl
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, buffer)
        gl.EnableVertexAttribArray(positionLocation)
        gl.VertexAttribPointer(positionLocation, 4, VertexAttribPointerType.Float, false, uint32 (4 * Float32Size), pointerOffset 0 Float32Size)
        gl.VertexAttribDivisor(positionLocation, divisor |> Option.defaultValue 0u)
            

    member _.SetTexturePosition(buffer, ?divisor) =
        let gl = gl
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, buffer)
        gl.EnableVertexAttribArray(texturePositionLocation)
        gl.VertexAttribPointer(texturePositionLocation, 4, VertexAttribPointerType.Float, false, uint32 (4 * Float32Size), pointerOffset 0 Float32Size)
        gl.VertexAttribDivisor(texturePositionLocation, divisor |> Option.defaultValue 0u)
            

    member _.SetTextureLayer(buffer, ?divisor) =
        let gl = gl
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, buffer)
        gl.EnableVertexAttribArray(textureLayerLocation)
        gl.VertexAttribPointer(textureLayerLocation, 1, VertexAttribPointerType.Float, false, uint32 (1 * Float32Size), pointerOffset 0 Float32Size)
        gl.VertexAttribDivisor(textureLayerLocation, divisor |> Option.defaultValue 0u)
            

    member _.SetAnimation(buffer, ?divisor) =
        let gl = gl
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, buffer)
        gl.EnableVertexAttribArray(animationLocation)
        gl.VertexAttribPointer(animationLocation, 4, VertexAttribPointerType.Float, false, uint32 (4 * Float32Size), pointerOffset 0 Float32Size)
        gl.VertexAttribDivisor(animationLocation, divisor |> Option.defaultValue 0u)
            

    member _.SetPalette(buffer, ?divisor) =
        let gl = gl
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, buffer)
        gl.EnableVertexAttribArray(paletteLocation)
        gl.VertexAttribPointer(paletteLocation, 1, VertexAttribPointerType.Float, false, uint32 (1 * Float32Size), pointerOffset 0 Float32Size)
        gl.VertexAttribDivisor(paletteLocation, divisor |> Option.defaultValue 0u)
            

    member _.SetDepth(buffer, ?divisor) =
        let gl = gl
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, buffer)
        gl.EnableVertexAttribArray(depthLocation)
        gl.VertexAttribPointer(depthLocation, 1, VertexAttribPointerType.Float, false, uint32 (1 * Float32Size), pointerOffset 0 Float32Size)
        gl.VertexAttribDivisor(depthLocation, divisor |> Option.defaultValue 0u)
            

    member _.SetOutputResolution(x : float32, y : float32) =
        gl.Uniform2(outputResolutionLocation, x, y);   
                

    member _.SetTextureResolution(x : float32, y : float32) =
        gl.Uniform2(textureResolutionLocation, x, y);   
                

    member _.SetTime(x : float32) =
        gl.Uniform1(timeLocation, x);   
                

    member _.SetOffset(x : float32, y : float32) =
        gl.Uniform2(offsetLocation, x, y);   
                

    member _.SetDiffuseTexture(value : GLTexture) =
        let gl = gl
        gl.Uniform1(diffuseTextureLocation, 0)  
        gl.ActiveTexture(TextureUnit.Texture0)
        gl.BindTexture(TextureTarget.Texture2DArray, value)
                

    member _.SetDepthOffset(x : float32) =
        gl.Uniform1(depthOffsetLocation, x);   
                

    member _.SetMaxDepth(x : float32) =
        gl.Uniform1(maxDepthLocation, x);   
                

    member _.SetPaletteTexture(value : GLTexture) =
        let gl = gl
        gl.Uniform1(paletteTextureLocation, 1)  
        gl.ActiveTexture(TextureUnit.Texture1)
        gl.BindTexture(TextureTarget.Texture2D, value)
                

    
type SingleSprite(gl) = 
    let vertex = "#version 300 es\r\nprecision highp float;\r\n\r\nin vec4 vertex;\r\nuniform vec4 position;\r\nuniform vec4 texturePosition;\r\nuniform float textureLayer;\r\nuniform vec4 animation;\r\nuniform float palette;\r\n\r\nuniform float depth;\r\n\r\nuniform vec2 outputResolution;\r\nuniform vec2 textureResolution;\r\nuniform float time;\r\nuniform vec2 offset;\r\n\r\nout vec3 frag_texCoords;\r\nout float frag_depth;\r\nout float frag_palette;\r\n\r\nvoid main(void)\r\n{\r\n    float a = mod(time, animation.w);\r\n    if (animation.x > 0.0 && a >= animation.y && a < animation.z)\r\n    {\r\n        gl_Position = vec4(0.0);\r\n    }\r\n    else\r\n    {\r\n        // y might need to be flipped\r\n        frag_texCoords = vec3(mix(vec2(0.0, 0.0), vec2(1.0, 1.0), (texturePosition.xy + (texturePosition.zw * vertex.zw)) / textureResolution), textureLayer);\r\n\r\n        float x = mix(-1.0, 1.0, (offset.x + position.x + (position.z * vertex.x)) / outputResolution.x);\r\n        float y = mix(1.0, -1.0, (offset.y + position.y + (position.w * (1.0 - vertex.y))) / outputResolution.y);\r\n\r\n        gl_Position = vec4(x, y, 0.0, 1.0);\r\n        frag_depth = depth + (x * 0.0000001);\r\n        frag_palette = palette;\r\n    }\r\n}"
    let fragment = "#version 300 es\r\nprecision highp float;\r\n\r\nin float frag_depth;\r\nin vec3 frag_texCoords;\r\nin float frag_palette;\r\nuniform mediump sampler2DArray diffuseTexture;\r\nuniform float depthOffset;\r\nuniform float maxDepth;\r\nuniform sampler2D paletteTexture;\r\n\r\nout vec4 outputColor;\r\n\r\nvoid main()\r\n{\r\n    vec4 textureColor = texture(diffuseTexture, frag_texCoords);\r\n    outputColor = texture(paletteTexture, vec2(frag_palette, textureColor.a)); // 1.0 - x need to be flip when the textures are?\r\n\r\n    gl_FragDepth = textureColor.a > 0.0 ? 1.0 - ((frag_depth + depthOffset + (textureColor.r * 255.0)) / maxDepth) : 1.0;\r\n}"

    let program = 
        let p = gl |> createShader vertex fragment
        gl.UseProgram(p)
        p


    let vertexLocation = gl.GetAttribLocation(program, "vertex") |> uint
                

    let positionLocation = gl.GetUniformLocation(program, "position")
                

    let texturePositionLocation = gl.GetUniformLocation(program, "texturePosition")
                

    let textureLayerLocation = gl.GetUniformLocation(program, "textureLayer")
                

    let animationLocation = gl.GetUniformLocation(program, "animation")
                

    let paletteLocation = gl.GetUniformLocation(program, "palette")
                

    let depthLocation = gl.GetUniformLocation(program, "depth")
                

    let outputResolutionLocation = gl.GetUniformLocation(program, "outputResolution")
                

    let textureResolutionLocation = gl.GetUniformLocation(program, "textureResolution")
                

    let timeLocation = gl.GetUniformLocation(program, "time")
                

    let offsetLocation = gl.GetUniformLocation(program, "offset")
                

    let diffuseTextureLocation = gl.GetUniformLocation(program, "diffuseTexture")
                

    let depthOffsetLocation = gl.GetUniformLocation(program, "depthOffset")
                

    let maxDepthLocation = gl.GetUniformLocation(program, "maxDepth")
                

    let paletteTextureLocation = gl.GetUniformLocation(program, "paletteTexture")
                

    member _.Use() =
        gl.UseProgram(program)


    member _.SetVertex(buffer, ?divisor) =
        let gl = gl
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, buffer)
        gl.EnableVertexAttribArray(vertexLocation)
        gl.VertexAttribPointer(vertexLocation, 4, VertexAttribPointerType.Float, false, uint32 (4 * Float32Size), pointerOffset 0 Float32Size)
        gl.VertexAttribDivisor(vertexLocation, divisor |> Option.defaultValue 0u)
            

    member _.SetPosition(x : float32, y : float32, z : float32, w : float32) =
        gl.Uniform4(positionLocation, x, y, z, w);   
                

    member _.SetTexturePosition(x : float32, y : float32, z : float32, w : float32) =
        gl.Uniform4(texturePositionLocation, x, y, z, w);   
                

    member _.SetTextureLayer(x : float32) =
        gl.Uniform1(textureLayerLocation, x);   
                

    member _.SetAnimation(x : float32, y : float32, z : float32, w : float32) =
        gl.Uniform4(animationLocation, x, y, z, w);   
                

    member _.SetPalette(x : float32) =
        gl.Uniform1(paletteLocation, x);   
                

    member _.SetDepth(x : float32) =
        gl.Uniform1(depthLocation, x);   
                

    member _.SetOutputResolution(x : float32, y : float32) =
        gl.Uniform2(outputResolutionLocation, x, y);   
                

    member _.SetTextureResolution(x : float32, y : float32) =
        gl.Uniform2(textureResolutionLocation, x, y);   
                

    member _.SetTime(x : float32) =
        gl.Uniform1(timeLocation, x);   
                

    member _.SetOffset(x : float32, y : float32) =
        gl.Uniform2(offsetLocation, x, y);   
                

    member _.SetDiffuseTexture(value : GLTexture) =
        let gl = gl
        gl.Uniform1(diffuseTextureLocation, 0)  
        gl.ActiveTexture(TextureUnit.Texture0)
        gl.BindTexture(TextureTarget.Texture2DArray, value)
                

    member _.SetDepthOffset(x : float32) =
        gl.Uniform1(depthOffsetLocation, x);   
                

    member _.SetMaxDepth(x : float32) =
        gl.Uniform1(maxDepthLocation, x);   
                

    member _.SetPaletteTexture(value : GLTexture) =
        let gl = gl
        gl.Uniform1(paletteTextureLocation, 1)  
        gl.ActiveTexture(TextureUnit.Texture1)
        gl.BindTexture(TextureTarget.Texture2D, value)
                

    
type Simple(gl) = 
    let vertex = "#version 300 es\r\nprecision highp float;\r\n\r\nin vec4 vertex;\r\n\r\nout vec2 frag_texCoords;\r\n\r\nvoid main(void)\r\n{\r\n    frag_texCoords = vec2(vertex.z, 1.0 - vertex.w);\r\n\r\n    gl_Position = vec4((vertex.x - 0.5) * 2.0, ((vertex.y - 0.5) * 2.0), 0.0, 1.0);\r\n}"
    let fragment = "#version 300 es\r\nprecision highp float;\r\n\r\nin vec2 frag_texCoords;\r\nuniform sampler2D diffuseTexture;\r\n\r\nout vec4 outputColor;\r\n\r\nvoid main()\r\n{\r\n    outputColor =  texture(diffuseTexture, frag_texCoords);\r\n    gl_FragDepth = outputColor.a > 0.0 ? 0.3 : 0.0;\r\n}"

    let program = 
        let p = gl |> createShader vertex fragment
        gl.UseProgram(p)
        p


    let vertexLocation = gl.GetAttribLocation(program, "vertex") |> uint
                

    let diffuseTextureLocation = gl.GetUniformLocation(program, "diffuseTexture")
                

    member _.Use() =
        gl.UseProgram(program)


    member _.SetVertex(buffer, ?divisor) =
        let gl = gl
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, buffer)
        gl.EnableVertexAttribArray(vertexLocation)
        gl.VertexAttribPointer(vertexLocation, 4, VertexAttribPointerType.Float, false, uint32 (4 * Float32Size), pointerOffset 0 Float32Size)
        gl.VertexAttribDivisor(vertexLocation, divisor |> Option.defaultValue 0u)
            

    member _.SetDiffuseTexture(value : GLTexture) =
        let gl = gl
        gl.Uniform1(diffuseTextureLocation, 0)  
        gl.ActiveTexture(TextureUnit.Texture0)
        gl.BindTexture(TextureTarget.Texture2D, value)
                

    

