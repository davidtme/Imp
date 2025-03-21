module Imp.Tools.Pipeline

open Argu
open Imp.Tools.Common
open System.IO
open Imp.SpriteHelpers

type SpritePipelineArguments =
    | Input of string
    | File_Output of string
    | Resx_Output of string
    | Temp of string
    interface IArgParserTemplate with
        member _.Usage = ""

let spriteCliPipeline<'a> argv pipeline meta =
    let parser = ArgumentParser.Create<SpritePipelineArguments>()
    let results = parser.Parse argv

    let temp = results.GetResult(Temp) |> rootPath
    let fileOutput = results.GetResult(File_Output) |> rootPath
    let resxOutput = results.GetResult(Resx_Output) |> rootPath

    if fileOutput.EndsWith(".aseprite") |> not then
        clearDirectory temp

    let toPack = temp </> "ToPack"

    do 
        let working = temp </> "Working"

        ensureDirectory working
        clearDirectory working
            
        do
            let input = results.GetResult(Input) |> rootPath
            Aseprite.extractDecodeStore input working

        Images.iterSprites working <| fun img sprite ->
            pipeline img sprite

        ensureDirectory toPack

        let images = Directory.EnumerateFiles(working, "*.*", SearchOption.TopDirectoryOnly) |> Seq.toList
        for image in images do
            match Path.GetExtension(image) with
            | ".json"
            | ".png" ->
                File.Move(image, toPack </> Path.GetFileName(image), true)
            | x -> ()

        clearDirectory working

    ensureDirectory fileOutput
    clearDirectory fileOutput
    TexturePacker.pack toPack fileOutput "mega"
    let megaInfo = TexturePacker.parseAtlasFile (fileOutput </> "Mega.atlas")
    File.Delete (fileOutput </> "Mega.atlas")

    let sprites : Models.SpriteInfo<'a> list =
        [ for imageFile in megaInfo do
            for frame in imageFile.Items do
                let sprite = Aseprite.readSpriteFile (toPack </> $"{frame.Name}.json")

                let key = 
                    match sprite.Tag with
                    | Some tag ->
                        $"{sprite.Slice}-{tag}"
                    | _ ->
                        sprite.Slice

                let layer = 
                    { Texture = Path.GetFileNameWithoutExtension(imageFile.Name)
                      Rect = 
                        { X = frame.X
                          Y = frame.Y
                          Width = frame.Width
                          Height = frame.Height }
                      Offset =
                        { X = fst sprite.Offset
                          Y = snd sprite.Offset
                        }
                      MetaData = meta sprite.Options
                    }

                key, (sprite.Frame, (sprite.SortOrder, layer))
        ]
        |> List.groupBy(fst)
        |> List.map(fun (key, frameLayers) ->
            let frames =
                frameLayers 
                |> List.map snd
                |> List.groupBy fst
                |> List.map(fun (frame, layers) -> frame, layers |> List.map snd)
                |> List.sortBy fst
                |> List.map (fun (_, layers) -> 
                    { Layers = 
                        layers
                        |> List.sortBy fst
                        |> List.map snd
                        |> List.toArray })
                |> List.toArray

            { Name = key
              Frames = frames }
        )

    sprites |> saveJson (fileOutput </> "sprites.json")

    do
        let files = 
            Directory.EnumerateFiles(fileOutput)
            |> Seq.map(fun path ->
                Path.GetFileName(path), path
            )

        files |> Imp.Tools.Resources.save resxOutput