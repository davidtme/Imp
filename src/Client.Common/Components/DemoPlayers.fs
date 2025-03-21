module Client.Common.Components.DemoPlayers

open Imp
open Imp.SpriteHelpers
open Client.Common.ViewHelpers

let items =
    [ "Player-North-Walk"
      "Player-West-Walk" 
      "Player-South-Walk" 
      "Player-East-Walk" 
              
      "Player-North-Run"
      "Player-West-Run" 
      "Player-South-Run" 
      "Player-East-Run"  ]

let render key = 
    view (key = key) {

    for i = 0 to items.Length - 1 do
        let item = items.[i]

        component (key = $"{item}", prop = ui.SpriteDetails item) <| fun hooks playerDetails ->
            let currentFrame = hooks.UseState(1)

            hooks.UseEffect() <| fun _ ->
                let rec loop() =
                    hooks.RegisterDelay 100 <| fun _ ->

                        currentFrame.Update(fun currentFrame ->
                            if currentFrame + 1 > playerDetails.Frames.Length then
                                1
                            else
                                currentFrame + 1
                        )
                        loop()
                loop()

            view (x=40+(96 * i),y=500) {
                renderSpriteFrame (playerDetails |> spriteFrame currentFrame.Current) (0,0)
            }
    }