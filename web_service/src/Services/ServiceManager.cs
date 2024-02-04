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
using WebService.Services.Inference.GateId;
using WebService.Services.Inference.HumanDetection;
using WebService.Services.FallDetection;
using WebService.Services.Inference.SmartFanGestures;

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
                    if (instance == null)
                        instance = new ServiceManager();
                }
            }

            return instance;
        }
    }

    private ServiceManager() 
    {
        servicesSettings = new List<ExtensionServiceSettings>();
        services = new List<IExtensionService>();
    }

    #endregion

    private List<ExtensionServiceSettings> servicesSettings;

    private List<IExtensionService> services;

    public void InitExtensionServices(List<ExtensionServiceSettings>? servicesSettings)
    {
        if (servicesSettings == null)
        {
            servicesSettings = new List<ExtensionServiceSettings>();
        }
        
        this.servicesSettings = servicesSettings;

        services.Add(new RadarRecordingService());
        services.Add(new GateIdService());
        services.Add(new HumanDetectionService());
        services.Add(new CameraRecordingService());
        services.Add(new FallDetectionService());
        services.Add(new SmartFanService());

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
                System.Console.WriteLine($"{device.LogTag} Error: could not initialize service context for service: {linkedService.ServiceId}");
                System.Console.WriteLine($"{device.LogTag} Error: {ex.Message}");
                throw;
            }
        }
    }

    public void InitServiceContext(DeviceEntity device, DeviceEntity.LinkedService linkedService)
    {
        IExtensionService service = services.First(service => service.ServiceId == linkedService.ServiceId);

        if (service.Settings!.Enabled)
        {
            Console.WriteLine($"{device.LogTag} Creating {service.ServiceId} context.");

            try
            {
                linkedService.ServiceContext = service.CreateServiceContext(device, linkedService.ServiceOptions);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{device.LogTag} Error: failed to create service context. exception:\n{ex}");
                throw;
            }
        }
        else
        {
            linkedService.ServiceContext = null;
        }
    }

    public void DisposeServiceContext(string deviceId, DeviceEntity.LinkedService linkedService)
    {
        if (linkedService.ServiceContext == null)
            return;

        IExtensionService service = services.First(service => service.ServiceId == linkedService.ServiceId);
        System.Console.WriteLine($"[{deviceId}] Disposing {service.ServiceId} context.");
        service.DisposeServiceContext(linkedService.ServiceContext!);
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
            catch
            {
                System.Console.WriteLine($"{device.LogTag} Error: unexpected error while running service: {linkedService.ServiceId}");
            }
        }
    }
}