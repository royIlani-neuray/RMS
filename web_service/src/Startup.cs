/***
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using System;
using System.Threading;
using System.Threading.Tasks;

using WebService.Database;
using WebService.Context;
using WebService.RadarLogic.IPRadar;
using WebService.Actions.Radars;
using WebService.Scheduler;
using WebService.Services;
using WebService.Events;
using System.Collections;

public class Startup 
{
    public static void SetServicePort()
    {
        var port = Environment.GetEnvironmentVariable("RMS_SERVICE_PORT");

        if (port != null)
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_URLS", $"http://+:{port}");
        }
    }

    public static void ApplicationStart(ConfigurationManager config)
    {
        string version = config["RMS_version"]!;

        Console.WriteLine($"Radar Management Service Started - Version: {version}");

        Console.WriteLine($"Running as user: {Environment.UserName}");

        ServiceSettings.Instance.RMSVersion = version;

        Console.WriteLine("Initializing DB...");
        StorageDatabase.DatabaseInit();
    
        Console.WriteLine("Initializing S3 connection...");
        var rmsName = config.GetSection("RMS_name").Get<String>();
        S3Manager.Instance.InitS3Connection(rmsName!);

        Console.WriteLine("Loading users from storage...");
        UserContext.Instance.LoadUsersFromStorage();

        Console.WriteLine("Loading templates from storage...");
        TemplateContext.Instance.LoadTemplatesFromStorage();

        Console.WriteLine("Loading recording schedules from storage...");
        RecordingScheduleContext.Instance.LoadSchedulesFromStorage();

        Console.WriteLine("Loading services...");
        var servicesSettings = config.GetSection("services").Get<List<ExtensionServiceSettings>>();

        ServiceManager.Instance.InitExtensionServices(servicesSettings);

        Console.WriteLine("Loading cameras from storage...");
        CameraContext.Instance.LoadCamerasFromStorage();

        Console.WriteLine("Loading radars from storage...");
        RadarContext.Instance.LoadRadarsFromStorage();

        Console.WriteLine("Starting Radar Device Mapper...");
        RadarDeviceMapper.Instance.SetDeviceDiscoveredCallback(DeviceDiscoveredAction.OnDeviceDiscoveredCallback);
        RadarDeviceMapper.Instance.Start();

        Console.WriteLine("Starting Events WebSocket...");
        RMSEvents.Instance.StartWorker();   

        Console.WriteLine("Starting Device Mapping Scheduler...");
        DeviceMappingScheduler.Instance.Start();

        Console.WriteLine("Starting Connection Scheduler...");
        ConnectionScheduler.Instance.Start();

        Console.WriteLine("Starting Recordings Scheduler...");
        RecordingScheduler.Instance.Start();
    }

}