/***
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using System.Text.Json.Serialization;
using WebService.Actions.Services;
using WebService.Context;
using WebService.Events;
using WebService.Recordings;
using WebService.Services.RadarRecording;

namespace WebService.Actions.Recordings;

public class StopRecordingArgs 
{
    [JsonPropertyName("radars")]
    public List<string> RadarIds { get; set; }

    [JsonPropertyName("cameras")]
    public List<string> CameraIds { get; set; }

    public StopRecordingArgs()
    {
        RadarIds = new List<string>();
        CameraIds = new List<string>();
    }
}

public class StopRecordingAction : IAction
{
    private StopRecordingArgs args;

    public StopRecordingAction(StopRecordingArgs args)
    {
        this.args = args;
    }

    public void Run()
    {
        foreach (string radarId in args.RadarIds)
        {
            var radar = RadarContext.Instance.GetRadar(radarId);
            
            var linkedService = radar.LinkedServices.FirstOrDefault(linkedService => linkedService.ServiceId == RadarRecordingService.SERVICE_ID);
            if (linkedService == null)
                continue;

            var action = new UnlinkRadarServiceAction(radarId, RadarRecordingService.SERVICE_ID);
            action.Run();

            RMSEvents.Instance.RecordingStoppedEvent(radarId);
            linkedService.ServiceOptions.TryGetValue(RecordingsManager.RECORDING_NAME, out string? recordingName);
            linkedService.ServiceOptions.TryGetValue(RecordingsManager.UPLOAD_S3, out string? uploadS3);
            RecordingsManager.Instance.MarkDeviceRecordingFinished(recordingName!, radarId);
            if (ServiceSettings.Instance.CloudUploadSupport &&
                RecordingsManager.Instance.IsRecordingFinished(recordingName!) &&
                bool.Parse(uploadS3!)) {
                    RecordingsManager.Instance.UploadRecordingToS3(recordingName!);
            }
        }

        foreach (string cameraId in args.CameraIds)
        {
            var camera = CameraContext.Instance.GetCamera(cameraId);
            
            var linkedService = camera.LinkedServices.FirstOrDefault(linkedService => linkedService.ServiceId == RadarRecordingService.SERVICE_ID);
            if (linkedService == null)
                continue;
            
            var action = new UnlinkCameraServiceAction(cameraId, CameraRecordingService.SERVICE_ID);
            action.Run();

            RMSEvents.Instance.RecordingStoppedEvent(cameraId);
            linkedService.ServiceOptions.TryGetValue(RecordingsManager.RECORDING_NAME, out string? recordingName);
            linkedService.ServiceOptions.TryGetValue(RecordingsManager.UPLOAD_S3, out string? uploadS3);
            RecordingsManager.Instance.MarkDeviceRecordingFinished(recordingName!, cameraId);
            if (ServiceSettings.Instance.CloudUploadSupport &&
                RecordingsManager.Instance.IsRecordingFinished(recordingName!) &&
                bool.Parse(uploadS3!)) {
                    RecordingsManager.Instance.UploadRecordingToS3(recordingName!);
            }
        }
    }
}