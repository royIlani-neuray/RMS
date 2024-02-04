/***
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using WebService.Entites;
using WebService.RadarLogic.Tracking;
using System.Text.Json;
using WebService.Recordings;

namespace WebService.Services.RadarRecording;

public class RadarRecordingService : IExtensionService 
{
    public const string SERVICE_ID = "RADAR_RECORDER";

    public static readonly string StoragePath = "./data/recordings";

    public const string RecordingDataFileExtention = ".rrec";

    public string ServiceId => SERVICE_ID;

    public List<DeviceEntity.DeviceTypes> SupportedDeviceTypes => new List<DeviceEntity.DeviceTypes>() { DeviceEntity.DeviceTypes.Radar };

    public ExtensionServiceSettings? Settings { get; set; }

    public IServiceContext CreateServiceContext(DeviceEntity device, Dictionary<string,string> serviceOptions)
    {
        if (device.Type != DeviceEntity.DeviceTypes.Radar)
            throw new Exception("Unsupported device passed to service.");
        
        serviceOptions.TryGetValue(RecordingsManager.RECORDING_OVERRIDE_KEY, out string? recordingName);
        RecordingsManager.Instance.CreateRecordingEntry(device, out string entryPath, recordingName);

        Radar radar = (Radar) device;

        float frameRate;

        if (radar.radarSettings!.DetectionParams == null)
        {
            // TODO: need to parse DetectionParams on configs that has subframes. for now this is a patch...
            System.Console.WriteLine("Warning: unknown frame rate, setting to 4.16");
            frameRate = 4.16F;
        }
        else
        {
            frameRate = radar.radarSettings!.DetectionParams!.FrameRate;
        }

        string recordingPath = System.IO.Path.Combine(entryPath, $"radar.rrec");

        string deviceString = JsonSerializer.Serialize(radar);
        string configPath = System.IO.Path.Combine(entryPath, $"radar.json");
        File.WriteAllText(configPath, deviceString);

        RadarRecordingContext recordingContext = new RadarRecordingContext(radar.Id, recordingPath, frameRate);
        recordingContext.StartWorker();
        recordingContext.State = IServiceContext.ServiceState.Active;
        return recordingContext;
    }

    public void DisposeServiceContext(IServiceContext serviceContext)
    {
        RadarRecordingContext recordingContext = (RadarRecordingContext) serviceContext;
        recordingContext.StopWorker();
        recordingContext.State = IServiceContext.ServiceState.Initialized;
    }

    public void RunService(object dataObject, IServiceContext serviceContext)
    {
        if (dataObject is not FrameData)
            return;

        FrameData frame = (FrameData) dataObject;
        RadarRecordingContext recordingContext = (RadarRecordingContext) serviceContext;
        recordingContext.RecordFrame(frame);
    }
}