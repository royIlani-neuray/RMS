/***
** Copyright (C) 2020-2023 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using WebService.Entites;
using WebService.RadarLogic.Tracking;
using System.Text.Json;

namespace WebService.Services.Inference.SmartFanGestures;

public class SmartFanService : IExtensionService
{
    private const string SERVICE_ID = "SMART_FAN_GESTURES";

    private const string SERVICE_OPTION_MODEL_NAME = "smart_fan_model";

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
        
        SmartFanContext smartFanContext = new SmartFanContext(radar, modelName);
        smartFanContext.StartWorker();
        smartFanContext.State = IServiceContext.ServiceState.Active;
        return smartFanContext;
    }

    public void DisposeServiceContext(IServiceContext serviceContext)
    {
        SmartFanContext smartFanContext = (SmartFanContext) serviceContext;
        smartFanContext.StopWorker();
        smartFanContext.State = IServiceContext.ServiceState.Initialized;
    }

    public void RunService(object dataObject, IServiceContext serviceContext)
    {
        if (dataObject is not FrameData)
            return;

        FrameData frame = (FrameData) dataObject;
        SmartFanContext smartFanContext = (SmartFanContext) serviceContext;
        smartFanContext.HandleFrame(frame);
    }
}
