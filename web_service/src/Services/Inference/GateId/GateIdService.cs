/***
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using WebService.Entites;
using WebService.RadarLogic.Tracking;
using System.Text.Json;

namespace WebService.Services.Inference.GateId;

public class GateIdService : IExtensionService
{
    private const string SERVICE_ID = "GATE_ID_CLOSED_SET";
    // private const int REQUIRED_WINDOW_SIZE = 30;    // amount of frames required for inference
    private const int REQUIRED_WINDOW_SIZE = 32;    // amount of frames required for inference

    private const int GAIT_WINDOW_SHIFT_SIZE = 8;   // amount of frames to remove from the window after inference.

    private const int MAJORITY_PREDICTOR_MIN_REQUIRED_HITS = 3;
    private const int MAJORITY_PREDICTOR_WINDOW_SIZE = 10;

    private const string SERVICE_OPTION_MODEL_NAME = "gate_id_model";
    private const string SERVICE_OPTION_MAJORITY_WINDOW_SIZE = "majority_window_size";
    private const string SERVICE_OPTION_MAJORITY_MIN_HITS = "majority_required_hits";

    private const string SERVICE_OPTION_GATE_WINDOW_SHIFT_SIZE = "gait_window_shift_size";

    public string ServiceId => SERVICE_ID;

    public List<DeviceEntity.DeviceTypes> SupportedDeviceTypes => new List<DeviceEntity.DeviceTypes>() { DeviceEntity.DeviceTypes.Radar };

    public ExtensionServiceSettings? Settings { get; set; }

    private void GetServiceSettings(Dictionary<string, string> serviceOptions, out string modelName, out int gaitWindowShitSize, out int minRequiredHitCount, out int majorityWindowSize)
    {
        majorityWindowSize = MAJORITY_PREDICTOR_WINDOW_SIZE;
        if (serviceOptions.ContainsKey(SERVICE_OPTION_MAJORITY_WINDOW_SIZE))
            majorityWindowSize = int.Parse(serviceOptions[SERVICE_OPTION_MAJORITY_WINDOW_SIZE]);

        minRequiredHitCount = MAJORITY_PREDICTOR_MIN_REQUIRED_HITS;
        if (serviceOptions.ContainsKey(SERVICE_OPTION_MAJORITY_MIN_HITS))
            minRequiredHitCount = int.Parse(serviceOptions[SERVICE_OPTION_MAJORITY_MIN_HITS]);

        gaitWindowShitSize = GAIT_WINDOW_SHIFT_SIZE;
        if (serviceOptions.ContainsKey(SERVICE_OPTION_GATE_WINDOW_SHIFT_SIZE))
            gaitWindowShitSize = int.Parse(serviceOptions[SERVICE_OPTION_GATE_WINDOW_SHIFT_SIZE]);

        if (!serviceOptions.ContainsKey(SERVICE_OPTION_MODEL_NAME))
            throw new BadRequestException($"Missing required service option: {SERVICE_OPTION_MODEL_NAME}");
        
        modelName = serviceOptions[SERVICE_OPTION_MODEL_NAME];
    }

    public IServiceContext CreateServiceContext(DeviceEntity device, Dictionary<string, string> serviceOptions)
    {
        if (device.Type != DeviceEntity.DeviceTypes.Radar)
            throw new Exception("Unsupported device passed to service.");

        Radar radar = (Radar) device;
        GetServiceSettings(serviceOptions, out string modelName, out int gaitWindowShitSize, out int minRequiredHitCount, out int majorityWindowSize);
        GateIdContext gateIdContext = new GateIdContext(radar, modelName, REQUIRED_WINDOW_SIZE, gaitWindowShitSize, minRequiredHitCount, majorityWindowSize);
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
