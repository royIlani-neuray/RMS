/***
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using WebService.Entites;
using WebService.Context;
using WebService.Events;

namespace WebService.Actions;

public abstract class CameraAction : EntityAction<Camera> {

    public CameraAction(string cameraId) : base(CameraContext.Instance, cameraId) {}

    protected abstract void RunCameraAction(Camera camera);
    protected override void RunAction(Camera camera)
    {
        RunCameraAction(camera);
    }

    protected override void RunPostActionTask(Camera camera)
    {
        RMSEvents.Instance.CameraUpdatedEvent(camera.Id);
    }
}