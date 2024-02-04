/***
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using System.Text.Json.Serialization;
using WebService.RadarLogic.IPRadar;
using WebService.RadarLogic.Tracking;
using WebService.WebSockets;
using WebService.Database;

namespace WebService.Entites;

public class Radar : DeviceEntity {

    [JsonIgnore]
    public override IEntity.EntityTypes EntityType => IEntity.EntityTypes.Radar;
    
    [JsonIgnore]
    public override string StoragePath => StorageDatabase.RadarStoragePath;

    [JsonPropertyName("send_tracks_report")]
    public bool SendTracksReport { get; set; }

    [JsonPropertyName("config_script")]
    public List<string> ConfigScript { get; set; }

    [JsonPropertyName("device_mapping")]
    public RadarDeviceMapper.MappedDevice? deviceMapping { get; set;}

    [JsonPropertyName("radar_settings")]
    public RadarSettings? radarSettings { get; set;}

    [JsonIgnore]
    public IPRadarClient? ipRadarClient;

    [JsonIgnore]
    public RadarTracker? radarTracker;

    [JsonIgnore]
    public RadarWebSocketServer RadarWebSocket;

    public class RadarBrief : DeviceBrief
    {
        [JsonPropertyName("send_tracks_report")]
        public bool SendTracksReport { get; set; }

        public RadarBrief(Radar device) : base(device)
        {
            SendTracksReport = device.SendTracksReport;
        }
    }

    public Radar() : base(DeviceTypes.Radar)
    {
        ConfigScript = new List<string>();
        RadarWebSocket = new RadarWebSocketServer();
    }
}