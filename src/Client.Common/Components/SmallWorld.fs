module Client.Common.Components.SmallWorld

open Imp
open Imp.Elmish
open Imp.SpriteHelpers
open Client.Common.ViewHelpers

type ButtonState =
    { mutable Up : bool
      mutable Down : bool 
      mutable Left : bool
      mutable Right : bool }

type private Controller = 
    { mutable Thumbstick : Vector
      Buttons : ButtonState }

type private TileType =
    | Gound
    | Wall

type private World = 
    { Grid : TileType [][]
      mutable UpdateCounter : int64 }
    member this.Updated() = this.UpdateCounter <- this.UpdateCounter + 1L

type private Player =
    { mutable Position : PointF
      mutable Angle : string
      mutable Tag : string
      mutable Frame : float
      mutable UpdateCounter : int64 }
    member this.Updated() = this.UpdateCounter <- this.UpdateCounter + 1L

type private Players =
    { Player1 : Player }

type private Model =
    { Controller : Controller
      World : World
      Players : Players
      IsActive : bool
      }

[<RequireQualifiedAccess>]
type private Message = 
    | Noop
    | Input of InputHandlerMessage
    | Focus
    | Blur
    | Tick

let private init () =
    let world = 
        Array.init 8 (fun y ->
            Array.init 8 (fun x ->
                TileType.Gound
            )
        )

    world.[1].[1] <- TileType.Wall
    world.[3].[1] <- TileType.Wall

    world.[1].[3] <- TileType.Wall
    world.[2].[3] <- TileType.Wall
    world.[3].[3] <- TileType.Wall
    world.[4].[3] <- TileType.Wall
    world.[5].[4] <- TileType.Wall
    world.[5].[5] <- TileType.Wall

    world.[5].[3] <- TileType.Wall
    world.[1].[5] <- TileType.Wall
    world.[2].[5] <- TileType.Wall
    world.[3].[5] <- TileType.Wall
    
    { IsActive = false
      World =
        { Grid = world
          UpdateCounter = 0L }

      Players = 
        { Player1 = 
            { Position = { X = 0.5; Y = 0.5 }
              Angle = "South"
              Tag = "Idle"
              Frame = 0.0
              UpdateCounter = 0L } }

      Controller =
          { Thumbstick = Vector.zero
            Buttons = 
                { Up = false
                  Down = false
                  Left = false
                  Right = false } } },
    Cmd.batch [
        Message.Tick |> Cmd.ofMsg
    ]

let private canMove (model : Model) (point : Point) = 
    match model.World.Grid |> Array.tryItem point.Y with
    | Some row ->
        match row |> Array.tryItem point.X with
        | Some tile ->  
            match tile with
            | TileType.Gound -> true
            | _ -> false

        | _ ->
            false

    | _ ->
        false


