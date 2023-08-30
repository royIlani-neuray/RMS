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

namespace WebService.Services.Inference.SmartFanGestures;

public class SmartFanContext : WorkerThread<FrameData>, IServiceContext 
{
    public IServiceContext.ServiceState State { get; set; }

    private const int MAX_QUEUE_CAPACITY = 20;

    private string modelName;
    
    public SmartFanContext(Radar radar, string modelName) : base("SmartFanContext", MAX_QUEUE_CAPACITY)
    {
        State = IServiceContext.ServiceState.Initialized;

        this.modelName = modelName;
    }

    public void HandleFrame(FrameData frameData)
    {
        Enqueue(frameData);
    }

    protected override async Task DoWork(FrameData frame)
    {
        await Task.CompletedTask;
    }
}