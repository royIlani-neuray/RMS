using WebService.Entites;
using WebService.Radar;

namespace WebService.Actions.Radar;

public class ReconnectAction : RadarDeviceAction 
{
    public ReconnectAction(string deviceId) : base(deviceId) {}

    protected override void RunDeviceAction(RadarDevice radarDevice)
    {
        if (!radarDevice.Enabled)
            return;

        if (radarDevice.State != RadarDevice.DeviceState.Disconnected)
            return;

        var connectAction = new ConnectRadarAction(radarDevice);
        connectAction.Run();
    }
}