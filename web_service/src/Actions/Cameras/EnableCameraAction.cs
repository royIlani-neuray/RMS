/***
** Copyright (C) 2020-2023 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using WebService.Entites;

namespace WebService.Actions.Cameras;

public class EnableCameraAction : CameraAction 
{
    public EnableCameraAction(string cameraId) : base(cameraId) {}

    protected override void RunCameraAction(Camera camera)
    {
        if (camera.Enabled)
            return; // nothing to do.
        
        camera.SetStatus("Enabling camera device...");
        camera.Enabled = true;

        var connectAction = new ConnectCameraAction(camera);
        connectAction.Run();
    }
}