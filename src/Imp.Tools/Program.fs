module Program

open Argu
open System
open System.IO
open Imp.Tools.Common
//open Models

type Mode =
    | Shaders
    //| Sprites

type CliArguments =
    | Mode of Mode
    | Input of string
    | Output of string
    | Module_Name of string
    | Watch

    interface IArgParserTemplate with
        member _.Usage = ""



let watchHandler convert =
    let mail = 
        MailboxProcessor.Start(fun inbox ->
            let rec loop () =
                async { 
                    do! inbox.Receive()
                    convert()
                    return! loop() 
                }
            loop ())

    fun () -> mail.Post ()




[<EntryPoint>]
let main (argv : string []) =

    let parser = ArgumentParser.Create<CliArguments>()
    let results = parser.Parse argv

    let mode = results.GetResult(Mode)
    match mode with
    | Mode.Shaders ->
        let config = results.GetResult(Input) |> rootPath
        let output = results.GetResult(Output) |> rootPath
        let moduleName = results.GetResult(Module_Name)

        Tools.Shaders.convert config moduleName output 

        if results.Contains Watch then
            use watcher = new FileSystemWatcher(config)
            let watchHandler = watchHandler (fun _ -> Tools.Shaders.convert config output moduleName)

            watcher.Changed.Add(fun _ -> watchHandler())
            watcher.Deleted.Add(fun _ -> watchHandler())
            watcher.Created.Add(fun _ -> watchHandler())

            watcher.EnableRaisingEvents <- true

            Console.ReadKey() |> ignore

    //| Mode.Sprites ->
    //    let output = results.GetResult(Output) |> rootPath
    //    let toPack = Path.GetDirectoryName(output) </> "ToPack"

    //    do 
    //        let working = Path.GetDirectoryName(output) </> "Working"

    //        ensureDirectory working
    //        clearDirectory working
            
    //        do
    //            let input = results.GetResult(Input) |> rootPath
    //            Aseprite.extractDecodeStore input working

    //        //Images.expand output
    //        Images.trim working

    //        ensureDirectory toPack

    //        let images = Directory.EnumerateFiles(working, "*.*", SearchOption.TopDirectoryOnly) |> Seq.toList
    //        for image in images do
    //            match Path.GetExtension(image) with
    //            | ".json"
    //            | ".png" ->
    //                File.Move(image, toPack </> Path.GetFileName(image), true)
    //            | x -> ()

    //        clearDirectory working



    //    ensureDirectory output
    //    clearDirectory output
    //    TexturePacker.pack toPack output "Mega"
    //    let megaInfo = TexturePacker.parseAtlasFile (output </> "Mega.atlas")

    //    let sprites =
    //        [ for imageFile in megaInfo do
    //            for frame in imageFile.Items do
    //                let sprite = Aseprite.readSpriteFile (toPack </> $"{frame.Name}.json")

    //                let key = 
    //                    match sprite.Tag with
    //                    | Some tag ->
    //                        $"{sprite.Slice}-{tag}"
    //                    | _ ->
    //                        sprite.Slice

    //                let layer = 
    //                    { Texture = Path.GetFileNameWithoutExtension(imageFile.Name)
    //                      Rect = 
    //                        { X = frame.X
    //                          Y = frame.Y
    //                          Width = frame.Width
    //                          Height = frame.Height }
    //                      Offset =
    //                        { X = fst sprite.Offset
    //                          Y = snd sprite.Offset
    //                        }
    //                    }

    //                key, (sprite.Frame, (sprite.SortOrder, layer))
    //        ]
    //        |> List.groupBy(fst)
    //        |> List.map(fun (key, frameLayers) ->
    //            let frames =
    //                frameLayers 
    //                |> List.map snd
    //                |> List.groupBy fst
    //                |> List.map(fun (frame, layers) -> frame, layers |> List.map snd)
    //                |> List.sortBy fst
    //                |> List.map (fun (_, layers) -> 
    //                    { Layers = 
    //                        layers
    //                        |> List.sortBy fst
    //                        |> List.map snd
    //                        |> List.toArray })
    //                |> List.toArray

    //            { Name = key
    //              Frames = frames }
    //        )
    //        |> List.toArray

    //    let root = { Sprites = sprites }

    //    root |> saveJson @"C:\dev\Alchemists\src\Test\Output\sprites.json" 

    //    ()

    0