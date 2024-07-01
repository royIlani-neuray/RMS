/***
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using WebService.Context;
using WebService.Actions.Cameras;
using WebService.Actions.Services;
using WebService.Services.CameraRecording;
namespace WebService.Scheduler;

public class CameraResetScheduler : TaskScheduler{

    #region Singleton
    
    private static object singletonLock = new object();
    private static volatile CameraResetScheduler? instance; 

    public static CameraResetScheduler Instance {
        get 
        {
            if (instance == null)
            {
                lock (singletonLock)
                {
                    if (instance == null)
                        instance = new CameraResetScheduler();
                }
            }

            return instance;
        }
    }

    private CameraResetScheduler() : base(SCHEDULING_PERIOD_MINUTES) {}

    #endregion

    private const double SCHEDULING_PERIOD_MINUTES = 180;


    public override void RunTask()
    {
        var camerasList = CameraContext.Instance.GetCamerasBrief();
        System.Console.WriteLine($"Sending command to reset cameras...");

        foreach (var cameraInfo in camerasList)
        {
            if (cameraInfo.State == Entites.Camera.DeviceState.Active)
            {
                var camera = CameraContext.Instance.GetCamera(cameraInfo.Id);
                if (!camera.LinkedServices.Exists(service => service.ServiceId == CameraRecordingService.SERVICE_ID))
                {
                    try
                    {
                        var disableAction = new DisableCameraAction(cameraInfo.Id);
                        disableAction.Run();
                        var enableAction = new EnableCameraAction(cameraInfo.Id);
                        enableAction.Run();
                    }
                    catch 
                    {
                    }
                }
            }
        }
    }    

    // public override void RunTask()
    // {
    //     RestartCameras();
    // }

}