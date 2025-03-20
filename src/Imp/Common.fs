[<AutoOpen>]
module Imp.Common

let private maxPoolSize = 10_000

type Pool<'a,'b>(create : 'b -> 'a, clean : 'a -> unit) =
    let items = System.Collections.Generic.Stack<'a>()
    let mutable itemOut = 0
    let mutable total = 0
    member _.Borrow init =
        if total > maxPoolSize then
            invalidOp "Memory leak?"

        itemOut <- itemOut + 1

        if items.Count > 0 then
            items.Pop()
        else
            total <- total + 1
            create init

    member _.Return item = 
        itemOut <- itemOut - 1
        clean item
        items.Push(item)

type Pool<'a>(create : unit -> 'a, clean : 'a -> unit) =
    let items = System.Collections.Generic.Stack<'a>()
    let mutable itemOut = 0
    let mutable total = 0
    member _.Borrow () =
        if total > 1000 then
            invalidOp "Memory leak?"

        itemOut <- itemOut + 1
        if items.Count > 0 then
            items.Pop()
        else
            total <- total + 1
            create ()

    member _.Return item = 
        itemOut <- itemOut - 1
        clean item
        items.Push(item)
