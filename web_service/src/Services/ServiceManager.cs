/***
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using WebService.Entites;
using WebService.Services.RadarRecording;
using WebService.Services.CameraRecording;
using WebService.Services.Inference.GateId;
using WebService.Services.Inference.HumanDetection;
using WebService.Services.FallDetection;
using WebService.Services.Inference.SmartFanGestures;
using WebService.Services.LineCrossing;

namespace WebService.Services;

public sealed class ServiceManager {

    #region Singleton
    
    private static object singletonLock = new object();
    private static volatile ServiceManager? instance; 

    public static ServiceManager Instance {
        get 
        {
            if (instance == null)
            {
                lock (singletonLock)
                {
                    instance ??= new ServiceManager();
                }
            }

            return instance;
        }
    }

    private ServiceManager() 
    {
        servicesSettings = [];
        services = [];
    }

    #endregion

    private List<ExtensionServiceSettings> servicesSettings;

    private List<IExtensionService> services;

    public void InitExtensionServices(List<ExtensionServiceSettings>? servicesSettings)
    {
        servicesSettings ??= [];
        
        this.servicesSettings = servicesSettings;

        services.Add(new RadarRecordingService());
        services.Add(new GateIdService());
        services.Add(new HumanDetectionService());
        services.Add(new CameraRecordingService());
        services.Add(new FallDetectionService());
        services.Add(new SmartFanService());
        services.Add(new LineCrossingService());

        foreach (var service in services)
        {
            service.Settings = servicesSettings.FirstOrDefault(settings => settings.Id == service.ServiceId);

            if (service.Settings == null)
                throw new Exception($"Error: Missing service settings for service: {service.ServiceId}");
        }
    }

    public bool ServiceExist(string serviceId)
    {
        return services.Exists(service => service.ServiceId == serviceId);
    }

    public bool IsDeviceSupportedByService(string serviceId, DeviceEntity device)
    {
        var service = services.First(service => service.ServiceId == serviceId);
        return service.SupportedDeviceTypes.Contains(device.Type);
    }


    public void InitDeviceServices(DeviceEntity device)
    {
        foreach (var linkedService in device.LinkedServices)
        {
            try
            {
                ServiceManager.Instance.InitServiceContext(device, linkedService);
            }
            catch (Exception ex)
            {
                device.Log.Error($"could not initialize service context for service: {linkedService.ServiceId}", ex);
                throw;
            }
        }
    }

    public void InitServiceContext(DeviceEntity device, DeviceEntity.LinkedService linkedService)
    {
        IExtensionService service = services.First(service => service.ServiceId == linkedService.ServiceId);

        if (service.Settings!.Enabled)
        {
            if (linkedService.ServiceContext != null)
                return;

            device.Log.Information($"Creating {service.ServiceId} context.");

            try
            {
                linkedService.ServiceContext = service.CreateServiceContext(device, linkedService.ServiceOptions);
            }
            catch (Exception ex)
            {
                device.Log.Error("Error: failed to create service context.", ex);
                throw;
            }
        }
        else
        {
            DisposeServiceContext(device, linkedService);
        }
    }

    public void DisposeServiceContext(DeviceEntity device, DeviceEntity.LinkedService linkedService)
    {
        if (linkedService.ServiceContext == null)
            return;

        IExtensionService service = services.First(service => service.ServiceId == linkedService.ServiceId);
        device.Log.Information($"Disposing {service.ServiceId} context.");
        
        try
        {
            service.DisposeServiceContext(linkedService.ServiceContext!);
        }
        catch (Exception ex)
        {
            device.Log.Error($"failed to dispose {service.ServiceId} context.", ex);
            throw;
        }
        finally
        {
            linkedService.ServiceContext = null;
        }
    }

    public void RunServices(DeviceEntity device, object dataObject)
    {
        foreach (var linkedService in device.LinkedServices)
        {
            try
            {
                IExtensionService service = services.First(service => service.ServiceId == linkedService.ServiceId);

                if ((!service.Settings!.Enabled) || (linkedService.ServiceContext == null))
                    continue;

                service.RunService(dataObject, linkedService.ServiceContext);
            }
            catch (Exception ex)
            {
                device.Log.Error($"unexpected error while running service: {linkedService.ServiceId}", ex);
            }
        }
    }
}