using WebService.Entites;
using WebService.Services;

namespace WebService.Actions.Services;

public class UnlinkServiceAction : RadarDeviceAction 
{
    private string serviceId;

    public UnlinkServiceAction(string deviceId, string serviceId) : base(deviceId) 
    {
        this.serviceId = serviceId;
    }

    protected override void RunDeviceAction(RadarDevice radarDevice)
    {
        var linkedService = radarDevice.LinkedServices.FirstOrDefault(linkedService => linkedService.ServiceId == serviceId);

        if (linkedService == null)
            throw new Exception($"Could not find linked service with id - {serviceId}");
        
        ServiceManager.Instance.DisposeServiceContext(linkedService);
        radarDevice.LinkedServices.Remove(linkedService);
    }
}