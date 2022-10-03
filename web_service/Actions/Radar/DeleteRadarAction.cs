using WebService.Entites;
using WebService.Context;

namespace WebService.Actions.Radar;

public class DeleteRadarAction : RadarDeviceAction 
{
    public DeleteRadarAction(string deviceId) : base(deviceId) {}

    protected override void RunDeviceAction(RadarDevice radarDevice)
    {
        System.Console.WriteLine($"Deleting radar device - {deviceId}");

        var disconnectAction = new DisconnectRadarAction(radarDevice);
        disconnectAction.Run();

        DeviceContext.Instance.DeleteDevice(radarDevice);
    }
}