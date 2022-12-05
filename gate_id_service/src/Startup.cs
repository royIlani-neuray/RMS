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
using GateId.Controllers;

public class Startup 
{
    public static void ApplicationStart(ConfigurationManager config)
    {
        string version = config["service_version"]!;
        ServiceController.ServiceVersion = version;

        Console.WriteLine($"Gate ID Service Started - Version: {version}");

        Console.WriteLine($"Running as user: {Environment.UserName}");
    }

}