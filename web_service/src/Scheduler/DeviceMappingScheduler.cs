/***
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using Serilog;
using WebService.RadarLogic.IPRadar;

namespace WebService.Scheduler;

public class DeviceMappingScheduler : TaskScheduler {

    #region Singleton
    
    private static object singletonLock = new object();
    private static volatile DeviceMappingScheduler? instance; 

    public static DeviceMappingScheduler Instance {
        get 
        {
            if (instance == null)
            {
                lock (singletonLock)
                {
                    if (instance == null)
                        instance = new DeviceMappingScheduler();
                }
            }

            return instance;
        }
    }

    private DeviceMappingScheduler() : base(SCHEDULING_PERIOD_MINUTES) {}

    #endregion

    private const double SCHEDULING_PERIOD_MINUTES = 2;

    public override void RunTask()
    {
        // Trigger device mapping
        Log.Information("Mapping Scheduler: Triggering device mapping.");

        RadarDeviceMapper.Instance.MapDevices();
    }

}