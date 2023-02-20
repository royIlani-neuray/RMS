/***
** Copyright (C) 2020-2023 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using System.Text.Json.Serialization;
using WebService.Context;
using WebService.Entites;
using WebService.Events;

namespace WebService.Actions.Cameras;

public class DeleteCameraAction : CameraAction {

    public DeleteCameraAction(string cameraId) : base(cameraId) {}

    protected override void RunCameraAction(Camera camera)
    {
        System.Console.WriteLine($"Deleting camera - {camera.Id}");
        CameraContext.Instance.DeleteCamera(camera);

        RMSEvents.Instance.CameraDeletedEvent(camera.Id);        
    }
}