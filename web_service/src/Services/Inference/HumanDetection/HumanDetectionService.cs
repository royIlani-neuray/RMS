/***
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using WebService.Entites;
using WebService.RadarLogic.Streaming;
using WebService.Services.Inference.GateId;

namespace WebService.Services.Inference.HumanDetection;

public class HumanDetectionService : IExtensionService
{
    private const string SERVICE_ID = "HUMAN_DETECTION";
    private const int REQUIRED_WINDOW_SIZE = 15;
    private const int WINDOW_SHIFT_SIZE = 15;   // amount of frames to remove from the window after inference.
    private const int PREDICTION_REQUIRED_HIT_COUNT = 1;
    private const int PREDICTION_REQUIRED_MISS_COUNT = 1;

    private const string SERVICE_OPTION_MODEL_NAME = "human_detection_model";

    public string ServiceId => SERVICE_ID;

    public List<DeviceEntity.DeviceTypes> SupportedDeviceTypes => new List<DeviceEntity.DeviceTypes>() { DeviceEntity.DeviceTypes.Radar };

    public ExtensionServiceSettings? Settings { get; set; }

    private void GetServiceSettings(Dictionary<string, string> serviceOptions, out string modelName)
    {
        if (!serviceOptions.ContainsKey(SERVICE_OPTION_MODEL_NAME))
            throw new BadRequestException($"Missing required service option: {SERVICE_OPTION_MODEL_NAME}");
        
        modelName = serviceOptions[SERVICE_OPTION_MODEL_NAME];
    }

    public IServiceContext CreateServiceContext(DeviceEntity device, Dictionary<string, string> serviceOptions)
    {
        if (device.Type != DeviceEntity.DeviceTypes.Radar)
            throw new Exception("Unsupported device passed to service.");

        Radar radar = (Radar) device;
        GetServiceSettings(serviceOptions, out string modelName);
        GateIdContext gateIdContext = new GateIdContext(radar, modelName, REQUIRED_WINDOW_SIZE, WINDOW_SHIFT_SIZE, PREDICTION_REQUIRED_HIT_COUNT, PREDICTION_REQUIRED_MISS_COUNT);
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

    public void RunService(object dataObject, IServiceContext serviceContext)
    {
        if (dataObject is not FrameData)
            return;

        FrameData frame = (FrameData) dataObject;
        GateIdContext gateIdContext = (GateIdContext) serviceContext;
        gateIdContext.HandleFrame(frame);
    }
}
