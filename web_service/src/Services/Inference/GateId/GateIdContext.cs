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
using System.Text.Json;

namespace WebService.Services.Inference.GateId;

public class GateIdContext : WorkerThread<FrameData>, IServiceContext 
{
    public IServiceContext.ServiceState State { get; set; }

    private const int MAX_QUEUE_CAPACITY = 5;
    
    public GateIdContext() : base(MAX_QUEUE_CAPACITY)
    {
        State = IServiceContext.ServiceState.Initialized;
    }

    public void HandleFrame(FrameData frameData)
    {
        Enqueue(frameData);
    }

    protected override void DoWork(FrameData workItem)
    {

    }
}