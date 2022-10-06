
using System.Text.Json.Serialization;

namespace WebService.Tracking;

public class FrameData 
{
    public class Point
    {
        public float Range;
        public float Azimuth;
        public float Elevation;
        public float Doppler;
    }

    public class Track
    {
        [JsonPropertyName("track_id")]
        public uint TrackId { get; set; }

        [JsonPropertyName("position-x")]
        public float PositionX { get; set; }

        [JsonPropertyName("position-y")]
        public float PositionY { get; set; }

        [JsonPropertyName("position-z")]
        public float PositionZ { get; set; }

        [JsonPropertyName("velocity-x")]
        public float VelocityX { get; set; }

        [JsonPropertyName("velocity-y")]
        public float VelocityY { get; set; }

        [JsonPropertyName("velocity-z")]
        public float VelocityZ { get; set; }

        [JsonPropertyName("acceleration-x")]
        public float AccelerationX { get; set; }

        [JsonPropertyName("acceleration-y")]
        public float AccelerationY { get; set; }

        [JsonPropertyName("acceleration-z")]
        public float AccelerationZ { get; set; }
    }

    [JsonIgnore]
    public List<Point> pointsList = new List<Point>();

    [JsonPropertyName("device_id")]
    public string DeviceId { get; set; } = String.Empty;
    
    [JsonPropertyName("device_name")]
    public string DeviceName { get; set; } = String.Empty;
    
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    [JsonPropertyName("tracks")]
    public List<Track> tracksList { get; set; } = new List<Track>();
}