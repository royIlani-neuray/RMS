/***
** Copyright (C) 2020-2023 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using WebService.Utils;
using WebService.Tracking;

namespace WebService.Services.Inference.GateId;

public class GateIdContext : WorkerThread<FrameData>, IServiceContext 
{
    public IServiceContext.ServiceState State { get; set; }

    private const int MAX_QUEUE_CAPACITY = 5;

    private TracksWindowBuilder tracksWindowBuilder;
    private string modelName;
    
    public GateIdContext(string modelName, int requiredWindowSize) : base(MAX_QUEUE_CAPACITY)
    {
        State = IServiceContext.ServiceState.Initialized;
        tracksWindowBuilder = new TracksWindowBuilder(requiredWindowSize);
        this.modelName = modelName;
    }

    public void HandleFrame(FrameData frameData)
    {
        Enqueue(frameData);
    }

    protected override Task DoWork(FrameData frame)
    {
        tracksWindowBuilder.AddFrame(frame);

        var readyWindows = tracksWindowBuilder.PullReadyWindows();

        //TODO: call the inference service....
        return Task.CompletedTask;
    }
}