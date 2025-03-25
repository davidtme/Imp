namespace Client.Android

open Silk.NET.Windowing.Sdl.Android
open Android.App
open Android.Content
open Android.Content.PM
open Imp.Setup
open Client.Common

type SampleReceiver() =
    inherit BroadcastReceiver()

    override _.OnReceive(context, intent) =
        ()

[<Activity(Label = "@string/app_name", MainLauncher = true, ScreenOrientation = ScreenOrientation.Landscape)>]
type MainActivity() =
    inherit SilkActivity()

    let sampleReceiver = new SampleReceiver()

    override _.OnCreate(savedInstanceState) =
        use intentFilter = new IntentFilter("com.companyname.AndroidDemo")
        base.BaseContext.RegisterReceiver(sampleReceiver, intentFilter, ReceiverFlags.Exported) |> ignore
        base.OnCreate(savedInstanceState)

    override _.OnResume() =
        base.OnResume()

    override _.OnPause() =
        base.OnPause()

    override this.OnRun() =
        this.Title <- "Demo"

        let dataManager = 
            ResourceDataManager(
                assembly = typeof<Models.DummyType>.Assembly,
                name = "Data")

        let display = AndroidOpenGLDisplay(ViewHelpers.tickRate)
        Views.init dataManager display
