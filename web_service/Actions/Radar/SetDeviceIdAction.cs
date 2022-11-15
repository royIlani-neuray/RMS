using WebService.Radar;
using WebService.Context;
using System.Text.Json.Serialization;

namespace WebService.Actions.Radar;

public class SetDeviceIdArgs
{
    [JsonPropertyName("new_device_id")]
    public string NewDeviceId { get; set; } = String.Empty;

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(NewDeviceId))
            throw new BadRequestException("new_device_id wasn't provided.");

        if (!Guid.TryParse(NewDeviceId, out _))
            throw new BadRequestException("new_device_id must be a valid Guid format.");
    }
}

public class SetDeviceIdAction : IAction {

    private SetDeviceIdArgs args;
    private string deviceId;

    public SetDeviceIdAction(string deviceId, SetDeviceIdArgs args)
    {
        this.args = args;
        this.deviceId = deviceId;
    }

    public void Run()
    {
        if (DeviceMapper.Instance.IsDeviceHasMapping(args.NewDeviceId))
            throw new Exception("The new device id provided is already exist for another device in the network.");

        if (DeviceContext.Instance.IsRadarDeviceExist(args.NewDeviceId))
            throw new Exception("The new device id provided is already registerd in RMS by another device.");

        var mappedDevice = DeviceMapper.Instance.GetMappedDevice(deviceId); 

        System.Console.WriteLine($"Updating device id. Current id: [{deviceId}], New id: [{args.NewDeviceId}] ...");    
        IPRadarClient client = new IPRadarClient(mappedDevice.ipAddress);
        client.Connect();

        client.SetDeviceId(deviceId, args.NewDeviceId);
    
        client.Disconnect();
    }

}