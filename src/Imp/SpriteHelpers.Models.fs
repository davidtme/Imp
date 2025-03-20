[<AutoOpen>]
module Imp.SpriteHelpers.Models

open Imp

type Layer<'T> = 
    { Texture : string
      Rect : Rect
      Offset : Point
      MetaData : 'T }

type Frame<'T> =
    { Layers : Layer<'T> [] }

type SpriteInfo<'T> = 
    { Name : string
      Frames : Frame<'T> [] }

