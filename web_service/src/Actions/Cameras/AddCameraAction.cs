/***
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
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

    [JsonPropertyName("rtsp_url")]
    public String RTSPUrl { get; set; }

    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = false;

    [JsonPropertyName("frame_rate")]
    public float FrameRate { get; set; }

    [JsonPropertyName("fov_x")]
    public int FieldOfViewX { get; set; }

    [JsonPropertyName("fov_y")]
    public int FieldOfViewY { get; set; }

    [JsonPropertyName("resolution_x")]
    public int ResolutionX { get; set; }

    [JsonPropertyName("resolution_y")]
    public int ResolutionY { get; set; }

    public AddCameraArgs()
    {
        Name = String.Empty;
        Description = String.Empty;
        RTSPUrl = String.Empty;
    }

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Name))
            throw new HttpRequestException("Camera name not defined");
        if (string.IsNullOrWhiteSpace(RTSPUrl))
            throw new HttpRequestException("Camera RTSP url not defined");
    }
}

public class AddCameraAction : IAction 
{
    AddCameraArgs args;

    public string CameraId { get; set; }

    public AddCameraAction(AddCameraArgs args)
    {
        this.args = args;
        CameraId = String.Empty;
    }

    public void Run()
    {
        args.Validate();

        Camera camera = new Camera();
        camera.Name = args.Name;
        camera.Description = args.Description;
        camera.RTSPUrl = args.RTSPUrl;
        camera.FrameRate = args.FrameRate;
        camera.FieldOfViewX = args.FieldOfViewX;
        camera.FieldOfViewY = args.FieldOfViewY;
        camera.ResolutionX = args.ResolutionX;
        camera.ResolutionY = args.ResolutionY;

        System.Console.WriteLine($"Adding new camera - [{camera.Name}]");
 
        CameraContext.Instance.AddCamera(camera);

        System.Console.WriteLine($"Camera added.");

        RMSEvents.Instance.CameraAddedEvent(camera.Id);

        if (args.Enabled)
        {
            Task enableCameraTask = new Task(() => 
            {
                try
                {
                    var action = new EnableCameraAction(camera.Id);
                    action.Run();
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine($"Error: could not enable camera device! - {ex.Message}");
                }
            });
            enableCameraTask.Start();
        }

        CameraId = camera.Id;
    }
}
