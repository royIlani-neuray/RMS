using System.Text.Json.Serialization;

public class RadarDeviceBrief 
{
    public enum DeviceState {
        Disconnected,
        Connected,
        Active
    };
    
    [JsonPropertyName("state")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public DeviceState? State { get; set; }

    [JsonPropertyName("name")]
    public String Name { get; set; }

    [JsonPropertyName("description")]
    public String Description { get; set; }

    [JsonPropertyName("device_id")]
    public String Id { get; set; }

    [JsonPropertyName("enabled")]
    public bool Enabled {get; set; }

    [JsonPropertyName("send_tracks_report")]
    public bool SendTracksReport { get; set; }

    public RadarDeviceBrief()
    {
        Name = String.Empty;
        Description = String.Empty;
        Id = String.Empty;
    }
}