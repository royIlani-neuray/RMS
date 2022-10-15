using WebService.Entites;
using WebService.Radar;

namespace WebService.Actions.Radar;

public class SetRadarConfigAction : RadarDeviceAction 
{
    private List<string> configScript;
    public SetRadarConfigAction(string deviceId, List<string> configScript) : base(deviceId) 
    {
        this.configScript = configScript;
    }

    protected override void RunDeviceAction(RadarDevice radarDevice)
    {
        Console.WriteLine($"Setting radar config for device - {deviceId}");

        try
        {
            RadarConfigParser configParser = new RadarConfigParser(configScript);
            radarDevice.radarSettings = configParser.GetRadarSettings();
        }
        catch
        {
            throw new Exception($"Error: could not parse config script for device - {deviceId}. make sure the config is valid.");
        }

        radarDevice.ConfigScript = configScript;

        var disconnectAction = new DisconnectRadarAction(radarDevice);
        disconnectAction.Run();

        var connectAction = new ConnectRadarAction(radarDevice);
        connectAction.Run(); 
    }
}