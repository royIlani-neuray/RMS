/***
** Copyright (C) 2020-2023 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
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

        [JsonPropertyName("snr")]
        public float SNR { get; set; }

        [JsonPropertyName("position_x")]
        public float PositionX { get; set; }       

        [JsonPropertyName("position_y")]
        public float PositionY { get; set; }       

        [JsonPropertyName("position_z")]
        public float PositionZ { get; set; }       
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
    public List<Point> PointsList { get; set; } = new List<Point>();

    [JsonPropertyName("targets_index")]
    public List<byte> TargetsIndexList { get; set; } = new List<byte>();

    [JsonPropertyName("device_id")]
    public string DeviceId { get; set; } = String.Empty;
    
    [JsonPropertyName("device_name")]
    public string DeviceName { get; set; } = String.Empty;
    
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    [JsonPropertyName("tracks")]
    public List<Track> tracksList { get; set; } = new List<Track>();
}