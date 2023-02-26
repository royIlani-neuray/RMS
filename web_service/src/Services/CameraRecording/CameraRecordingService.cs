/***
** Copyright (C) 2020-2023 neuRay Labs. All rights reserved.
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

namespace WebService.Services.RadarRecording;

public class CameraRecordingService : IExtensionService 
{
    private const string SERVICE_ID = "CAMERA_RECORDER";

    public static readonly string StoragePath = "./data/recordings";

    public const string RecordingDataFileExtention = ".h264";

    public string ServiceId => SERVICE_ID;

    public List<DeviceEntity.DeviceTypes> SupportedDeviceTypes => new List<DeviceEntity.DeviceTypes>() { DeviceEntity.DeviceTypes.Camera };

    public ExtensionServiceSettings? Settings { get; set; }

    public CameraRecordingService()
    {
        if (!System.IO.Directory.Exists(StoragePath))
        {
            System.Console.WriteLine("Creating device recordings folder.");
            System.IO.Directory.CreateDirectory(StoragePath);
        }
    }

    public IServiceContext CreateServiceContext(DeviceEntity device, Dictionary<string,string> serviceOptions)
    {
        if (device.Type != DeviceEntity.DeviceTypes.Camera)
            throw new Exception("Unsupported device passed to service.");
        
        Camera camera = (Camera) device;
        
        string filename = $"{camera.Id}_{DateTime.UtcNow.ToString("yyyy_MM_ddTHH_mm_ss")}";
        string recordingPath = System.IO.Path.Combine(StoragePath, $"{filename}{RecordingDataFileExtention}");

        CameraRecordingContext recordingContext = new CameraRecordingContext(recordingPath);
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