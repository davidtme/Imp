module Aseprite

open System.IO
open Imp.Tools.Common
open System

let asepritePath =
    let p = System.Environment.GetEnvironmentVariable("ASEPRITE_PATH")
    if String.IsNullOrWhiteSpace(p) then
        @"C:\Program Files\Aseprite\Aseprite.exe"
    else
        p

module Data =
    type Bounds =
        { X : int
          Y : int
          W : int
          H : int
          }

    type Point =
        { X : int
          Y : int }

    type Key = 
        { Frame : int
          Pivot : Point option
        }

    type Layer =
        { Name : string
          Color : string option
          Data : string option
          Group : string option
          BlendMode : string option }

    type Slice =
        { Name : string
          Keys : Key list
        }

    type Meta =
        { Layers : Layer list
          Slices : Slice list }

    type Root =
        { Meta : Meta }


open Data
open System.Text.RegularExpressions
open Legivel.Serialization

let (|Regex|_|) pattern input =
    let m = Regex.Match(input, pattern, RegexOptions.IgnoreCase)
    if m.Success then Some(List.tail [ for g in m.Groups -> g.Value.Trim(' ') ])
    else None

let (|Float|_|) (input : string) =
    match Double.TryParse(input) with
    | true, value -> Some value
    | _ -> None

let (|Ranges|_|) (input : string) =
    input.Split(',')
    |> Seq.collect(fun i -> 
                
        match i.Split([|".."|], StringSplitOptions.None) with
        | [| a |] -> [ Int32.Parse a ]
        | [| s; e |] ->
            let s = Int32.Parse(s)
            let e = Int32.Parse(e)
            [ for i = s to e do i ]
        | _ -> 
            []

    )
    |> Seq.toList
    |> Some

type ExtraYaml =
    { Prefix : string }

let extract (path : string) output =
    let filename = Path.GetFileNameWithoutExtension(path)
    ensureDirectory output

    let di = DirectoryInfo(output)
    for file in di.GetFiles() do
        file.Delete()

    let extraDetails =
        let yaml = Path.ChangeExtension(path, ".yaml")
        if File.Exists(yaml) then
            File.ReadAllText(yaml)
            |> Deserialize<ExtraYaml>
            |> fun x ->
                match x with
                | DeserializeResult.Success y :: _ -> y.Data
                | _ -> invalidOp "bang"
        else
            { Prefix = "" }

    let arg = [
        "-b"; "--split-slices"; "--split-layers"; "--ignore-empty"; "--all-layers"
        escapeFilename path
        "--data"; escapeFilename (output </> "aseprite.json")
        "--format"; "json-array"
        "--list-layers"; "--list-tags"; "--list-slices"; "--save-as";
        escapeFilename (output </> filename + $$"""~{{extraDetails.Prefix}}{slice}~{tag}~{frame}~{layer}.png""")
    ]

    startProcess asepritePath arg

    let yaml = Path.ChangeExtension(path, ".yaml")
    if File.Exists(yaml) then
        let extraDetails =
            File.ReadAllText(yaml)
            |> Deserialize<ExtraYaml>
            |> fun x ->
                match x with
                | DeserializeResult.Success y :: _ -> y.Data
                | _ -> invalidOp "bang"

        let dataPath = output </> "aseprite.json"
        let data : Data.Root = loadJson dataPath

        let data =
            { data with 
                Meta = 
                    { data.Meta with
                       Slices = data.Meta.Slices |> List.map(fun x -> { x with Name = $"{extraDetails.Prefix}{x.Name}"})
                    } }

        data |> saveJson dataPath



type SpriteData =
    { Name : string
      Slice : string
      Tag : string option
      Frame : int
      Offset : int*int
      Options : string list
      Color : string option
      SortOrder : int
    }


let decode path =
    let dataPath = path </> "aseprite.json"
    let data : Data.Root = loadJson dataPath

    File.Delete(dataPath)

    let files = Directory.EnumerateFiles(path, "*.png", SearchOption.TopDirectoryOnly) |> Seq.toList

    let result = new ResizeArray<_>()

    for inputFilename in files do
        match Path.GetFileNameWithoutExtension(inputFilename).Split('~') with
        | [| name; sliceName; tag; frame; layer |] ->
            let tag = if String.IsNullOrWhiteSpace(tag) then None else Some tag
            let frame = Int32.Parse(frame)

            match 
                data.Meta.Slices |> List.tryFind(fun x -> x.Name = sliceName),
                data.Meta.Layers |> List.tryFindIndex(fun x -> x.Name = layer)
                with
            | Some slice, Some layerIndex ->
                let layer = data.Meta.Layers.[layerIndex]

                let offset =
                    match slice.Keys |> List.tryFindBack(fun k -> k.Frame <= frame) with
                    | Some { Pivot = Some pivot } ->
                        -pivot.X, -pivot.Y
                    | _ ->
                        0,0

                let praseData (data : string option) = 
                    match data with
                    | Some data -> data.Split(";") |> List.ofArray |> List.map (fun x -> x.Trim(' '))
                    | _ -> []
                        

                let rec groupData groupName = 
                    match groupName with
                    | Some groupName ->

                        match data.Meta.Layers |> List.tryFind(fun x -> x.Name = groupName && x.BlendMode = None) with
                        | Some group -> 
                            let parentData, parentColor = groupData group.Group

                            (praseData group.Data) @ parentData,
                            match group.Color with
                            | Some c -> Some c
                            | _ -> parentColor

                        | _ ->
                            [], None

                    | _ ->
                        [], None


                let groupData, groupColor = groupData layer.Group

                let data = praseData layer.Data @ groupData
                let color = match layer.Color with Some c -> Some c | _ -> groupColor



                if color = Some "#d186dfff" || layer.BlendMode = None then
                    ()
                else
                    let json =
                        { Name = name
                          Slice = sliceName
                          Tag = tag
                          Offset = offset
                          Frame = frame
                          Options = data
                          Color = color
                          SortOrder = layerIndex }
                        
                    result.Add((inputFilename, json))

            | _ ->
                ()

        | _ ->
            ()

    result |> Map.ofSeq


let extractDecodeStore (path : string) (output : string) =
    let temp = output </> "Exported"

    let items = 
        if path.EndsWith(".aseprite") then
            [ path ]
        else
            Directory.EnumerateFiles(path, "*.aseprite", SearchOption.TopDirectoryOnly) |> Seq.toList

    for item in items do
        clearDirectory temp
        extract item temp
        let results = decode temp

        let files = Directory.EnumerateFiles(temp, "*.png", SearchOption.TopDirectoryOnly) |> Seq.toList
        for file in files do
            match results |> Map.tryFind file with
            | Some found ->
                let outputFilename = output </> Path.GetFileName(file)
                File.Move(file, outputFilename, true)

                let json = found |> Json.serialize
                File.WriteAllText(Path.ChangeExtension(outputFilename, "json"), json)

            | _ ->
                File.Delete file


        ()

let readSpriteFile path : SpriteData =
    loadJson path