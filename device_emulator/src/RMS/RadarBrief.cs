/***
** Copyright (C) 2020-2023 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using System.Text.Json.Serialization;

public class RadarBrief 
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

    public RadarBrief()
    {
        Name = String.Empty;
        Description = String.Empty;
        Id = String.Empty;
    }
}