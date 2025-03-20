module Tools.Shaders

open System.Text.RegularExpressions
open System.IO
open Legivel.Serialization
open Imp.Tools.Common

let includeReg = Regex("#include \"([^\"]+)\"", RegexOptions.IgnoreCase) 

let rec readFile (path : string) =
    let dir = Path.GetDirectoryName(path)
    let file = path |> File.ReadAllText

    let evaluator = MatchEvaluator(fun m -> 
        let a = dir </> m.Groups.[1].Value
        readFile a
    )

    includeReg.Replace(file, evaluator)

type InputTypes = 
    | AttributeFloat
    | AttributeVector2
    | AttributeVector3
    | AttributeVector4
    | UniformFloat
    | UniformVector2
    //| UniformVector2I
    | UniformVector3
    | UniformVector4
    | UniformMatrix4
    | Sampler2D of int
    | Sampler2DArray of int

type ShaderItem = 
    { Name : string
      Type : InputTypes }

let parseAttributes vertex fragment =
    let parse s =
        Regex("(attribute|uniform|in|out)(?: mediump)? (float|vec2|vec4|vec3|sampler2D|sampler2DArray|mat4) +([a-z0-9_]+).*;", RegexOptions.IgnoreCase).Matches(s) |> Seq.cast<Match> |> Seq.toList

    let items = 
        parse vertex @ parse fragment

    let outs = 
        items
        |> List.choose(fun m -> 
            if m.Groups.[1].Value = "out" then
                Some m.Groups.[3].Value
            else
                None
        ) 

    let ins =
        items
        |> List.filter(fun m ->
            outs |> List.contains m.Groups.[3].Value |> not
        )


    (0, ins)
    ||> List.mapFold(fun t m ->
        let isAttribute = m.Groups.[1].Value = "attribute" || m.Groups.[1].Value = "in"
        let name = m.Groups.[3].Value

        let typ, t' =
            match m.Groups.[2].Value with
            | "float" when isAttribute -> InputTypes.AttributeFloat, t
            | "vec2" when isAttribute -> InputTypes.AttributeVector2, t
            | "vec3" when isAttribute -> InputTypes.AttributeVector3, t
            | "vec4" when isAttribute -> InputTypes.AttributeVector4, t
            | "float" -> InputTypes.UniformFloat, t
            | "vec2" -> InputTypes.UniformVector2, t
            //| "ivec2" -> InputTypes.UniformVector2I, t
            | "vec3" -> InputTypes.UniformVector3, t
            | "vec4" -> InputTypes.UniformVector4, t
            | "sampler2D" -> InputTypes.Sampler2D t, t + 1
            | "sampler2DArray" -> InputTypes.Sampler2DArray t, t + 1
            | "" -> InputTypes.UniformMatrix4, t
            | _ -> invalidOp "Unknown attribute or uniform"



        { Name = name
          Type = typ
          }, t'
    )
    |> fst

let formatLabel (label : string) =                
    match label.ToCharArray() |> Array.toList with
    | a::b ->  
        [(a.ToString().ToUpper())] @ (b |> List.map(fun (v : char) -> v.ToString()))
        |> String.concat ""
    | _ -> label

type Shader = 
    { Name : string
      Vertex : string
      Fragment : string }

let re = Regex("#include \"([^\"]+)\"", RegexOptions.IgnoreCase) 

let rec readShaderFile (path : string) =
    let dir = Path.GetDirectoryName(path)
    let file = path |> File.ReadAllText

    let evaluator = MatchEvaluator(fun m -> 
        let a = dir </> m.Groups.[1].Value
        readFile a
    )

    re.Replace(file, evaluator)

