module Json

open System

#if FABLE_COMPILER
open Thoth.Json
#else
open Thoth.Json.Net
#endif

let caseStrategy = CaseStrategy.CamelCase

//let withResizeArray<'a> extra =   
//    let preEncoder = Encode.Auto.generateEncoderCached<'a>(caseStrategy, extra = extra)
//    let preDecoder = Decode.Auto.generateDecoderCached<'a>(caseStrategy, extra = extra)

//    let encoder (item : System.Collections.Generic.List<'a>) =
//        item |> Seq.map(fun x -> preEncoder x) |> Seq.toArray |> Encode.array

//    let decoder : Decoder<_> = fun name token ->
//        match Decode.array preDecoder name token with
//        | Ok arr -> ResizeArray(arr) |> Ok
//        | Error result -> Error result

//    Extra.withCustom encoder decoder

let all = 
    let extraCoders = 
        Extra.empty
        |> Extra.withInt64

    //let extraCoders = extraCoders |> withResizeArray<string*Client.Messages.PropertyValue> extraCoders
    //let extraCoders = extraCoders |> withResizeArray<Client.Messages.MapPointItem> extraCoders

    extraCoders

let inline encoder<'T> = Encode.Auto.generateEncoderCached<'T>(caseStrategy, extra = all)
let inline decoder<'T> = Decode.Auto.generateDecoderCached<'T>(caseStrategy, extra = all)

let inline serialize (value : 'a) =
    encoder value |> Encode.toString 4

let inline deserialize (json) : 'a = 
    json |> Decode.unsafeFromString decoder