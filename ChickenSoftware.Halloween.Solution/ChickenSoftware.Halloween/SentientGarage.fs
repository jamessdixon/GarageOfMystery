namespace ChickenSoftware.Halloween

open System
open Phidgets
open Phidgets.Events
open Microsoft.Kinect

type ColorDataReadyEventArgs(colorData:byte[], width:int, height:int, stride:int) = 
    inherit EventArgs()
    member this.ColorData = colorData
    member this.Width = width
    member this.Height = height
    member this.Stride = stride

type LightSensorChangeEventArgs(sensorIndex:int, lightAmount:int)=
    inherit EventArgs()
    member this.SensorIndex = sensorIndex
    member this.LightAmount = lightAmount

type SentientGarage() =
    let _servoController = new AdvancedServo()
    let _interfaceKit = new InterfaceKit()
    let _kinectSensor = KinectSensor.KinectSensors.[0]
    let imageChangeThreshold = 60

    let mutable _isServoControllerReady = false
    let mutable _isInterfaceKitReady = false
    let mutable previousImageBytes = Seq.init 0 byte
    
    let colorDataReady = new Event<_>()
    let lightSensorChange = new Event<_>()
    let imageChanged = new Event<_>()
    let skeletonChanged = new Event<_>()

    [<CLIEvent>]
    member this.ColorDataReady = colorDataReady.Publish
    [<CLIEvent>]
    member this.LightSensorChange = lightSensorChange.Publish
    [<CLIEvent>]
    member this.ImageChanged = imageChanged.Publish
    [<CLIEvent>]
    member this.SkeletonChanged = skeletonChanged.Publish

    member this.kinectSensor_ColorFrameReady(args: ColorImageFrameReadyEventArgs) =
        use colorFrame = args.OpenColorImageFrame()
        if not (colorFrame = null) then
            let colorData =  Array.zeroCreate<byte> colorFrame.PixelDataLength 
            colorFrame.CopyPixelDataTo(colorData)
            let width = colorFrame.Width
            let height = colorFrame.Height
            let stride = colorFrame.Width * colorFrame.BytesPerPixel
            let eventArgs = new ColorDataReadyEventArgs(colorData,width,height,stride)
            colorDataReady.Trigger(eventArgs)
            ()

    member this.KinectSensor_SkeletonFrameReady(args: SkeletonFrameReadyEventArgs) =
        use skeletonFrame = args.OpenSkeletonFrame()
        if not (skeletonFrame = null) then
            let skeletons = Array.zeroCreate<Skeleton> skeletonFrame.SkeletonArrayLength
            skeletonFrame.CopySkeletonDataTo(skeletons)
            let skeletons1 = skeletons |> Array.filter (fun s -> s.TrackingState = SkeletonTrackingState.Tracked)               
            if skeletons1.Length > 0 then
              skeletonChanged.Trigger(skeletons1.[0])
            ()
        ()

    member this.initializeKinect() =
        _kinectSensor.ColorStream.Enable()
        _kinectSensor.ColorFrameReady.Subscribe(this.kinectSensor_ColorFrameReady) |> ignore
        _kinectSensor.SkeletonStream.Enable();
        _kinectSensor.SkeletonFrameReady.Subscribe(this.KinectSensor_SkeletonFrameReady) |> ignore
        _kinectSensor.Start()
 
    member this.interfaceKit_Attached(args: Events.AttachEventArgs) =
        let _interfaceKit = args.Device :?> InterfaceKit
        _interfaceKit.sensors 
                        |> Seq.cast
                        |> Seq.map(fun s -> s :> InterfaceKitAnalogSensor)
                        |> Seq.map(fun s -> s.Sensitivity <- 20)
                        |>ignore
        _isInterfaceKitReady <- true

    member this.interfaceKit_SensorChange(e: SensorChangeEventArgs ) =
        let eventArgs = new LightSensorChangeEventArgs(e.Index,e.Value)
        lightSensorChange.Trigger(eventArgs)

    member this.initializeInterfaceKit() =
        _interfaceKit.Attach.Add(this.interfaceKit_Attached)
        _interfaceKit.SensorChange.Add(this.interfaceKit_SensorChange)
        _interfaceKit.``open``()
        _interfaceKit.waitForAttachment()

    member this.servoController_Attached(args:Events.AttachEventArgs) =
        let _servoController = args.Device :?> AdvancedServo
        _servoController.servos.[0].Engaged <- true 
        _servoController.servos.[0].Position <- 110. 
        _isServoControllerReady  <- true

    member this.initializeController() =
        _servoController.Attach.Add(this.servoController_Attached)
        _servoController.``open``()

    member this.moveController(position:float) =
        if _isServoControllerReady then
            _servoController.servos.[0].Position <- position 
