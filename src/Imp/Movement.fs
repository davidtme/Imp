module Movement

open Imp

[<Struct>]
type private Rectangle = { X: float; Y: float; Width: float; Height: float }

[<Struct>]
type private Circle = { X: float; Y: float; Radius: float }

let private isOverlapping (rect: Rectangle) (circle: Circle) =
    // Calculate the center X coordinate of the rectangle
    let rectCenterX = rect.X + rect.Width / 2.0
    // Calculate the center Y coordinate of the rectangle
    let rectCenterY = rect.Y + rect.Height / 2.0
    // Calculate the horizontal distance between the circle's center and the rectangle's center
    let circleDistanceX = abs (circle.X - rectCenterX)
    // Calculate the vertical distance between the circle's center and the rectangle's center
    let circleDistanceY = abs (circle.Y - rectCenterY)

    // Check if the horizontal distance is greater than half the rectangle's width plus the circle's radius
    if circleDistanceX > (rect.Width / 2.0 + circle.Radius) then false
    // Check if the vertical distance is greater than half the rectangle's height plus the circle's radius
    elif circleDistanceY > (rect.Height / 2.0 + circle.Radius) then false
    // Check if the horizontal distance is less than or equal to half the rectangle's width
    elif circleDistanceX <= (rect.Width / 2.0) then true
    // Check if the vertical distance is less than or equal to half the rectangle's height
    elif circleDistanceY <= (rect.Height / 2.0) then true
    else
        // Calculate the squared distance from the circle's center to the rectangle's corner
        let cornerDistanceSq = (circleDistanceX - rect.Width / 2.0) ** 2.0 + (circleDistanceY - rect.Height / 2.0) ** 2.0
        // Check if the squared distance is less than or equal to the squared radius of the circle
        cornerDistanceSq <= (circle.Radius ** 2.0)

let private (|AngleRange|_|) (baseRange : float) (input : float) =
    let diff = abs ((baseRange + 360.0) - (input + 360.0))
    if diff < 15 then 
        Some ()
    else None


