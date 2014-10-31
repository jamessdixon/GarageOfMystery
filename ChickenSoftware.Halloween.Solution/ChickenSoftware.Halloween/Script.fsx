
#r @"C:\Program Files\Phidgets\Phidget21.NET.dll"

open Phidgets

let _servoController = new AdvancedServo()
let mutable _isServoControllerReady = false

let servoController_Attached(args:Events.AttachEventArgs) =
    let _servoController = args.Device :?> AdvancedServo
    _servoController.waitForAttachment()
    if(_servoController.Attached) then
        _servoController.servos.[0].Position = 30.00 |> ignore
    _isServoControllerReady  <- true

let initializeController() =
    _servoController.Attach.Add(servoController_Attached)
    _servoController.``open``()

let moveController(position:float) =
    if _isServoControllerReady then
        _servoController.servos.[0].Position = position |> ignore
    

initializeController()
moveController(120.) //Throws an exception

#r @"C:\Program Files\Phidgets\Phidget21.NET.dll"

open System
open Phidgets
open Phidgets.Events

let _interfaceKit = new InterfaceKit()
let mutable _isInterfaceKitReady = false

let interfaceKit_Attached(args: Events.AttachEventArgs) =
    let _interfaceKit = args.Device :?> InterfaceKit
    _interfaceKit.sensors 
                    |> Seq.cast
                    |> Seq.map(fun s -> s :> InterfaceKitAnalogSensor)
                    |> Seq.map(fun s -> s.Sensitivity <- 20)
                    |>ignore
    _isInterfaceKitReady <- true

let interfaceKit_SensorChange(e: SensorChangeEventArgs ) =
    printfn "%A %A"(e.Index,e.Value) |> ignore
    ()

let initializeInterfaceKit() =
    _interfaceKit.Attach.Add(interfaceKit_Attached)
    _interfaceKit.SensorChange.Add(interfaceKit_SensorChange)
    _interfaceKit.``open``()
    _interfaceKit.waitForAttachment(5000)

initializeInterfaceKit()


#r @"C:\Program Files\Microsoft SDKs\Kinect\v1.7\Assemblies\Microsoft.Kinect.dll"

open Microsoft.Kinect

let _kinectSensor = KinectSensor.KinectSensors.[0]

let kinectSensor_ColorFrameReady(args: ColorImageFrameReadyEventArgs) =
    use colorFrame = args.OpenColorImageFrame()
    if not (colorFrame = null) then
        let colorData =  Array.zeroCreate<byte> colorFrame.PixelDataLength  
        colorFrame.CopyPixelDataTo(colorData)
        //RaiseEvent with color frame as the event args

let initializeKinect() =
    _kinectSensor.ColorStream.Enable()
    _kinectSensor.ColorFrameReady.Subscribe(kinectSensor_ColorFrameReady) |> ignore
    _kinectSensor.Start()

initializeKinect()








