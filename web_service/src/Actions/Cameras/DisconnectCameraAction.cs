/***
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using WebService.Entites;

namespace WebService.Actions.Cameras;

public class DisconnectCameraAction : IAction
{
    Camera camera;

    public DisconnectCameraAction(Camera camera)
    {
        this.camera = camera;
    }

    public void Run()
    {
        //System.Console.WriteLine($"Debug: DisconnectCameraAction - state: {camera.State}, enabled: {camera.Enabled}");

        if (camera.State == Camera.DeviceState.Active)
        {
            camera.SetStatus("Stopping streamer...");
            camera.cameraStreamer!.Stop();
            camera.SetStatus("Camera streamer stopped.");
            camera.State = Camera.DeviceState.Connected;
        }   

        if (camera.State == Camera.DeviceState.Connected)
        {
            camera.SetStatus("Disconnecting from the camera device...");

            camera.cameraStreamer = null;

            camera.State = Camera.DeviceState.Disconnected;
            camera.SetStatus("The camera is disconnected.");
        }
    }
} 