let private update (message : Message) (model : Model) =
    match message with
    | Message.Noop ->
        model,
        Cmd.none

    | Message.Tick ->
        let player = model.Players.Player1

        let controllerVector = Movement.readControllerVector model.Controller.Buttons model.Controller.Thumbstick

        if controllerVector.Distance > 0.0 then
            let timeDelta = (float tickRate / 200.0)

            player.Position <- Movement.moveWithTimeDelta (canMove model) timeDelta player.Position controllerVector 0.40

            player.Angle <-
                match (controllerVector |> Vector.roundToCardinal).Angle with
                | 0.0 -> "North"
                | 90.0 -> "East"
                | 180.0 -> "South"
                | _ -> "West"

            if controllerVector.Distance > 0.0 then
                player.Tag <- "Walk" // if controllerVector.Distance < 0.8 then "Walk" else "Run"
                player.Frame <- (player.Frame + (controllerVector.Distance * timeDelta * 4.0)) % 8.0
            else
                player.Tag <- "Idle"
                player.Frame <- 0.0

            player.Updated()

        elif player.Tag <> "Idle" then
            player.Tag <- "Idle"
            player.Frame <- 0.0
            player.Updated()
            
        
        model,
        Cmd.delay tickRate <| fun _ -> Message.Tick

    | Message.Input input ->
        match input with
        | InputHandlerMessage.Key (pressed, key) -> 
            match key with
            | Key.W
            | Key.Up -> model.Controller.Buttons.Up <- pressed
            | Key.A
            | Key.Left -> model.Controller.Buttons.Left <- pressed
            | Key.S
            | Key.Down -> model.Controller.Buttons.Down <- pressed
            | Key.D 
            | Key.Right -> model.Controller.Buttons.Right <- pressed
            | _ -> ()

        | InputHandlerMessage.ControllerThumbstick (x, y) ->
            model.Controller.Thumbstick <- Vector.fromPoints PointF.zero { X = x; Y = y }

        | InputHandlerMessage.ControllerButton (pressed, button) ->
            match button with
            | ControllerButton.DPadUp -> model.Controller.Buttons.Up <- pressed
            | ControllerButton.DPadLeft -> model.Controller.Buttons.Left <- pressed
            | ControllerButton.DPadDown -> model.Controller.Buttons.Down <- pressed
            | ControllerButton.DPadRight -> model.Controller.Buttons.Right <- pressed
            | ControllerButton.A
            | ControllerButton.B -> ()

        model,
        Cmd.none

    | Message.Focus ->
        let model = { model with IsActive = true }
        model.Players.Player1.Updated()

        model,
        Cmd.none

    | Message.Blur ->
        let model = { model with IsActive = false }

        let buttons = model.Controller.Buttons
        buttons.Up <- false
        buttons.Left <- false
        buttons.Down <- false
        buttons.Right <- false

        model.Controller.Thumbstick <- Vector.zero

        model,
        Cmd.none


let private useUpdateCounter (_, a) (_, b)  = a = b

let private renderPlayer (player : Player) =
    let playerDetails = ui.SpriteDetails $"Player-{player.Angle}-{player.Tag}"
    let frame = (int player.Frame) + 1

    view (
        x = int (player.Position.X * 32.0) - 16,
        y = int (player.Position.Y * 32.0) - 16,
        z = int ((player.Position.Y + 0.4) * 32.0)
        ) {
        singleRenderer () {
            renderSpriteFrame (playerDetails |> spriteFrame frame) (0,0)
        }
    }

let private renderWorld (world : World) =
    view () {
        singleRenderer () {
            for rowY = 0 to world.Grid.Length - 1 do
                view (z = rowY*32) {
                    let row = world.Grid.[rowY]
                    for rowX = 0 to row.Length - 1 do
                        let item = row.[rowX]
                        let name = 
                            match item with
                            | TileType.Wall -> "Wall"
                            | TileType.Gound -> "Earth"

                        ui.RenderSprite name 1 (rowX * 32, rowY * 32)
                }
        }
    }
    

let private rootView (model : Model) dispatch =

    view (
        x = 400,
        y = 100,
        z = 1,
        width = (model.World.Grid.Length * 32),
        height = (model.World.Grid.[0].Length * 32),
        mouseUp = (fun _ ->
            Message.Focus |> dispatch),
        blur = (fun _ ->
            Message.Blur |> dispatch)
        ) {
        context (fun ctx -> { ctx with ZRange = 0, 8*32 }) {
            component (key = "input", prop = model.IsActive) <| fun hooks (isActive) ->
                hooks.TriggerSyntheticEvent (fun _ -> SyntheticEvent.Up)
            
                if isActive then
                    hooks.HandleInput (Message.Input >> dispatch)

                view () {
                    batchRenderer () {
                        ui.Text() (if isActive then "Active" else "Click on the world to activate")
                    }
                }

            view (key = "world holder", y = 64) {
                component (key = "world", prop = (model.World, model.World.UpdateCounter), compare = useUpdateCounter) <| fun _ (world, _) ->
                    renderWorld world

                component (key = "player", prop = (model.Players.Player1, model.Players.Player1.UpdateCounter), compare = useUpdateCounter) <| fun _ (player, _) -> 
                    renderPlayer (player)
            }
        }
    }


let render key = reducer key init update rootView