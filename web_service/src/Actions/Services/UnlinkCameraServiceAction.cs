/***
** Copyright (C) 2020-2023 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using WebService.Entites;

namespace WebService.Actions.Services;

public class UnlinkCameraServiceAction : CameraAction 
{
    private string serviceId;

    public UnlinkCameraServiceAction(string cameraId, string serviceId) : base(cameraId) 
    {
        this.serviceId = serviceId;
    }

    protected override void RunCameraAction(Camera camera)
    {
        var unlinkServiceAction = new UnlinkServiceAction(camera, serviceId);
        unlinkServiceAction.Run();
    }
}