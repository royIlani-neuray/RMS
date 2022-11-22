using WebService.Entites;
using WebService.Tracking;
using WebService.Services.Recording;

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
        servicesSettings = new List<RadarServiceSettings>();
        services = new List<IRadarService>();
    }

    #endregion

    private List<RadarServiceSettings> servicesSettings;

    private List<IRadarService> services;

    public void InitServices(List<RadarServiceSettings>? servicesSettings)
    {
        if (servicesSettings == null)
        {
            servicesSettings = new List<RadarServiceSettings>();
        }
        
        this.servicesSettings = servicesSettings;

        services.Add(new RecordingService());

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

    public void InitServiceContext(RadarDevice device, RadarDevice.LinkedService linkedService)
    {
        IRadarService service = services.First(service => service.ServiceId == linkedService.ServiceId);

        if (service.Settings!.Enabled)
        {
            linkedService.ServiceContext = service.CreateServiceContext(device, linkedService.ServiceOptions);
        }
        else
        {
            linkedService.ServiceContext = null;
        }
    }

    public void DisposeServiceContext(RadarDevice.LinkedService linkedService)
    {
        if (linkedService.ServiceContext == null)
            return;

        IRadarService service = services.First(service => service.ServiceId == linkedService.ServiceId);
        service.DisposeServiceContext(linkedService.ServiceContext!);
    }

    public void HandleFrame(FrameData frame, List<RadarDevice.LinkedService> linkedServices)
    {
        foreach (var linkedService in linkedServices)
        {
            try
            {
                IRadarService service = services.First(service => service.ServiceId == linkedService.ServiceId);

                if ((!service.Settings!.Enabled) || (linkedService.ServiceContext == null))
                    continue;

                service.HandleFrame(frame, linkedService.ServiceContext);
            }
            catch
            {
                System.Console.WriteLine($"[{frame.DeviceId}] Error: unexpected error while handling frame data in service: {linkedService.ServiceId}");
            }
        }
    }
}