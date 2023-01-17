/***
** Copyright (C) 2020-2023 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using WebService.Entites;
using WebService.Tracking;
using System.Text.Json;

namespace WebService.Services.Recording;

public class RecordingService : IRadarService 
{
    private const string SERVICE_ID = "RADAR_RECORDER";

    public static readonly string StoragePath = "./data/recordings";

    public const string RecordingDataFileExtention = ".rrec";

    public string ServiceId => SERVICE_ID;

    public RadarServiceSettings? Settings { get; set; }

    public RecordingService()
    {
        if (!System.IO.Directory.Exists(StoragePath))
        {
            System.Console.WriteLine("Creating device recordings folder.");
            System.IO.Directory.CreateDirectory(StoragePath);
        }
    }

    public IServiceContext CreateServiceContext(RadarDevice device, Dictionary<string,string> serviceOptions)
    {
        float frameRate = device.radarSettings!.DetectionParams!.FrameRate;

        string filename = $"{device.Id}_{DateTime.UtcNow.ToString("yyyy_MM_ddTHH_mm_ss")}";
        string recordingPath = System.IO.Path.Combine(StoragePath, $"{filename}{RecordingDataFileExtention}");

        string deviceString = JsonSerializer.Serialize(device);
        string configPath = System.IO.Path.Combine(StoragePath, $"{filename}.json");
        File.WriteAllText(configPath, deviceString);

        RecordingContext recordingContext = new RecordingContext(device.Id, recordingPath, frameRate);
        recordingContext.StartWorker();
        recordingContext.State = IServiceContext.ServiceState.Active;
        return recordingContext;
    }

    public void DisposeServiceContext(IServiceContext serviceContext)
    {
        RecordingContext recordingContext = (RecordingContext) serviceContext;
        recordingContext.StopWorker();
        recordingContext.State = IServiceContext.ServiceState.Initialized;
    }

    public void HandleFrame(FrameData frame, IServiceContext serviceContext)
    {
        RecordingContext recordingContext = (RecordingContext) serviceContext;
        recordingContext.RecordFrame(frame);
    }
}