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

public class DisableCameraAction : CameraAction 
{
    public DisableCameraAction(string cameraId) : base(cameraId) {}

    protected override void RunCameraAction(Camera camera)
    {
        if (!camera.Enabled)
            return; // nothing to do.

        camera.SetStatus("Disabling camera device...");

        var disconnectAction = new DisconnectCameraAction(camera);
        disconnectAction.Run();

        camera.Enabled = false;
        camera.SetStatus("The camera is disabled.");
    }
}