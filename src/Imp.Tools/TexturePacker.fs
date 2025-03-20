module TexturePacker

open System
open System.IO
open Imp.Tools.Common

let mutable texturePackerPath = @"C:\dev\Game2023\Tools\runnable-texturepacker.jar"

module Data = 
    //type Bounds =
    //    { X : int
    //      Y : int
    //      W : int
    //      H : int }

    //type Frame = 
    //    { Filename : string
    //      Frame : Bounds }

    //type Root = 
    //    { Frames : Frame list }

    type ItemItem =
        { Name : string
          mutable X : int
          mutable Y : int
          mutable Width : int
          mutable Height : int
        }

    type ImageFile =
        { Name : string
          Items : ResizeArray<ItemItem>
        }

// https://libgdx.com/wiki/tools/texture-packer

let pack input output filename =
    ensureDirectory output


    File.WriteAllText(input </> "pack.json", """{
	"rotation": false,
	"minWidth": 1024,
	"minHeight": 1024,
	"maxWidth": 1024,
	"maxHeight": 1024,
	"paddingX": 1,
	"paddingY": 1
}""")


    let arg = [
        "-cp"; Path.GetFileName(texturePackerPath); "com.badlogic.gdx.tools.texturepacker.TexturePacker" 
        escapeFilename input
        escapeFilename output
        filename ] |> String.concat " "

    let startInfo = Diagnostics.ProcessStartInfo("java", arg )
    startInfo.WorkingDirectory <- Path.GetDirectoryName(texturePackerPath)

    let proc = Diagnostics.Process.Start(startInfo)
    proc.WaitForExit()

open Data

let parseAtlasFile path =
    let lines = File.ReadAllLines(path)

    let imageFiles = ResizeArray<ImageFile>()

    for line in lines do
        if line.EndsWith(".png") then
            imageFiles.Add(
                { Name = line
                  Items = ResizeArray()
                }
            )

        elif String.IsNullOrWhiteSpace(line) |> not then
            let imageFile = imageFiles.[imageFiles.Count - 1]
            
            if line.StartsWith("  ") then
                let line = line.TrimStart()
                let item = imageFile.Items.[imageFile.Items.Count - 1]

                if line.StartsWith("xy: ") then
                    match line.Substring(3).Split(", ") with
                    | [| x; y |] ->
                        item.X <- Int32.Parse(x)
                        item.Y <- Int32.Parse(y)

                        ()

                    | _ ->
                        ()

                elif line.StartsWith("size: ") then
                    match line.Substring(6).Split(", ") with
                    | [| w; h |] ->
                        item.Width <- Int32.Parse(w)
                        item.Height <- Int32.Parse(h)

                        ()

                    | _ ->
                        ()

                else
                    ()

            elif line.Contains(": ") then
                ()

            
            else
                let item = 
                    { Name = line
                      X = 0
                      Y = 0
                      Width = 0
                      Height = 0
                      }

                imageFile.Items.Add(item)

    imageFiles |> Seq.toList