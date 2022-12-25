/***
** Copyright (C) 2020-2023 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using WebService.Entites;
using WebService.Tracking;
using System.Text.Json;

namespace WebService.Services.Inference.GateId;

public class GateIdService : IRadarService
{
    private const string SERVICE_ID = "GATE_ID_CLOSED_SET";

    public string ServiceId => SERVICE_ID;

    public RadarServiceSettings? Settings { get; set; }

    public IServiceContext CreateServiceContext(RadarDevice device, Dictionary<string, string> serviceOptions)
    {
        GateIdContext gateIdContext = new GateIdContext();
        gateIdContext.StartWorker();
        gateIdContext.State = IServiceContext.ServiceState.Active;
        return gateIdContext;
    }

    public void DisposeServiceContext(IServiceContext serviceContext)
    {
        GateIdContext gateIdContext = (GateIdContext) serviceContext;
        gateIdContext.StopWorker();
        gateIdContext.State = IServiceContext.ServiceState.Initialized;
    }

    public void HandleFrame(FrameData frame, IServiceContext serviceContext)
    {
        GateIdContext gateIdContext = (GateIdContext) serviceContext;
        gateIdContext.HandleFrame(frame);
    }
}
