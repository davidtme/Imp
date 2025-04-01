module Imp.Tools.Common

open System
open System.IO

let (</>) a b = sprintf "%s" (Uri(Path.Combine(a, b)).LocalPath.Trim('/', '\\'))

let absolutePathToRelative (absolute : string) (root : string) = 
    let absoluteUri = Uri(absolute)
    let rootUri = Uri(root + "\\")
    let result = rootUri.MakeRelativeUri(absoluteUri).ToString().Replace("/", "\\")
    result

let saveIfChanged (filename : string) text =
    let changed =
        if IO.File.Exists(filename) |> not then
            true
        else
            let current = File.ReadAllText(filename)
            current <> text

    if changed then
        printfn "Writing %s" filename
        IO.File.WriteAllText(filename, text)     
        true
    else
        ()
        //printfn "Skipping %s" filename
        false

let escapeFilename filename = sprintf "\"%s\"" (Uri(filename).LocalPath.Trim('/', '\\'))

let startProcess (path : string) arguments = 
    let exeFolder = Path.GetDirectoryName(path)

    let startInfo = Diagnostics.ProcessStartInfo(path, arguments |> String.concat " ")
    startInfo.WorkingDirectory <- exeFolder

    let proc = Diagnostics.Process.Start(startInfo)
    proc.WaitForExit()

let ensureDirectory path = 
    Directory.CreateDirectory(path) |> ignore

let rec clearDirectory path = 
    let di = DirectoryInfo(path)

    if di.Exists then
        di.EnumerateFiles()
        |> Seq.iter(fun f ->
            try
                if File.Exists(f.FullName) then
                    File.Delete(f.FullName)
            with _ -> ()
        )

        di.EnumerateDirectories ()
        |> Seq.iter(fun d ->
            clearDirectory d.FullName
        )

    try
        if Directory.Exists(path) then
            Directory.Delete(path)
    with _ -> ()

let loadJson<'a> filename : 'a =
    let jsonString = File.ReadAllText filename
    Json.deserialize jsonString

let saveJson (filename : string) content =
    ensureDirectory (Path.GetDirectoryName(filename))

    let jsonString = Json.serialize content
    File.WriteAllText(filename, jsonString)

let hashBytes (bytes : byte[]) =
    use hasher = System.Security.Cryptography.MD5.Create()
    hasher.ComputeHash(bytes)
    |> Array.map (sprintf "%02X")
    |> String.concat ""

let rootPath (path : string) = 
    if path.Contains(@":\") then path
    else Environment.CurrentDirectory </> path