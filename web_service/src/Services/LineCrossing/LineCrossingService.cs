/***
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using System.Text.Json;
using WebService.Entites;
using WebService.RadarLogic.Streaming;

namespace WebService.Services.LineCrossing;

public class LineCrossingService : IExtensionService
{
    private const string SERVICE_ID = "LINE_CROSSING";

    private const string SERVICE_OPTION_LINES = "lines";

    public string ServiceId => SERVICE_ID;

    public List<DeviceEntity.DeviceTypes> SupportedDeviceTypes => [DeviceEntity.DeviceTypes.Radar];

    public ExtensionServiceSettings? Settings { get; set; }

    
    private void GetServiceSettings(Dictionary<string, string> serviceOptions, out List<LineSettings> lineSettingsList)
    {
        if (!serviceOptions.TryGetValue(SERVICE_OPTION_LINES, out string? linesJson))
            throw new BadRequestException($"Missing required service option: {SERVICE_OPTION_LINES}");
        
        lineSettingsList = JsonSerializer.Deserialize<List<LineSettings>>(linesJson) ?? throw new BadRequestException($"Invalid line settings provided!");
    }


    public IServiceContext CreateServiceContext(DeviceEntity device, Dictionary<string, string> serviceOptions)
    {
        if (device.Type != DeviceEntity.DeviceTypes.Radar)
            throw new Exception("Unsupported device passed to service.");

        Radar radar = (Radar) device;
        
        GetServiceSettings(serviceOptions, out List<LineSettings> lineSettingsList);
        
        var lineCrossingContext = new LineCrossingContext(radar, lineSettingsList);
        lineCrossingContext.StartWorker();
        lineCrossingContext.State = IServiceContext.ServiceState.Active;
        return lineCrossingContext;
    }

    public void DisposeServiceContext(IServiceContext serviceContext)
    {
        LineCrossingContext lineCrossingContext = (LineCrossingContext) serviceContext;
        lineCrossingContext.StopWorker();
        lineCrossingContext.State = IServiceContext.ServiceState.Initialized;
    }

    public void RunService(object dataObject, IServiceContext serviceContext)
    {
        if (dataObject is not FrameData)
            return;

        FrameData frame = (FrameData) dataObject;
        LineCrossingContext lineCrossingContext = (LineCrossingContext) serviceContext;
        lineCrossingContext.HandleFrame(frame);
    }
}
