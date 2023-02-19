/***
** Copyright (C) 2020-2023 neuRay Labs. All rights reserved.
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
using WebService.Radar;
using WebService.Actions.Radar;
using WebService.Scheduler;
using WebService.Services;
using WebService.Events;

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

        Console.WriteLine("Loading users from storage...");
        UserContext.Instance.LoadUsersFromStorage();

        Console.WriteLine("Loading templates from storage...");
        TemplateContext.Instance.LoadTemplatesFromStorage();

        Console.WriteLine("Loading services...");
        var servicesSettings = config.GetSection("services").Get<List<RadarServiceSettings>>();

        ServiceManager.Instance.InitServices(servicesSettings);

        Console.WriteLine("Loading devices from storage...");
        DeviceContext.Instance.LoadDevicesFromStorage();

        Console.WriteLine("Starting Device Mapper...");
        DeviceMapper.Instance.SetDeviceDiscoveredCallback(DeviceDiscoveredAction.OnDeviceDiscoveredCallback);
        DeviceMapper.Instance.Start();

        Console.WriteLine("Starting Events WebSocket...");
        RMSEvents.Instance.StartWorker();   

        Console.WriteLine("Starting Device Mapping Scheduler...");
        DeviceMappingScheduler.Instance.Start();

        Console.WriteLine("Starting Connection Scheduler...");
        ConnectionScheduler.Instance.Start();
    }

}