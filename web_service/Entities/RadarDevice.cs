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

    [JsonPropertyName("status")]
    public String Status { get; set; }

    [JsonPropertyName("name")]
    public String Name { get; set; }

    [JsonPropertyName("description")]
    public String Description { get; set; }

    [JsonPropertyName("device_id")]
    public String Id { get; set; }
    
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; }

    [JsonPropertyName("send_tracks_report")]
    public bool SendTracksReport { get; set; }

    [JsonPropertyName("config_script")]
    public List<string> ConfigScript { get; set; }

    [JsonPropertyName("device_mapping")]
    public DeviceMapper.MappedDevice? deviceMapping { get; set;}

    [JsonPropertyName("radar_settings")]
    public RadarSettings? radarSettings { get; set;}

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

        [JsonPropertyName("send_tracks_report")]
        public bool SendTracksReport { get; set; }

        public RadarDeviceBrief(RadarDevice device)
        {
            State = device.State;
            Name = device.Name;
            Description = device.Description;
            Id = device.Id;
            Enabled = device.Enabled;
            SendTracksReport = device.SendTracksReport;
        }
    }

    public RadarDevice()
    {
        deviceLock = new ReaderWriterLockSlim();
        Name = String.Empty;
        Description = String.Empty;
        Id = String.Empty;
        Enabled = false;
        State = DeviceState.Disconnected;
        Status = String.Empty;
        ConfigScript = new List<string>();
    }

    public void SetStatus(string status)
    {
        this.Status = status;
        System.Console.WriteLine($"[{Id}] {status}");
    }
}