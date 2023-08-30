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

namespace WebService.Services.FallDetection;

public class FallDetectionService : IExtensionService
{
    private const string SERVICE_ID = "FALL_DETECTION";

    private const string SERVICE_OPTION_FALL_THRESHOLD = "falling_threshold";
    private const string SERVICE_OPTION_MIN_TRACKING_DURATION = "min_tracking_duration_seconds";
    private const string SERVICE_OPTION_MAX_TRACKING_DURATION = "max_tracking_duration_seconds";
    private const string SERVICE_OPTION_ALERT_DURATION = "alert_duration_seconds";

    public string ServiceId => SERVICE_ID;

    public List<DeviceEntity.DeviceTypes> SupportedDeviceTypes => new List<DeviceEntity.DeviceTypes>() { DeviceEntity.DeviceTypes.Radar };

    public ExtensionServiceSettings? Settings { get; set; }

    private void GetServiceSettings(Dictionary<string, string> serviceOptions, out float fallingThreshold, 
                                    out float minTrackingDurationSeconds, out float maxTrackingDurationSeconds, out float alertDurationSeconds)
    {
        if (!serviceOptions.ContainsKey(SERVICE_OPTION_FALL_THRESHOLD))
            throw new BadRequestException($"Missing required service option: {SERVICE_OPTION_FALL_THRESHOLD}");
        
        if (!float.TryParse(serviceOptions[SERVICE_OPTION_FALL_THRESHOLD], out fallingThreshold))
            throw new BadRequestException($"Invalid Fall Detection threshold provided!");

        if ((fallingThreshold < 0) || (fallingThreshold > 1))
            throw new BadRequestException($"Fall Detection threshold must be between 0 to 1");

        if (!serviceOptions.ContainsKey(SERVICE_OPTION_MIN_TRACKING_DURATION))
            throw new BadRequestException($"Missing required service option: {SERVICE_OPTION_MIN_TRACKING_DURATION}");

        if (!float.TryParse(serviceOptions[SERVICE_OPTION_MIN_TRACKING_DURATION], out minTrackingDurationSeconds))
            throw new BadRequestException($"Invalid value for '{SERVICE_OPTION_MIN_TRACKING_DURATION}' provided!");

        if (!serviceOptions.ContainsKey(SERVICE_OPTION_MAX_TRACKING_DURATION))
            throw new BadRequestException($"Missing required service option: {SERVICE_OPTION_MAX_TRACKING_DURATION}");

        if (!float.TryParse(serviceOptions[SERVICE_OPTION_MAX_TRACKING_DURATION], out maxTrackingDurationSeconds))
            throw new BadRequestException($"Invalid value for '{SERVICE_OPTION_MAX_TRACKING_DURATION}' provided!");

        if (!serviceOptions.ContainsKey(SERVICE_OPTION_ALERT_DURATION))
            throw new BadRequestException($"Missing required service option: {SERVICE_OPTION_ALERT_DURATION}");

        if (!float.TryParse(serviceOptions[SERVICE_OPTION_ALERT_DURATION], out alertDurationSeconds))
            throw new BadRequestException($"Invalid value for '{SERVICE_OPTION_ALERT_DURATION}' provided!");
    }

    public IServiceContext CreateServiceContext(DeviceEntity device, Dictionary<string, string> serviceOptions)
    {
        if (device.Type != DeviceEntity.DeviceTypes.Radar)
            throw new Exception("Unsupported device passed to service.");

        Radar radar = (Radar) device;
        GetServiceSettings(serviceOptions, out float fallingThreshold, out float minTrackingDurationSeconds, out float maxTrackingDurationSeconds, out float alertDurationSeconds);
        
        FallDetectionContext fallDetectionContext = new FallDetectionContext(radar, fallingThreshold, minTrackingDurationSeconds, maxTrackingDurationSeconds, alertDurationSeconds);
        fallDetectionContext.StartWorker();
        fallDetectionContext.State = IServiceContext.ServiceState.Active;
        return fallDetectionContext;
    }

    public void DisposeServiceContext(IServiceContext serviceContext)
    {
        FallDetectionContext fallDetectionContext = (FallDetectionContext) serviceContext;
        fallDetectionContext.StopWorker();
        fallDetectionContext.State = IServiceContext.ServiceState.Initialized;
    }

    public void RunService(object dataObject, IServiceContext serviceContext)
    {
        if (dataObject is not FrameData)
            return;

        FrameData frame = (FrameData) dataObject;
        FallDetectionContext fallDetectionContext = (FallDetectionContext) serviceContext;
        fallDetectionContext.HandleFrame(frame);
    }
}
