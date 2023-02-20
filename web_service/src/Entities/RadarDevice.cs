/***
** Copyright (C) 2020-2023 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using System.Text.Json.Serialization;
using WebService.RadarLogic;
using WebService.Tracking;
using WebService.WebSockets;
using WebService.Services;
using WebService.Database;

namespace WebService.Entites;

public class RadarDevice : DeviceEntity {

    public class LinkedService
    {
        [JsonPropertyName("service_id")]
        public String ServiceId { get; set; } = String.Empty;

        [JsonPropertyName("service_options")]
        public Dictionary<string,string> ServiceOptions { get; set; } = new Dictionary<string, string>();

        [JsonIgnore]
        public IServiceContext? ServiceContext;
    }

    [JsonIgnore]
    public override IEntity.EntityTypes EntityType => IEntity.EntityTypes.Radar;
    
    [JsonIgnore]
    public override string StoragePath => StorageDatabase.RadarStoragePath;

    [JsonPropertyName("send_tracks_report")]
    public bool SendTracksReport { get; set; }

    [JsonPropertyName("config_script")]
    public List<string> ConfigScript { get; set; }

    [JsonPropertyName("device_mapping")]
    public DeviceMapper.MappedDevice? deviceMapping { get; set;}

    [JsonPropertyName("radar_settings")]
    public RadarSettings? radarSettings { get; set;}

    [JsonPropertyName("linked_services")]
    public List<LinkedService> LinkedServices { get; set; }

    [JsonIgnore]
    public IPRadarClient? ipRadarClient;

    [JsonIgnore]
    public RadarTracker? radarTracker;

    [JsonIgnore]
    public DeviceWebSocketServer DeviceWebSocket;

    public class RadarDeviceBrief : DeviceBrief
    {
        [JsonPropertyName("send_tracks_report")]
        public bool SendTracksReport { get; set; }

        public RadarDeviceBrief(RadarDevice device) : base(device)
        {
            SendTracksReport = device.SendTracksReport;
        }
    }

    public RadarDevice() : base(DeviceTypes.Radar)
    {
        ConfigScript = new List<string>();
        LinkedServices = new List<LinkedService>();
        DeviceWebSocket = new DeviceWebSocketServer();
    }

    public void SetStatus(string status)
    {
        this.Status = status;
        System.Console.WriteLine($"[{Id}] {status}");
    }
}