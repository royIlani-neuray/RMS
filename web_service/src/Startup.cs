/***
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using WebService.Database;
using WebService.Context;
using WebService.RadarLogic.IPRadar;
using WebService.Actions.Radars;
using WebService.Scheduler;
using WebService.Services;
using WebService.Events;
using Serilog;
using WebService.Recordings;

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

        Log.Information($"Radar Management Service Started - Version: {version}");

        Log.Information($"Running as user: {Environment.UserName}");

        RMSSettings.Instance.RMSVersion = version;

        Log.Information("Initializing DB...");
        StorageDatabase.DatabaseInit();
    
        Log.Information("Initializing S3 connection...");
        var rmsName = config.GetSection("RMS_name").Get<String>();
        S3Manager.Instance.InitS3Connection(rmsName!);

        Log.Information("Loading users from storage...");
        UserContext.Instance.LoadUsersFromStorage();

        Log.Information("Loading templates from storage...");
        TemplateContext.Instance.LoadTemplatesFromStorage();

        Log.Information("Loading recording schedules from storage...");
        RecordingScheduleContext.Instance.LoadSchedulesFromStorage();

        Log.Information("Loading services...");
        var servicesSettings = config.GetSection("services").Get<List<ExtensionServiceSettings>>();

        ServiceManager.Instance.InitExtensionServices(servicesSettings);

        Log.Information("Loading cameras from storage...");
        CameraContext.Instance.LoadCamerasFromStorage();

        Log.Information("Loading radars from storage...");
        RadarContext.Instance.LoadRadarsFromStorage();

        Log.Information("Loading Device Groups...");
        DeviceGroupContext.Instance.LoadDeviceGroupsFromStorage();

        Log.Information("Starting Radar Device Mapper...");
        RadarDeviceMapper.Instance.SetDeviceDiscoveredCallback(DeviceDiscoveredAction.OnDeviceDiscoveredCallback);
        RadarDeviceMapper.Instance.Start();

        Log.Information("Starting Events WebSocket...");
        RMSEvents.Instance.StartWorker();   

        Log.Information("initializing Recording Manager...");
        RecordingsManager.Instance.Init();

        Log.Information("Starting Device Mapping Scheduler...");
        DeviceMappingScheduler.Instance.Start();

        Log.Information("Starting Connection Scheduler...");
        ConnectionScheduler.Instance.Start();

        Log.Information("Starting Recordings Scheduler...");
        RecordingScheduler.Instance.Start();

        Log.Information("Starting Recordings Rettention Scheduler...");
        RecordingsRetentionScheduler.Instance.Start();

    }

}