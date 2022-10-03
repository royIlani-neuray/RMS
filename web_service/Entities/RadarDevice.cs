using System.Text.Json.Serialization;
using WebService.Radar;
using WebService.Tracking;

namespace WebService.Entites;


public class RadarDevice {

    public enum DeviceState {
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

    [JsonPropertyName("device_id")]
    public String Id { get; set; }
    
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; }

    [JsonPropertyName("config")]
    public List<string> ConfigScript { get; set; }

    [JsonPropertyName("device_mapping")]
    public DeviceMapper.MappedDevice? deviceMapping { get; set;}

    [JsonIgnore]
    public ReaderWriterLockSlim deviceLock;

    [JsonIgnore]
    public IPRadarClient? ipRadarClient;

    [JsonIgnore]
    public RadarTracker? radarTracker;

    public class RadarDeviceBrief 
    {
        [JsonPropertyName("state")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public DeviceState State { get; set; }

        [JsonPropertyName("name")]
        public String Name { get; set; }

        [JsonPropertyName("description")]
        public String Description { get; set; }

        [JsonPropertyName("device_id")]
        public String Id { get; set; }

        [JsonPropertyName("enabled")]
        public bool Enabled {get; set; }

        public RadarDeviceBrief(RadarDevice device)
        {
            State = device.State;
            Name = device.Name;
            Description = device.Description;
            Id = device.Id;
            Enabled = device.Enabled;
        }
    }

    public RadarDevice()
    {
        deviceLock = new ReaderWriterLockSlim();
        Name = "";
        Description = "";
        Id = "";
        Enabled = false;
        State = DeviceState.Disconnected;
        ConfigScript = new List<string>();
    }
}