module Alchemists.Client.Desktop.Program

open Imp.Setup
open Client.Common

let dataManager = 
    ResourceDataManager(
        assembly = typeof<Models.DummyType>.Assembly,
        name = "Data")

let display =
    OpenGLDisplay(
        width = 800,
        height = 600,
        tickRate = ViewHelpers.tickRate,
        title = "Game")
    
Views.init dataManager display

exit 0