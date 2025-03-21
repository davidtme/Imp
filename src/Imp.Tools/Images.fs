module Images

open System.IO
open System.Drawing
open Imp.Tools.Common

let expand output =    
    let amountX = 0
    let amountY = 20

    let files = Directory.EnumerateFiles(output, "*.png", SearchOption.TopDirectoryOnly) |> Seq.toList
    for file in files do
        let jsonFile = Path.ChangeExtension(file, "json")
        if File.Exists(jsonFile) then
            //log.Debug("expand {file}", file)
    
            let data : Aseprite.SpriteData = 
                File.ReadAllText(jsonFile)
                |> Json.deserialize

            use inputImage = 
                use stream = new FileStream(file, FileMode.Open)
                new Bitmap(stream)

            use outputImage = new Bitmap(inputImage.Width + (amountX * 2), inputImage.Height + (amountY * 2))
            do
                use g = Graphics.FromImage(outputImage)
                g.DrawImageUnscaled(inputImage, amountX, amountY)

            outputImage.Save(file)

            { data with
                Offset = fst data.Offset - amountX, snd data.Offset - amountY
            } |> saveJson jsonFile
           
open System.Drawing.Imaging
open Argu

let trim file jsonFile =
    let bytes = File.ReadAllBytes(file)
    let croppedBytes, minX, minY =
        use image = 
            use stream = new MemoryStream(bytes)
            new Bitmap(stream)

        let mutable minX = image.Width - 1
        let mutable maxX = 0

        let mutable minY = image.Height - 1
        let mutable maxY = 0
            
        for y = 0 to image.Height - 1 do
            for x = 0 to image.Width - 1 do
                let pixel = image.GetPixel(x, y)
                if pixel.A > byte 0 then
                    minX <- min minX x
                    maxX <- max maxX x
                    minY <- min minY y
                    maxY <- max maxY y

        let rect = Rectangle(minX, minY, (maxX - minX) + 1, (maxY - minY) + 1)

        use copy = image.Clone(rect, image.PixelFormat)
        use stream = new MemoryStream()
        copy.Save(stream, ImageFormat.Png)

        let value = stream.ToArray(), minX, minY
        value

    let data : Aseprite.SpriteData = 
        File.ReadAllText(jsonFile)
        |> Json.deserialize

    let data = 
        { data with 
            Offset =
                fst data.Offset + minX, snd data.Offset + minY
        }

    File.WriteAllBytes(file, croppedBytes)
    File.WriteAllText(jsonFile, data |> Json.serialize)

let nothing file jsonFile =
    ignore()

let iterSprites search fn =
    let files = Directory.EnumerateFiles(search, "*.png", SearchOption.TopDirectoryOnly)

    for file in files do
        let jsonFile = Path.ChangeExtension(file, "json")
        if File.Exists(jsonFile) then
            fn file jsonFile

