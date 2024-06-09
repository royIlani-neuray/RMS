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
using WebService.Recordings;
using WebService.Events;
using WebService.Services.RadarRecording;

namespace WebService.Actions.Recordings;

public class StartRecordingArgs 
{
    [JsonPropertyName("recording_name")]
    public string RecordingName { get; set; }

    [JsonPropertyName("radars")]
    public List<string> RadarIds { get; set; }

    [JsonPropertyName("cameras")]
    public List<string> CameraIds { get; set; }

    public StartRecordingArgs()
    {
        RecordingName = String.Empty;
        RadarIds = new List<string>();
        CameraIds = new List<string>();
    }
}

public class StartRecordingAction : IAction
{
    private StartRecordingArgs args;

    public StartRecordingAction(StartRecordingArgs args)
    {
        this.args = args;
    }

    public void Run()
    {
        string recordingName = args.RecordingName;

        if (string.IsNullOrWhiteSpace(recordingName))
        {
            recordingName = DateTime.UtcNow.ToString("yyyy-MM-dd_HH-mm-ss.fff");
        }

        if (RecordingsManager.Instance.IsRecordingExist(recordingName))
        {
            throw new BadRequestException("Recording with the given name already exist!");
        }

        LinkServiceArgs serviceArgs = new LinkServiceArgs();
        serviceArgs.ServiceOptions.Add(RecordingsManager.RECORDING_OVERRIDE_KEY, recordingName);
        serviceArgs.ServiceId = RadarRecordingService.SERVICE_ID;
        
        foreach (string radarId in args.RadarIds)
        {
            var radar = RadarContext.Instance.GetRadar(radarId);

            if (radar.LinkedServices.Exists(service => service.ServiceId == RadarRecordingService.SERVICE_ID))
                continue;

            var action = new LinkRadarServiceAction(radarId, serviceArgs);
            action.Run();
            RMSEvents.Instance.RecordingStartedEvent(radarId);
        }

        serviceArgs.ServiceId = CameraRecordingService.SERVICE_ID;

        foreach (string cameraId in args.CameraIds)
        {
            var camera = CameraContext.Instance.GetCamera(cameraId);
            
            if (camera.LinkedServices.Exists(service => service.ServiceId == CameraRecordingService.SERVICE_ID))
                continue;

            var action = new LinkCameraServiceAction(cameraId, serviceArgs);
            action.Run();
            RMSEvents.Instance.RecordingStartedEvent(cameraId);
        }
    }
}