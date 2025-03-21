module Client.Common.Components.DemoWindow

open Imp
open Client.Common.ViewHelpers
open Imp.SpriteHelpers

let render key = 
    component (key = key) <| fun hooks ->
        ui.Window (x = 10, y = 150, width = 300, height = 200) <| fun _ -> 
            view (x = 10, y = 10) {
                let clicked = hooks.UseState false

                ui.Button (
                    label = "Click",
                    width = 80,
                    mouseUp = (fun _ -> clicked.Update(fun x -> not x))
                )

                if clicked.Current then
                    ui.Text (y = 80) "ON"
            }

type TextInputState =
    { Value : string
      Position : int
      Active : bool
      Shift : bool
      }

let textInput (key) =
    component (key = key) <| fun hooks ->
        let state =
            hooks.UseStateLazy <| fun _ -> 
                let v = "SOME TEXT"

                { Value = "SOME TEXT"
                  Position = v.Length
                  Active = false
                  Shift = false
                  }

        let s = state.Current

        if s.Active then
            hooks.HandleInput (fun m -> 
                match m with
                | InputHandlerMessage.Key (pressed, key) ->
                    match key with
                    | Key.Left ->
                        if pressed then
                            state.Update(fun s -> { s with Position = max 0 (s.Position - 1) })

                    | Key.Right ->
                        if pressed then
                            state.Update(fun s -> { s with Position = min (s.Value.Length) (s.Position + 1) })

                    | Key.Backspace ->
                        if pressed then
                            if s.Position > 0 then
                                let a = s.Value.Substring(0, s.Position - 1)
                                let c = s.Value.Substring(s.Position)
                                state.Update(fun s -> { s with Value = a+c; Position = s.Position - 1 })

                    | Key.Delete ->
                        if pressed then
                            if s.Position < s.Value.Length then
                                let a = s.Value.Substring(0, s.Position)
                                let c = s.Value.Substring(s.Position + 1)
                                state.Update(fun s -> { s with Value = a+c })

                    | Key.ShiftLeft
                    | Key.ShiftRight -> 
                        state.Update(fun s -> { s with Shift = pressed })
      
                    | _ -> 
                        if pressed then
                            let letter (normal) (shifted) =
                                let x = if s.Shift then shifted else normal

                                let p = s.Position
                                let v = 
                                    let a = s.Value.Substring(0, p)
                                    let c = s.Value.Substring(p)
                                    a + x + c

                                state.Update(fun s -> { s with Value = v; Position = p + 1 })

                            match key with
                            | Key.Space -> letter " " " "

                            | Key.Number1 -> letter "1" "!"
                            | Key.Number2 -> letter "2" "\""
                            | Key.Number3 -> letter "3" "£"
                            | Key.Number4 -> letter "4" "$"
                            | Key.Number5 -> letter "5" "%"
                            | Key.Number6 -> letter "6" "^"
                            | Key.Number7 -> letter "7" "&"
                            | Key.Number8 -> letter "8" "*"
                            | Key.Number9 -> letter "9" "("
                            | Key.Number0 -> letter "0" ")"

                            | Key.A -> letter "a" "A"
                            | Key.B -> letter "b" "B"
                            | Key.C -> letter "c" "C"
                            | Key.D -> letter "d" "D"
                            | Key.E -> letter "e" "E"
                            | Key.F -> letter "f" "F"
                            | Key.G -> letter "g" "G"
                            | Key.H -> letter "h" "H"
                            | Key.I -> letter "i" "I"
                            | Key.J -> letter "j" "J"
                            | Key.K -> letter "k" "K"
                            | Key.L -> letter "l" "L"
                            | Key.M -> letter "m" "M"
                            | Key.N -> letter "n" "N"
                            | Key.O -> letter "o" "O"
                            | Key.P -> letter "p" "P"
                            | Key.Q -> letter "q" "Q"
                            | Key.R -> letter "r" "R"
                            | Key.S -> letter "s" "S"
                            | Key.T -> letter "t" "T"
                            | Key.U -> letter "u" "U"
                            | Key.V -> letter "v" "V"
                            | Key.W -> letter "w" "W"
                            | Key.X -> letter "x" "X"
                            | Key.Y -> letter "y" "Y"
                            | Key.Z -> letter "z" "Z"
                            

                            | Key.Slash -> letter "/" "?"
                            | Key.Period -> letter "." ">"
                            | Key.Comma -> letter "," "<"
                            | Key.Apostrophe -> letter "'" "@"
                            | Key.Semicolon -> letter ";" ":"

                            | _ -> ()


                | _ ->
                    ()
            )

        let fontName = ui.DefaultFont
        let x = 0
        let y = 0
        let mutable offset = 0

        view (
            x = 0,
            y = 400,
            width = 100,
            height = 100,
            mouseUp = (fun _ -> 
                state.Update(fun s -> { s with Active = true })
            ),
            blur = (fun _ -> 
                state.Update(fun s -> { s with Active = false })
            )
            ) {
                for i = 0 to s.Value.Length - 1 do
                    let c = s.Value.Chars i
                    if c = ' ' then
                        offset <- offset + 4
                    else
                        let c =
                            match c with
                            | ':' -> ValueNone
                            | '-' -> ValueSome "Minus"
                            | '@' -> ValueSome "At"
                            | '?' -> ValueSome "Question Mark"
                            | '\"' -> ValueSome "Double Quote"
                            | '.' -> ValueSome "Full Stop"
                            | ',' -> ValueSome "Comma"
                            | x -> x.ToString().ToUpperInvariant() |> ValueSome

                        match c with 
                        | ValueSome c ->
                            match ui.Sprites.TryGetValue $"%s{fontName}-{c}" with
                            | true, s ->
                                let frame =  s  |> spriteFrame 1
                                renderSpriteFrame frame (x + offset, y)
                                offset <- offset + frame.Layers.[0].Rect.Width + 1

                            | _ -> 
                                ()

                        | _ -> ()

                    if s.Active && i = s.Position - 1 then
                        let frame =  ui.SpriteDetails $"%s{fontName}-Caret" |> spriteFrame 1
                        renderSpriteFrame frame (x + offset - 2, y)
                        
        }