using WebService.Entites;

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
        registeredServices = new List<RegisteredService>();
    }

    #endregion

    private List<RegisteredService> registeredServices;

    public void RegisterServices(List<RegisteredService> registeredServices)
    {
        this.registeredServices = registeredServices;

        foreach (var registerdService in registeredServices)
        {

        }
    }

    public bool ServiceExist(string serviceId)
    {
        return this.registeredServices.Exists(service => service.Id == serviceId);
    }

    public void InitServiceContext(RadarDevice.LinkedService linkedService)
    {
        //if (linkedService.ServiceId)
    }
}