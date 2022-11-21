using WebService.Entites;
using WebService.Services;
using System.Text.Json.Serialization;

namespace WebService.Actions.Services;

public class LinkServiceArgs
{
    [JsonPropertyName("service_id")]
    public string ServiceId { get; set; } = String.Empty;

    [JsonPropertyName("service_options")]
    public Dictionary<string,string> ServiceOptions { get; set; } = new Dictionary<string, string>();

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(ServiceId))
            throw new BadRequestException("service id wasn't provided.");
    }
}

public class LinkServiceAction : RadarDeviceAction 
{
    private LinkServiceArgs args;

    public LinkServiceAction(string deviceId, LinkServiceArgs args) : base(deviceId) 
    {
        this.args = args;
    }

    protected override void RunDeviceAction(RadarDevice radarDevice)
    {
        var alreadyLinked = radarDevice.LinkedServices.Exists(linkedService => linkedService.ServiceId == args.ServiceId);

        if (alreadyLinked)
            throw new Exception($"The service is already linked to this device.");        

        if (!ServiceManager.Instance.ServiceExist(args.ServiceId))
            throw new Exception($"Cannot find service with the provided id.");

        var linkedService = new RadarDevice.LinkedService() 
        {
            ServiceId = args.ServiceId,
            ServiceOptions = args.ServiceOptions
        };

        if (radarDevice.State == RadarDevice.DeviceState.Active)
        {
            ServiceManager.Instance.InitServiceContext(radarDevice, linkedService);
        }

        radarDevice.LinkedServices.Add(linkedService);
    }
}