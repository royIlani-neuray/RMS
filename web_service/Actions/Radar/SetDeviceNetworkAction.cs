using WebService.Radar;
using System.Text.Json.Serialization;
using System.Net;

namespace WebService.Actions.Radar;


public class SetDeviceNetworkArgs
{
    [JsonPropertyName("ip")]
    public string ipAddress { get; set; } = String.Empty;

    [JsonPropertyName("subnet")]
    public string subnetMask { get; set; } = String.Empty;

    [JsonPropertyName("gateway")]
    public string gwAddress { get; set; } = String.Empty;

    [JsonPropertyName("static_ip")]
    public bool? staticIP { get; set; }


    public void Validate()
    {
        if (staticIP == null)
            throw new BadRequestException("static IP option not provided");

        if (staticIP == true)
        {
            if (string.IsNullOrWhiteSpace(ipAddress) || !IPAddress.TryParse(ipAddress, out _))
                throw new BadRequestException("invalid IP provided");
            if (string.IsNullOrWhiteSpace(subnetMask) || !IPAddress.TryParse(subnetMask, out _))
                throw new BadRequestException("invalid subnet provided");
            if (string.IsNullOrWhiteSpace(gwAddress) || !IPAddress.TryParse(gwAddress, out _))
                throw new BadRequestException("invalid gateway address provided");
        }
    }
}

public class SetDeviceNetworkAction : IAction {

    private SetDeviceNetworkArgs args;
    private string deviceId;

    public SetDeviceNetworkAction(string deviceId, SetDeviceNetworkArgs args)
    {
        this.args = args;
        this.deviceId = deviceId;
    }

    public void Run()
    {
        IPRadarClient.SetDeviceNetwork(deviceId, args.ipAddress, args.subnetMask, args.gwAddress, args.staticIP!.Value);
    }

}