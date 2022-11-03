using WebService.Entites;
using WebService.Radar;
using WebService.Context;
using WebService.Utils;
using System.Text.Json.Serialization;


namespace WebService.Actions.Radar;

public class SetRadarConfigArgs
{
    [JsonPropertyName("config")]
    public List<string>? Config { get; set; }

    [JsonPropertyName("template_id")]
    public string? TemplateId { get; set; }

    [JsonPropertyName("sensor_position")]
    public RadarSettings.SensorPositionParams? SensorPosition {get; set;} = null;

    public void Validate()
    {
        if ((Config == null) && (TemplateId == null))
            throw new BadRequestException("missing radar configuration / template id");

        if ((TemplateId != null) && (SensorPosition == null))
            throw new BadRequestException("sensor position must be provided with radar config");
    }
}

public class SetRadarConfigAction : RadarDeviceAction 
{
    private SetRadarConfigArgs args;
    public SetRadarConfigAction(string deviceId, SetRadarConfigArgs args) : base(deviceId) 
    {
        this.args = args;
    }

    protected override void RunDeviceAction(RadarDevice radarDevice)
    {
        List<string> configScript;
        Console.WriteLine($"Setting radar config for device - {deviceId}");

        if (!string.IsNullOrWhiteSpace(args.TemplateId))
        {
            var template = TemplateContext.Instance.GetTemplate(args.TemplateId);
            configScript = new List<string>(template.ConfigScript);
            ConfigScriptUtils.UpdateSensorPosition(configScript, args.SensorPosition!);
        }
        else
        {
            configScript = args.Config!;
        }

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