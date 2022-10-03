using WebService.Entites;
using WebService.Radar;

namespace WebService.Actions.Radar;

public class DisableRadarAction : RadarDeviceAction 
{
    public DisableRadarAction(string deviceId) : base(deviceId) {}

    protected override void RunDeviceAction(RadarDevice radarDevice)
    {
        System.Console.WriteLine($"Disabling radar device - {deviceId}");

        if (!radarDevice.Enabled)
            return; // nothing to do.

        var disconnectAction = new DisconnectRadarAction(radarDevice);
        disconnectAction.Run();

        radarDevice.Enabled = false;
    }
}