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
    private const int REQUIRED_WINDOW_SIZE = 30;

    private const int PREDICTION_REQUIRED_HIT_COUNT = 3;
    private const int PREDICTION_REQUIRED_MISS_COUNT = 2;

    private const string SERVICE_OPTION_MODEL_NAME = "gate_id_model";

    public string ServiceId => SERVICE_ID;

    public RadarServiceSettings? Settings { get; set; }

    private void GetServiceSettings(Dictionary<string, string> serviceOptions, out string modelName)
    {
        if (!serviceOptions.ContainsKey(SERVICE_OPTION_MODEL_NAME))
            throw new BadRequestException($"Missing required service option: {SERVICE_OPTION_MODEL_NAME}");
        
        modelName = serviceOptions[SERVICE_OPTION_MODEL_NAME];
    }

    public IServiceContext CreateServiceContext(Radar device, Dictionary<string, string> serviceOptions)
    {
        GetServiceSettings(serviceOptions, out string modelName);
        GateIdContext gateIdContext = new GateIdContext(device, modelName, REQUIRED_WINDOW_SIZE, PREDICTION_REQUIRED_HIT_COUNT, PREDICTION_REQUIRED_MISS_COUNT);
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
