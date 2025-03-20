module Imp.Shaders

#if !FABLE_COMPILER
open Silk.NET.OpenGL
#endif

open Imp.GL

type Single(gl : GL) = 
    let vertex = "#version 300 es\r\nprecision highp float;\r\n\r\nin vec4 vertex;\r\nuniform vec4 position;\r\nuniform vec4 texturePosition;\r\nuniform float textureLayer;\r\nuniform float depth;\r\n\r\nuniform vec2 outputResolution;\r\nuniform vec2 textureResolution;\r\n\r\nout vec3 frag_texCoords;\r\nout float frag_depth;\r\n\r\nvoid main(void)\r\n{\r\n    // y might need to be flipped\r\n    frag_texCoords =\r\n        vec3(\r\n            mix(vec2(0.0, 0.0), vec2(1.0, 1.0), (texturePosition.xy + (texturePosition.zw * vertex.zw)) / textureResolution),\r\n            textureLayer);\r\n\r\n    float x = mix(-1.0, 1.0, (position.x + (position.z * vertex.x)) / outputResolution.x);\r\n    float y = mix(1.0, -1.0, (position.y + (position.w * (1.0 - vertex.y))) / outputResolution.y);\r\n\r\n    gl_Position = vec4(x, y, 0.0, 1.0);\r\n    frag_depth = depth;\r\n}"
    let fragment = "#version 300 es\r\nprecision highp float;\r\n\r\nin vec3 frag_texCoords;\r\nin float frag_depth;\r\nuniform mediump sampler2DArray diffuseTexture;\r\n\r\nout vec4 outputColor;\r\n\r\nvoid main() {\r\n    outputColor = texture(diffuseTexture, frag_texCoords);\r\n    gl_FragDepth = outputColor.a > 0.0 ? frag_depth : 1.0;\r\n}"

    let vertexArray = gl.GenVertexArray()
    let mutable initialized = false

    let program = 
        let p = gl |> createShader vertex fragment
        gl.UseProgram(p)
        p




    let vertexLocation = gl.GetAttribLocation(program, "vertex") |> uint
                

    let positionLocation = gl.GetUniformLocation(program, "position")
                

    let texturePositionLocation = gl.GetUniformLocation(program, "texturePosition")
                

    let textureLayerLocation = gl.GetUniformLocation(program, "textureLayer")
                

    let depthLocation = gl.GetUniformLocation(program, "depth")
                

    let outputResolutionLocation = gl.GetUniformLocation(program, "outputResolution")
                

    let textureResolutionLocation = gl.GetUniformLocation(program, "textureResolution")
                

    let diffuseTextureLocation = gl.GetUniformLocation(program, "diffuseTexture")
                

    member _.Program = program

    member _.Use() =
        gl.UseProgram(program)
        gl.BindVertexArray(vertexArray)

        if not initialized then
            initialized <- true


            gl.EnableVertexAttribArray(vertexLocation)
                


    member _.SetVertex(buffer, ?divisor) =
        let gl = gl
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, buffer)
        gl.VertexAttribPointer(vertexLocation, 4, VertexAttribPointerType.Float, false, uint32 (4 * Float32Size), pointerOffset 0 Float32Size)
        gl.VertexAttribDivisor(vertexLocation, divisor |> Option.defaultValue 0u)
            

    member _.SetPosition(x : float32, y : float32, z : float32, w : float32) =
        gl.Uniform4(positionLocation, x, y, z, w);   
                

    member _.SetTexturePosition(x : float32, y : float32, z : float32, w : float32) =
        gl.Uniform4(texturePositionLocation, x, y, z, w);   
                

    member _.SetTextureLayer(x : float32) =
        gl.Uniform1(textureLayerLocation, x);   
                

    member _.SetDepth(x : float32) =
        gl.Uniform1(depthLocation, x);   
                

    member _.SetOutputResolution(x : float32, y : float32) =
        gl.Uniform2(outputResolutionLocation, x, y);   
                

    member _.SetTextureResolution(x : float32, y : float32) =
        gl.Uniform2(textureResolutionLocation, x, y);   
                

    member _.SetDiffuseTexture(value : GLTexture) =
        let gl = gl
        gl.Uniform1(diffuseTextureLocation, 0)  
        gl.ActiveTexture(TextureUnit.Texture0)
        gl.BindTexture(TextureTarget.Texture2DArray, value)
                

module Single = 
    let mutable shader = ValueNone
    let reuse (gl : GL) fn =
        match shader with
        | ValueSome (s : Single) ->
            let current = gl.GetCurrentProgram()
            if current <> s.Program then
                s.Use()
                fn s

        | ValueNone ->
            let s = Single(gl)
            s.Use()
            fn s
            shader <- ValueSome s
            

        shader.Value

    
