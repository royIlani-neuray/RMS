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

using DeviceEmulator;

public class Startup 
{
    public static void SetServicePort()
    {
        var port = Environment.GetEnvironmentVariable("DEVICE_EMULATOR_PORT");

        if (port != null)
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_URLS", $"http://+:{port}");
        }
    }

    public static void ApplicationStart(ConfigurationManager config)
    {
        string version = config["emulator_version"]!;
        EmulatorSettings.Instance.EmulatorVersion = version;

        Console.WriteLine($"Radar Device Emulator Started - Version: {version}");

        Console.WriteLine($"Running as user: {Environment.UserName}");

        Emulator.Instance.Start();
    }

}