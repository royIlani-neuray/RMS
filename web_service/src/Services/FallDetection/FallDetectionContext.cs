/***
** Copyright (C) 2020-2023 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using System.Text.Json;
using WebService.Utils;
using WebService.RadarLogic.Tracking;
using WebService.Entites;

namespace WebService.Services.FallDetection;

public class FallDetectionContext : WorkerThread<FrameData>, IServiceContext 
{
    public IServiceContext.ServiceState State { get; set; }

    private const int MAX_QUEUE_CAPACITY = 100;

    private FallDetectionTracker fallTracker;

    public FallDetectionContext(Radar radar, float fallingThreshold, float minTrackingDurationSeconds, float maxTrackingDurationSeconds, float alertCooldownSeconds) : base("FallDetectionContext", MAX_QUEUE_CAPACITY)
    {
        State = IServiceContext.ServiceState.Initialized;

        float frameRate = radar.radarSettings!.DetectionParams!.FrameRate;
        int minTrackingDurationFrames = (int) (minTrackingDurationSeconds * frameRate);
        int maxTrackingDurationFrames = (int) (maxTrackingDurationSeconds * frameRate);

        fallTracker = new FallDetectionTracker(radar.RadarWebSocket, fallingThreshold, minTrackingDurationFrames, maxTrackingDurationFrames, alertCooldownSeconds);
    }

    public void HandleFrame(FrameData frameData)
    {
        Enqueue(frameData);
    }

    protected override async Task DoWork(FrameData frame)
    {
        fallTracker.HandleFrame(frame);
        
        await Task.CompletedTask;
    }
}