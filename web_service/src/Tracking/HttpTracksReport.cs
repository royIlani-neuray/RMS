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

public class HttpTracksReport 
{
    [JsonPropertyName("device_id")]
    public string DeviceId { get; set; } = String.Empty;
    
    [JsonPropertyName("device_name")]
    public string DeviceName { get; set; } = String.Empty;
    
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    [JsonPropertyName("tracks")]
    public List<FrameData.Track> tracksList { get; set; } = new List<FrameData.Track>();

    public HttpTracksReport()
    {        
    }

    public HttpTracksReport(FrameData frameData)
    {
        this.DeviceId = frameData.DeviceId;
        this.DeviceName = frameData.DeviceName;
        this.Timestamp = frameData.Timestamp;
        this.tracksList = frameData.tracksList;
    }
}