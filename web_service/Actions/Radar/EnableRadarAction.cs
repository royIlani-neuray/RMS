using WebService.Entites;

namespace WebService.Actions.Radar;

public class EnableRadarAction : RadarDeviceAction 
{
    public EnableRadarAction(string deviceId) : base(deviceId) {}

    protected override void RunDeviceAction(RadarDevice radarDevice)
    {
        if (radarDevice.Enabled)
            return; // nothing to do.
        
        radarDevice.SetStatus("Enabling radar device...");
        radarDevice.Enabled = true;

        var connectAction = new ConnectRadarAction(radarDevice);
        connectAction.Run();
    }
}