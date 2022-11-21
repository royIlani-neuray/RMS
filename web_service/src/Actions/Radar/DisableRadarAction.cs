using WebService.Entites;
using WebService.Radar;

namespace WebService.Actions.Radar;

public class DisableRadarAction : RadarDeviceAction 
{
    public DisableRadarAction(string deviceId) : base(deviceId) {}

    protected override void RunDeviceAction(RadarDevice radarDevice)
    {
        if (!radarDevice.Enabled)
            return; // nothing to do.

        radarDevice.SetStatus("Disabling radar device...");

        var disconnectAction = new DisconnectRadarAction(radarDevice);
        disconnectAction.Run();

        radarDevice.Enabled = false;
        radarDevice.SetStatus("The device is disabled.");
    }
}