let buildSharder root (shader : Shader) = 
    let vert = readShaderFile (root </> shader.Vertex)
    let frag = readShaderFile (root </> shader.Fragment)

    let r = parseAttributes vert frag

    let vert = vert.Replace("\r\n", "\\r\\n")
    let frag = frag.Replace("\r\n", "\\r\\n")

    let locations = 
        r
        |> List.choose(fun a ->
            let location = a.Name + "Location"
            match a.Type with
            | AttributeFloat
            | AttributeVector2
            | AttributeVector3
            | AttributeVector4 ->
                let size = 
                    match a.Type with
                    | AttributeFloat -> 1
                    | AttributeVector2 -> 2
                    | AttributeVector3 -> 3
                    | AttributeVector4 -> 4
                    | _ -> invalidOp "Not an AttributeVector"

                $$"""
    let {{location}} = gl.GetAttribLocation(program, "{{a.Name}}") |> uint
                """ |> Some

            | UniformFloat
            | UniformVector2
            //| UniformVector2I
            | UniformVector3
            | UniformVector4
            | Sampler2D _
            | Sampler2DArray _ ->
                $$"""
    let {{location}} = gl.GetUniformLocation(program, "{{a.Name}}")
                """ |> Some

            | _ ->
                None
        ) |> String.concat "\r\n"

    let init = 
        r
        |> List.choose(fun a ->
            let location = a.Name + "Location"
            match a.Type with
            | AttributeFloat
            | AttributeVector2
            | AttributeVector3
            | AttributeVector4 ->
                let size = 
                    match a.Type with
                    | AttributeFloat -> 1
                    | AttributeVector2 -> 2
                    | AttributeVector3 -> 3
                    | AttributeVector4 -> 4
                    | _ -> invalidOp "Not an AttributeVector"

                $$"""
            gl.EnableVertexAttribArray({{location}})
                """ |> Some

            | _ -> None
        ) |> String.concat "\r\n"

    let sets = 
        r
        |> List.choose(fun a ->
            let name = a.Name |> formatLabel
            let location = a.Name + "Location"
            match a.Type with
            | AttributeFloat
            | AttributeVector2
            | AttributeVector3
            | AttributeVector4 ->
                let size = 
                    match a.Type with
                    | AttributeFloat -> 1
                    | AttributeVector2 -> 2
                    | AttributeVector3 -> 3
                    | AttributeVector4 -> 4
                    | _ -> invalidOp "Not an AttributeVector"

                $$"""
    member _.Set{{name}}(buffer, ?divisor) =
        let gl = gl
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, buffer)
        gl.VertexAttribPointer({{location}}, {{size}}, VertexAttribPointerType.Float, false, uint32 ({{size}} * Float32Size), pointerOffset 0 Float32Size)
        gl.VertexAttribDivisor({{location}}, divisor |> Option.defaultValue 0u)
            """ |> Some

            | UniformFloat ->
                $$"""
    member _.Set{{name}}(x : float32) =
        gl.Uniform1({{location}}, x);   
                """ |> Some

            | UniformVector2 ->
                $$"""
    member _.Set{{name}}(x : float32, y : float32) =
        gl.Uniform2({{location}}, x, y);   
                """ |> Some

    //        | UniformVector2I ->
    //            $$"""
    //member _.Set{{name}}(x : int, y : int) =
    //    gl.Uniform2({{location}}, x, y);   
    //            """ |> Some

            | UniformVector3 ->
                $$"""
    member _.Set{{name}}(x : float32, y : float32, z : float32) =
        gl.Uniform3({{location}}, x, y, z);   
                """ |> Some

            | UniformVector4 ->
                $$"""
    member _.Set{{name}}(x : float32, y : float32, z : float32, w : float32) =
        gl.Uniform4({{location}}, x, y, z, w);   
                """ |> Some

            | Sampler2D index ->
                $$"""
    member _.Set{{name}}(value : GLTexture) =
        let gl = gl
        gl.Uniform1({{location}}, {{index}})  
        gl.ActiveTexture(TextureUnit.Texture{{index}})
        gl.BindTexture(TextureTarget.Texture2D, value)
                """ |> Some

            | Sampler2DArray index ->
                $$"""
    member _.Set{{name}}(value : GLTexture) =
        let gl = gl
        gl.Uniform1({{location}}, {{index}})  
        gl.ActiveTexture(TextureUnit.Texture{{index}})
        gl.BindTexture(TextureTarget.Texture2DArray, value)
                """ |> Some

            | _ -> None
        ) |> String.concat "\r\n"

    $$"""type {{shader.Name}}(gl : GL) = 
    let vertex = "{{vert}}"
    let fragment = "{{frag}}"

    let vertexArray = gl.GenVertexArray()
    let mutable initialized = false

    let program = 
        let p = gl |> createShader vertex fragment
        gl.UseProgram(p)
        p



{{locations}}

    member _.Program = program

    member _.Use() =
        gl.UseProgram(program)
        gl.BindVertexArray(vertexArray)

        if not initialized then
            initialized <- true

{{init}}

{{sets}}

module {{shader.Name}} = 
    let mutable shader = ValueNone
    let reuse (gl : GL) fn =
        match shader with
        | ValueSome (s : {{shader.Name}}) ->
            let current = gl.GetCurrentProgram()
            if current <> s.Program then
                s.Use()
                fn s

        | ValueNone ->
            let s = {{shader.Name}}(gl)
            s.Use()
            fn s
            shader <- ValueSome s
            

        shader.Value

    """

let convert (config : string) (moduleName : string) (outputFilename : string) =
    let dir = Path.GetDirectoryName(config)

    let shaders =
        File.ReadAllText(config)
        |> Deserialize<Shader list>
        |> fun x ->
            match x with
            | DeserializeResult.Success y :: _ -> y.Data
            | _ -> invalidOp "bang"


    let items = 
        let root = Path.GetDirectoryName(config)
        shaders
        |> Seq.map(fun item ->
            buildSharder root item
        )
        |> String.concat "\r\n"
                

    let output = $$"""module {{moduleName}}

#if !FABLE_COMPILER
open Silk.NET.OpenGL
#endif

open Imp.GL

{{items}}

"""

    if saveIfChanged outputFilename output then
        //let toDelete = dir </> "../bin/Debug/net8.0/Client.Shared.dll"
        //if File.Exists toDelete then
        //    File.Delete toDelete
        ()