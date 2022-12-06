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
using InferenceService.Controllers;
using InferenceService.Context;
using InferenceService.Storage;
using InferenceService.Entities;

public class Startup 
{
    public static void SetServicePort()
    {
        var port = Environment.GetEnvironmentVariable("INFERENCE_SERVICE_PORT");

        if (port != null)
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_URLS", $"http://+:{port}");
        }
    }

    public static void ApplicationStart(ConfigurationManager config)
    {
        string version = config["service_version"]!;
        ServiceController.ServiceVersion = version;

        Console.WriteLine($"Inference Service Started - Version: {version}");

        Console.WriteLine($"Running as user: {Environment.UserName}");

        Console.WriteLine("Initializing storage...");
        ModelsStorage.InitStorage();

        Console.WriteLine("Loading models from storage...");
        ModelsContext.Instance.LoadModelsFromStorage();

        /*
        Model tempModle = new Model();
        tempModle.Name = "temp";
        tempModle.Description = "temp model";
        tempModle.ModelType = Model.ModelTypes.GateId;
        ModelsContext.Instance.AddModel(tempModle);
        */
    }

}