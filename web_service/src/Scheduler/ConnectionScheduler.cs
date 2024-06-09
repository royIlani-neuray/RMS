/***
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using WebService.Context;
using WebService.Actions.Radars;
using WebService.Actions.Cameras;

namespace WebService.Scheduler;

public class ConnectionScheduler : TaskScheduler{

    #region Singleton
    
    private static object singletonLock = new object();
    private static volatile ConnectionScheduler? instance; 

    public static ConnectionScheduler Instance {
        get 
        {
            if (instance == null)
            {
                lock (singletonLock)
                {
                    if (instance == null)
                        instance = new ConnectionScheduler();
                }
            }

            return instance;
        }
    }

    private ConnectionScheduler() : base(SCHEDULING_PERIOD_MINUTES) {}

    #endregion

    private const double SCHEDULING_PERIOD_MINUTES = 2;

    private void ConnectRadars()
    {
        var radarsList = RadarContext.Instance.GetRadarsBrief();

        foreach (var radarInfo in radarsList)
        {
            if (radarInfo.State == Entites.Radar.DeviceState.Disconnected)
            {
                try
                {
                    var action = new ReconnectRadarAction(radarInfo.Id);
                    action.Run();
                }
                catch 
                {
                }
            }
        }
    }

    private void ConnectCameras()
    {
        var camerasList = CameraContext.Instance.GetCamerasBrief();

        foreach (var cameraInfo in camerasList)
        {
            if (cameraInfo.State == Entites.Camera.DeviceState.Disconnected)
            {
                try
                {
                    var action = new ReconnectCameraAction(cameraInfo.Id);
                    action.Run();
                }
                catch 
                {
                }
            }
        }
    }    

    public override void RunTask()
    {
        ConnectRadars();
        ConnectCameras();
    }

}