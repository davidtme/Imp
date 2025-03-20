[<AutoOpen>]
module Imp.CommonModels

open System

[<Struct>]
type Point3 = 
    { X : int 
      Y : int
      Z : int }

[<Struct>]
type Point =
    { X : int
      Y : int }

[<Struct>]
type Size =
    { Width : int
      Height : int }

[<Struct>]
type Rect =
    { X : int
      Y : int
      Width : int
      Height : int }

    member this.Inside(x,y) =
        x >= this.X && y >= this.Y &&
        x < this.X + this.Width && y < this.Y + this.Height

let private toDegrees (radians : float) =
    radians * 180.0 / Math.PI;

let cleanFloat (f : float) = Math.Round(f, 8)

type [<Struct>] Vector =
    { Angle : float
      Distance : float }

    static member zero = { Angle = 0.0; Distance = 0.0 }

    static member fromPoints (p1 : PointF) (p2 : PointF) =
        let x = p2.X - p1.X 
        let y = p2.Y - p1.Y

        let angle = (toDegrees(Math.Atan2(y, x)) + 450.0) % 360.0
        let distance = Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2))

        { Angle = Math.Round(angle, 8)
          Distance = Math.Round(distance, 8) }

    static member toPoint(v : Vector) =
        let r = Math.PI * (v.Angle / 180.0);
        let x = v.Distance * sin(r)
        let y = 0.0 - (v.Distance * cos(r))

        { X = if abs x < 0.0000000000001 then 0.0 else x
          Y = if abs y < 0.0000000000001 then 0.0 else y }

    static member inline setDistance distance (v : Vector) = { v with Distance = distance }
    static member inline setDistanceWith fn (v : Vector) = { v with Distance = fn v.Distance}

    static member rotate r (v : Vector) = 
        { v with
            Angle =
                let a = v.Angle + r
                if a < 0 then 
                    360.0 + a
                elif a >= 360 then 
                    a - 360.0
                else 
                    a }

    static member roundToCardinal (v : Vector) =
        { v with
            Angle =
                if (v.Angle > 315.0 && v.Angle <= 360.0) || (v.Angle >= 0.0 && v.Angle < 45.0) then
                    0.0
                 elif v.Angle >= 45.0 && v.Angle <= 135.0 then
                    90.0
                 elif v.Angle > 135.0 && v.Angle < 225.0 then
                    180.0
                 else
                    270.0 }

and PointF =
    { X : float
      Y : float }
    static member inline create (x : float, y : float) = { X = x; Y = y }
    static member inline createFromInt (x : int, y : int) = { X = float x; Y = float y }
    static member inline x (p : PointF) = p.X
    static member inline y (p : PointF) = p.Y
    static member inline deconstruct (p : PointF) = struct(p.X, p.Y)
    static member clean (p : PointF) = { X = Math.Round(p.X, 8); Y = Math.Round(p.Y, 8) }
    static member grid (p : PointF) = { X = round p.X; Y = round p.Y }
    static member add (p2 : PointF) (p1 : PointF) = { X = p1.X + p2.X; Y = p1.Y + p2.Y }
    static member addX (x : float) (p1 : PointF) = { X = p1.X + x; Y = p1.Y }
    static member addY (y : float) (p1 : PointF) = { X = p1.X; Y = p1.Y + y }
    static member addVector v p = p |> PointF.add (v |> Vector.toPoint)
    static member round (p : PointF) = { X = cleanFloat p.X; Y = cleanFloat p.Y }
    static member subtract (a: PointF) (b: PointF) = { X = a.X - b.X; Y = a.Y - b.Y }
    static member multiply (a: PointF) (scalar: float) = { X = a.X * scalar; Y = a.Y * scalar }
    static member zero = { X = 0.0; Y = 0.0 }

    static member normalize (v: PointF) =
        let length = Math.Sqrt(v.X * v.X + v.Y * v.Y)
        if length > 0.0 then { X = v.X / length; Y = v.Y / length }
        else { X = 0.0; Y = 0.0 }

    //static member damper (target : PointF) (halflife : float) dt (point : PointF) =
    //    { X = damper point.X target.X halflife dt
    //      Y = damper point.Y target.Y halflife dt }