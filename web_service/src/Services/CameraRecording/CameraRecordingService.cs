/***
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using RtspClientSharpCore.RawFrames;
using WebService.Entites;
using WebService.RadarLogic.Tracking;
using System.Text.Json;
using WebService.Recordings;

namespace WebService.Services.RadarRecording;

public class CameraRecordingService : IExtensionService 
{
    public const string SERVICE_ID = "CAMERA_RECORDER";

    public string ServiceId => SERVICE_ID;

    public List<DeviceEntity.DeviceTypes> SupportedDeviceTypes => new List<DeviceEntity.DeviceTypes>() { DeviceEntity.DeviceTypes.Camera };

    public ExtensionServiceSettings? Settings { get; set; }


    public IServiceContext CreateServiceContext(DeviceEntity device, Dictionary<string,string> serviceOptions)
    {
        if (device.Type != DeviceEntity.DeviceTypes.Camera)
            throw new Exception("Unsupported device passed to service.");
        
        serviceOptions.TryGetValue(RecordingsManager.RECORDING_OVERRIDE_KEY, out string? recordingName);
        RecordingsManager.Instance.CreateRecordingEntry(device, out string entryPath, recordingName);

        Camera camera = (Camera) device;

        string deviceString = JsonSerializer.Serialize(camera);
        string configPath = System.IO.Path.Combine(entryPath, $"camera.json");
        File.WriteAllText(configPath, deviceString);

        string recordingVideoPath = System.IO.Path.Combine(entryPath, $"camera.h264");
        string recordingTimestampPath = System.IO.Path.Combine(entryPath, $"camera.ts.csv");

        CameraRecordingContext recordingContext = new CameraRecordingContext(recordingVideoPath, recordingTimestampPath);
        recordingContext.StartWorker();
        recordingContext.State = IServiceContext.ServiceState.Active;
        return recordingContext;
    }

    public void DisposeServiceContext(IServiceContext serviceContext)
    {
        CameraRecordingContext recordingContext = (CameraRecordingContext) serviceContext;
        recordingContext.StopWorker();
        recordingContext.State = IServiceContext.ServiceState.Initialized;
    }

    public void RunService(object dataObject, IServiceContext serviceContext)
    {
        if (dataObject is not RawFrame)
            return;

        RawFrame frame = (RawFrame) dataObject;
        CameraRecordingContext recordingContext = (CameraRecordingContext) serviceContext;
        recordingContext.RecordFrame(frame);
    }
}