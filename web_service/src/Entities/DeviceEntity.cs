/***
** Copyright (C) 2020-2023 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using System.Text.Json.Serialization;

namespace WebService.Entites;

public abstract class DeviceEntity : IEntity {

    public abstract IEntity.EntityTypes EntityType { get; }
    public abstract string StoragePath { get; }

    public enum DeviceTypes {
        Radar,
        Camera
    }

    public enum DeviceState {
        Disconnected,
        Connected,
        Active
    };

    [JsonPropertyName("type")]

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public DeviceTypes Type { get; set; }

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

    [JsonIgnore]
    public ReaderWriterLockSlim EntityLock { get; set; }

    public abstract class DeviceBrief 
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

        public DeviceBrief(DeviceEntity device)
        {
            State = device.State;
            Name = device.Name;
            Description = device.Description;
            Id = device.Id;
            Enabled = device.Enabled;
        }
    }

    public DeviceEntity(DeviceTypes deviceType)
    {
        Type = deviceType;
        State = DeviceState.Disconnected;
        Name = String.Empty;
        Description = String.Empty;
        Id = String.Empty;
        Status = String.Empty;
        Enabled = false;
        EntityLock = new ReaderWriterLockSlim();
    }

    public void SetStatus(string status)
    {
        this.Status = status;
        System.Console.WriteLine($"[{Type} - {Id}] {status}");
    }
}