type Batch(gl : GL) = 
    let vertex = "#version 300 es\r\nprecision highp float;\r\n\r\nin vec4 vertex;\r\nuniform vec3 offset;\r\nin vec4 position;\r\nin vec4 texturePosition;\r\nin float textureLayer;\r\nin float depth;\r\n\r\nuniform vec2 outputResolution;\r\nuniform vec2 textureResolution;\r\n\r\nout vec3 frag_texCoords;\r\nout float frag_depth;\r\n\r\nvoid main(void)\r\n{\r\n    // y might need to be flipped\r\n    frag_texCoords =\r\n        vec3(\r\n            mix(vec2(0.0, 0.0), vec2(1.0, 1.0), (texturePosition.xy + (texturePosition.zw * vertex.zw)) / textureResolution),\r\n            textureLayer);\r\n\r\n    float x = mix(-1.0, 1.0, (offset.x + position.x + (position.z * vertex.x)) / outputResolution.x);\r\n    float y = mix(1.0, -1.0, (offset.y + position.y + (position.w * (1.0 - vertex.y))) / outputResolution.y);\r\n\r\n    gl_Position = vec4(x, y, 0.0, 1.0);\r\n    frag_depth = offset.z + depth;\r\n}"
    let fragment = "#version 300 es\r\nprecision highp float;\r\n\r\nin vec3 frag_texCoords;\r\nin float frag_depth;\r\nuniform mediump sampler2DArray diffuseTexture;\r\n\r\nout vec4 outputColor;\r\n\r\nvoid main() {\r\n    outputColor = texture(diffuseTexture, frag_texCoords);\r\n    gl_FragDepth = outputColor.a > 0.0 ? frag_depth : 1.0;\r\n}"

    let vertexArray = gl.GenVertexArray()
    let mutable initialized = false

    let program = 
        let p = gl |> createShader vertex fragment
        gl.UseProgram(p)
        p




    let vertexLocation = gl.GetAttribLocation(program, "vertex") |> uint
                

    let offsetLocation = gl.GetUniformLocation(program, "offset")
                

    let positionLocation = gl.GetAttribLocation(program, "position") |> uint
                

    let texturePositionLocation = gl.GetAttribLocation(program, "texturePosition") |> uint
                

    let textureLayerLocation = gl.GetAttribLocation(program, "textureLayer") |> uint
                

    let depthLocation = gl.GetAttribLocation(program, "depth") |> uint
                

    let outputResolutionLocation = gl.GetUniformLocation(program, "outputResolution")
                

    let textureResolutionLocation = gl.GetUniformLocation(program, "textureResolution")
                

    let diffuseTextureLocation = gl.GetUniformLocation(program, "diffuseTexture")
                

    member _.Program = program

    member _.Use() =
        gl.UseProgram(program)
        gl.BindVertexArray(vertexArray)

        if not initialized then
            initialized <- true


            gl.EnableVertexAttribArray(vertexLocation)
                

            gl.EnableVertexAttribArray(positionLocation)
                

            gl.EnableVertexAttribArray(texturePositionLocation)
                

            gl.EnableVertexAttribArray(textureLayerLocation)
                

            gl.EnableVertexAttribArray(depthLocation)
                


    member _.SetVertex(buffer, ?divisor) =
        let gl = gl
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, buffer)
        gl.VertexAttribPointer(vertexLocation, 4, VertexAttribPointerType.Float, false, uint32 (4 * Float32Size), pointerOffset 0 Float32Size)
        gl.VertexAttribDivisor(vertexLocation, divisor |> Option.defaultValue 0u)
            

    member _.SetOffset(x : float32, y : float32, z : float32) =
        gl.Uniform3(offsetLocation, x, y, z);   
                

    member _.SetPosition(buffer, ?divisor) =
        let gl = gl
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, buffer)
        gl.VertexAttribPointer(positionLocation, 4, VertexAttribPointerType.Float, false, uint32 (4 * Float32Size), pointerOffset 0 Float32Size)
        gl.VertexAttribDivisor(positionLocation, divisor |> Option.defaultValue 0u)
            

    member _.SetTexturePosition(buffer, ?divisor) =
        let gl = gl
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, buffer)
        gl.VertexAttribPointer(texturePositionLocation, 4, VertexAttribPointerType.Float, false, uint32 (4 * Float32Size), pointerOffset 0 Float32Size)
        gl.VertexAttribDivisor(texturePositionLocation, divisor |> Option.defaultValue 0u)
            

    member _.SetTextureLayer(buffer, ?divisor) =
        let gl = gl
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, buffer)
        gl.VertexAttribPointer(textureLayerLocation, 1, VertexAttribPointerType.Float, false, uint32 (1 * Float32Size), pointerOffset 0 Float32Size)
        gl.VertexAttribDivisor(textureLayerLocation, divisor |> Option.defaultValue 0u)
            

    member _.SetDepth(buffer, ?divisor) =
        let gl = gl
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, buffer)
        gl.VertexAttribPointer(depthLocation, 1, VertexAttribPointerType.Float, false, uint32 (1 * Float32Size), pointerOffset 0 Float32Size)
        gl.VertexAttribDivisor(depthLocation, divisor |> Option.defaultValue 0u)
            

    member _.SetOutputResolution(x : float32, y : float32) =
        gl.Uniform2(outputResolutionLocation, x, y);   
                

    member _.SetTextureResolution(x : float32, y : float32) =
        gl.Uniform2(textureResolutionLocation, x, y);   
                

    member _.SetDiffuseTexture(value : GLTexture) =
        let gl = gl
        gl.Uniform1(diffuseTextureLocation, 0)  
        gl.ActiveTexture(TextureUnit.Texture0)
        gl.BindTexture(TextureTarget.Texture2DArray, value)
                

module Batch = 
    let mutable shader = ValueNone
    let reuse (gl : GL) fn =
        match shader with
        | ValueSome (s : Batch) ->
            let current = gl.GetCurrentProgram()
            if current <> s.Program then
                s.Use()
                fn s

        | ValueNone ->
            let s = Batch(gl)
            s.Use()
            fn s
            shader <- ValueSome s
            

        shader.Value

    

