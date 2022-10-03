using WebService.Entites;
using WebService.Radar;

namespace WebService.Actions.Radar;

public class EnableRadarAction : RadarDeviceAction 
{
    public EnableRadarAction(string deviceId) : base(deviceId) {}

    protected override void RunDeviceAction(RadarDevice radarDevice)
    {
        System.Console.WriteLine($"Enabling radar device - {deviceId}");

        if (radarDevice.Enabled)
            return; // nothing to do.
        
        radarDevice.Enabled = true;

        var connectAction = new ConnectRadarAction(radarDevice);
        connectAction.Run();
    }
}