// Optimized movement function with time delta, tracking only the closest collision
let moveWithTimeDelta canMove (timeDelta: float) (start: PointF) (vector : Vector) (radius: float) =
    let (velocity: PointF) = vector |> Vector.toPoint

    let checkPoint velocity =
        if velocity = PointF.zero then
            ValueNone
        else
            let target = start |> PointF.add (PointF.multiply velocity timeDelta)
            let targetCircle = { X = target.X; Y = target.Y; Radius = radius }

            let minX = min (int (floor (start.X - radius))) (int (floor (target.X - radius)))
            let maxX = max (int (ceil (start.X + radius))) (int (ceil (target.X + radius)))
            let minY = min (int (floor (start.Y - radius))) (int (floor (target.Y - radius)))
            let maxY = max (int (ceil (start.Y + radius))) (int (ceil (target.Y + radius)))

            let mutable canMoveAll = true

            // Check for collisions in the movement path
            for x = minX to maxX do
                for y = minY to maxY do
                    // X
                    if canMoveAll && canMove { Point.X = x; Y = y } |> not then
                        let gridRectangle = { X = x;  Y = y; Width = 1; Height = 1 }
                        if isOverlapping gridRectangle targetCircle then
                            canMoveAll <- false

            if canMoveAll then
                ValueSome target
            else 
                ValueNone



    match checkPoint velocity with
    | ValueSome target ->
        target
    | _ -> 

        let mutable result = ValueNone

        let checkGrid x y =
            canMove { 
                Point.X = x
                Y = y  }

        let toGrid v = v |> floor |> int
        let x = start.X |> toGrid
        let y = start.Y |> toGrid

        let xo = (start.X - float x) |> cleanFloat
        let yo = (start.Y - float y) |> cleanFloat

        // Just X or Y
        if result.IsNone then
            match checkPoint { velocity with Y = 0 } with
            | ValueSome target -> 
                let v = Vector.fromPoints start target
                if v.Distance > (0.2 * timeDelta) then
                    result <- ValueSome target
            | _ ->
                match checkPoint { velocity with X = 0 } with
                | ValueSome target -> 
                    let v = Vector.fromPoints start target
                    if v.Distance > (0.2 * timeDelta) then
                        result <- ValueSome target
                | _ -> 
                    ()

        // Nudging when close the the corner
        if result.IsNone then       
            match vector.Angle with
            
            | AngleRange 90.0 ->
                // Right close to top
                if yo >= 0.5 && checkGrid (x+1) (y) then
                    match checkPoint { X = 0; Y = -1 } with
                    | ValueSome target -> 
                        result <- ValueSome target
                    | _ -> ()

                // Right close to bottom
                elif yo <= 0.5 && checkGrid (x+1) (y) then
                    match checkPoint { X = 0; Y = 1 } with
                    | ValueSome target -> 
                        result <- ValueSome target
                    | _ -> ()

            | AngleRange 270.0 ->
                // Left close to top
                if yo >= 0.5 && checkGrid (x-1) (y) then
                    match checkPoint { X = 0; Y = -1 } with
                    | ValueSome target -> 
                        result <- ValueSome target
                    | _ -> ()

                elif yo <= 0.5 && checkGrid (x-1) (y) then
                    // Left close to bottom
                    match checkPoint { X = 0; Y = 1 } with
                    | ValueSome target -> 
                        result <- ValueSome target
                    | _ -> ()

            | AngleRange 180.0 ->
                // Down close to left
                if xo >= 0.5 && checkGrid (x) (y+1) then
                    match checkPoint { X = -1; Y = 0 } with
                    | ValueSome target -> 
                        result <- ValueSome target
                    | _ -> ()

                // Down close to right
                elif xo <= 0.5 && checkGrid (x) (y+1) then
                    match checkPoint { X = 1; Y = 0 } with
                    | ValueSome target -> 
                        result <- ValueSome target
                    | _ -> ()

            | AngleRange 0.0 ->
                // Up close to left
                if xo >= 0.5 && checkGrid (x) (y-1) then
                    match checkPoint { X = -1; Y = 0 } with
                    | ValueSome target -> 
                        result <- ValueSome target
                    | _ -> ()

                // Up close to right
                elif xo <= 0.5 && checkGrid (x) (y-1) then
                    match checkPoint { X = 1; Y = 0 } with
                    | ValueSome target -> 
                        result <- ValueSome target
                    | _ -> ()

            | _ ->
                ()


        // Aggressive nudging
        if result.IsNone then       
            match vector.Angle with

            // Right Up
            | AngleRange 90.0 when yo < 0.4 && checkGrid (x) (y-1) && checkGrid (x+1) (y-1) ->
                match checkPoint { X = 0; Y = -1 } with
                | ValueSome target -> result <- ValueSome target
                | _ -> ()

            // Right Down
            | AngleRange 90.0 when yo > 0.6 && checkGrid (x) (y+1) && checkGrid (x+1) (y+1) ->
                match checkPoint { X = 0; Y = 1 } with
                | ValueSome target -> result <- ValueSome target
                | _ -> ()


            // Left Up
            | AngleRange 270.0 when yo < 0.4 && checkGrid (x) (y-1) && checkGrid (x-1) (y-1) ->
                match checkPoint { X = 0; Y = -1 } with
                | ValueSome target -> result <- ValueSome target
                | _ -> ()

            // Left Down
            | AngleRange 270.0 when yo > 0.6 && checkGrid (x) (y+1) && checkGrid (x-1) (y+1) ->
                match checkPoint { X = 0; Y = 1 } with
                | ValueSome target -> result <- ValueSome target
                | _ -> ()

            // Down left
            | AngleRange 180.0 when xo < 0.4 && checkGrid (x-1) (y) && checkGrid (x-1) (y+1) ->
                match checkPoint { X = -1; Y = 0 } with
                | ValueSome target -> result <- ValueSome target
                | _ -> ()

            // Down right
            | AngleRange 180.0 when xo > 0.6 && checkGrid (x+1) (y) && checkGrid (x+1) (y+1) ->
                match checkPoint { X = 1; Y = 0 } with
                | ValueSome target -> result <- ValueSome target
                | _ -> ()

            // Up left
            | AngleRange 0.0 when xo < 0.4 && checkGrid (x-1) (y) && checkGrid (x-1) (y-1) ->
                match checkPoint { X = -1; Y = 0 } with
                | ValueSome target -> result <- ValueSome target
                | _ -> ()

            // Up right
            | AngleRange 0.0 when xo > 0.6 && checkGrid (x+1) (y) && checkGrid (x+1) (y-1) ->
                match checkPoint { X = 1; Y = 0 } with
                | ValueSome target -> result <- ValueSome target
                | _ -> ()

            | _ -> ()




        match result with
        | ValueSome result ->
            result
        | _ ->
            start


let inline readControllerVector (dpad : ^a) (thumbstick : Vector) =
    let mutable playerVector = Vector.zero
    do 
        let mutable dpadPointY = 0.0
        let mutable dpadPointX = 0.0

        if (^a : (member Up : _) dpad) then
            dpadPointY <- dpadPointY - 1.0

        if (^a : (member Down : _) dpad) then
            dpadPointY <- dpadPointY + 1.0

        if (^a : (member Left : _) dpad) then
            dpadPointX <- dpadPointX - 1.0

        if (^a : (member Right : _) dpad) then
            dpadPointX <- dpadPointX + 1.0

        if dpadPointY <> 0.0 || dpadPointX <> 0.0 then
            playerVector <- Vector.fromPoints PointF.zero { X = dpadPointX; Y = dpadPointY } |> Vector.setDistance 1.0

    if thumbstick.Distance > 0.0 then
        playerVector <- thumbstick |> Vector.setDistanceWith (fun d -> min d 1.0)

    playerVector