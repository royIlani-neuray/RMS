/***
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using WebService.Context;
using WebService.Entites;
using WebService.Events;

namespace WebService.Actions.Cameras;

public class DeleteCameraAction : CameraAction {

    public DeleteCameraAction(string cameraId) : base(cameraId) {}

    protected override void RunCameraAction(Camera camera)
    {
        System.Console.WriteLine($"Deleting camera - {camera.Id}");

        var disconnectAction = new DisconnectCameraAction(camera);
        disconnectAction.Run();

        CameraContext.Instance.DeleteCamera(camera);

        camera.CameraWebSocket.CloseServer();

        camera.SetStatus("Camera device deleted.");

        RMSEvents.Instance.CameraDeletedEvent(camera.Id);      
    }
}