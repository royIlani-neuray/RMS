using System.Text.Json.Serialization;
using WebService.Entites;
using WebService.Radar;

namespace WebService.Actions.Radar;

public class UpdateRadarInfoArgs
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = String.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = String.Empty;

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Name))
            throw new HttpRequestException("Missing radar name.");           
    }
}

public class UpdateRadarInfoAction : RadarDeviceAction 
{
    private UpdateRadarInfoArgs args;

    public UpdateRadarInfoAction(string deviceId, UpdateRadarInfoArgs args) : base(deviceId) 
    {
        this.args = args;
    }

    protected override void RunDeviceAction(RadarDevice radarDevice)
    {
        System.Console.WriteLine($"Updating radar info - {deviceId}");

        radarDevice.Name = args.Name;
        radarDevice.Description = args.Description;
    }
}