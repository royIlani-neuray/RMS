/***
** Copyright (C) 2020-2023 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using System.Text.Json.Serialization;
using WebService.Context;
using WebService.Entites;
using WebService.Events;

namespace WebService.Actions.Cameras;

public class AddCameraArgs 
{
    [JsonPropertyName("name")]
    public String Name { get; set; }

    [JsonPropertyName("description")]
    public String Description { get; set; }

    public AddCameraArgs()
    {
        Name = String.Empty;
        Description = String.Empty;
    }

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Name))
            throw new HttpRequestException("Camera name not defined");
    }
}

public class AddCameraAction : IAction 
{
    AddCameraArgs args;

    public AddCameraAction(AddCameraArgs args)
    {
        this.args = args;
    }

    public void Run()
    {
        args.Validate();

        Camera camera = new Camera();
        camera.Name = args.Name;
        camera.Description = args.Description;

        System.Console.WriteLine($"Adding new camera - [{camera.Name}]");
 
        CameraContext.Instance.AddCamera(camera);

        System.Console.WriteLine($"Camera added.");

        RMSEvents.Instance.CameraAddedEvent(camera.Id);
    }
}
