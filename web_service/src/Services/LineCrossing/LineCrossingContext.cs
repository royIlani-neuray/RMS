/***
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using WebService.Utils;
using WebService.RadarLogic.Tracking;
using WebService.Entites;

namespace WebService.Services.LineCrossing;

public class LineCrossingContext : WorkerThread<FrameData>, IServiceContext 
{
    public IServiceContext.ServiceState State { get; set; }

    private const int MAX_QUEUE_CAPACITY = 100;

    private TrackLineCrossingManager trackLineCrossingManager;

    public LineCrossingContext(Radar radar, List<LineSettings> lineSettingsList) : base("LineCrossingContext", MAX_QUEUE_CAPACITY)
    {
        State = IServiceContext.ServiceState.Initialized;
        trackLineCrossingManager = new TrackLineCrossingManager(radar, lineSettingsList);
    }

    public void HandleFrame(FrameData frameData)
    {
        Enqueue(frameData);
    }

    protected override async Task DoWork(FrameData frame)
    {
        trackLineCrossingManager.UpdateTracks(frame);
        await Task.CompletedTask;
    }
}