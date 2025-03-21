module Client.Tools

open Imp.Tools
open Client.Common

[<EntryPoint>]
let main (argv : string []) =
    let v = System.Environment.CurrentDirectory
    let x = v

    if argv.[0] = "Sprites" then
        Pipeline.spriteCliPipeline<Models.SpriteMetaData>
            argv.[1..]
            Images.trim
            (fun _ -> "")
    0