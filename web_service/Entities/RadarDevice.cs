using System.Text.Json.Serialization;

namespace WebService.Entites;


public class RadarDevice {

    public enum DeviceState {
        Unknown,
        Disconnected,
        Connected,
        Active
    };

    [JsonPropertyName("state")]

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public DeviceState State { get; set; }

    [JsonPropertyName("name")]
    public String Name { get; set; }

    [JsonPropertyName("description")]
    public String Description { get; set; }

    [JsonPropertyName("model")]
    public String Model { get; set; }

    [JsonPropertyName("device_id")]
    public String Id { get; set; }
    
    [JsonPropertyName("enabled")]
    public bool Enabled {get; set; }

    public class RadarDeviceBrief 
    {
        [JsonPropertyName("state")]

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public DeviceState State { get; set; }
        [JsonPropertyName("name")]
        public String Name { get; set; }
        [JsonPropertyName("description")]
        public String Description { get; set; }
        [JsonPropertyName("model")]
        public String Model { get; set; }
        [JsonPropertyName("device_id")]
        public String Id { get; set; }
        [JsonPropertyName("enabled")]
        public bool Enabled {get; set; }

        public RadarDeviceBrief(RadarDevice device)
        {
            State = device.State;
            Name = device.Name;
            Model = device.Model;
            Description = device.Description;
            Id = device.Id;
            Enabled = device.Enabled;
        }
    }

    public RadarDevice()
    {
        Name = "";
        Description = "";
        Model = "";
        Id = "";
        Enabled = false;
        State = DeviceState.Unknown;
    }
}