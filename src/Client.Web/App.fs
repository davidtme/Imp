module Client.Web.App

open Imp.Setup
open Client.Common

let dataManager =
    WebDataManager(
        baseUrl = "Data")

let display  = 
    WebGLDisplay(
        elementId = "App", 
        width = 800,
        height = 600)

Views.init dataManager display
