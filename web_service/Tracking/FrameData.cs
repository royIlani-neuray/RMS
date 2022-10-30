
using System.Text.Json.Serialization;

namespace WebService.Tracking;

public class FrameData 
{
    public class Point
    {
        [JsonPropertyName("range")]
        public float Range { get; set; }

        [JsonPropertyName("azimuth")]
        public float Azimuth { get; set; }

        [JsonPropertyName("elevation")]
        public float Elevation { get; set; }

        [JsonPropertyName("doppler")]
        public float Doppler { get; set; }
    }

    public class Track
    {
        [JsonPropertyName("track_id")]
        public uint TrackId { get; set; }

        [JsonPropertyName("position_x")]
        public float PositionX { get; set; }

        [JsonPropertyName("position_y")]
        public float PositionY { get; set; }

        [JsonPropertyName("position_z")]
        public float PositionZ { get; set; }

        [JsonPropertyName("velocity_x")]
        public float VelocityX { get; set; }

        [JsonPropertyName("velocity_y")]
        public float VelocityY { get; set; }

        [JsonPropertyName("velocity_z")]
        public float VelocityZ { get; set; }

        [JsonPropertyName("acceleration_x")]
        public float AccelerationX { get; set; }

        [JsonPropertyName("acceleration_y")]
        public float AccelerationY { get; set; }

        [JsonPropertyName("acceleration_z")]
        public float AccelerationZ { get; set; }
    }

    [JsonPropertyName("points")]
    public List<Point> pointsList = new List<Point>();

    [JsonPropertyName("targets_index")]
    public List<byte> targetsIndexList = new List<byte>();

    [JsonPropertyName("device_id")]
    public string DeviceId { get; set; } = String.Empty;
    
    [JsonPropertyName("device_name")]
    public string DeviceName { get; set; } = String.Empty;
    
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    [JsonPropertyName("tracks")]
    public List<Track> tracksList { get; set; } = new List<Track>();
}