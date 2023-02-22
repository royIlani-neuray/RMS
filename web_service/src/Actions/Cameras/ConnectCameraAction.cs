/***
** Copyright (C) 2020-2023 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using WebService.CameraLogic.CameraStream;
using WebService.Entites;

namespace WebService.Actions.Cameras;

public class ConnectCameraAction : IAction 
{
    Camera camera;

    public ConnectCameraAction(Camera camera)
    {
        this.camera = camera;
    }

    public void Run()
    {
        //System.Console.WriteLine($"Debug: ConnectCameraAction - state: {camera.State}, enabled: {camera.Enabled}");

        if (!camera.Enabled)
        {
            Console.WriteLine($"[{camera.Id}] camera device is disabled - ignore connect action.");
            return;
        }

        if (camera.State == Camera.DeviceState.Disconnected)
        {
            camera.SetStatus("Connecting to the camera...");

            try
            {
                camera.cameraStreamer = new CameraStreamer(camera);
            }
            catch
            {
                camera.SetStatus("Error: connection attempt to the camera failed.");
                return;
            }

            camera.State = Camera.DeviceState.Connected;
            camera.SetStatus("The camera device is connected.");
        }

        if (camera.State == Radar.DeviceState.Connected)
        {
            camera.SetStatus("Starting camera stream...");
            camera.cameraStreamer!.Start();

            camera.State = Camera.DeviceState.Active;
        }
    }
} 