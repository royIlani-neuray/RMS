/***
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using System.Text.Json.Serialization;
using WebService.Events;
using WebService.Services;
using Serilog;
using Amazon.S3.Model;

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

    public class LinkedService
    {
        [JsonPropertyName("service_id")]
        public String ServiceId { get; set; } = String.Empty;

        [JsonPropertyName("service_options")]
        public Dictionary<string,string> ServiceOptions { get; set; } = new Dictionary<string, string>();

        [JsonIgnore]
        public IServiceContext? ServiceContext;
    }

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

    private String deviceId = String.Empty;

    [JsonPropertyName("device_id")]
    public String Id 
    { 
        get => deviceId; 
        set
        {
            deviceId = value;
            InitDeviceLogger();
        } 
    }

    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; }

    [JsonPropertyName("linked_services")]
    public List<LinkedService> LinkedServices { get; set; }

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
        Status = String.Empty;
        Enabled = false;
        EntityLock = new ReaderWriterLockSlim();
        LinkedServices = new List<LinkedService>();
        Log = Serilog.Log.Logger;
    }

    [JsonIgnore]
    public Serilog.ILogger Log;

    private void InitDeviceLogger()
    {
        var loggerConfig = new LoggerConfiguration()
            .Enrich.WithProperty("DeviceTag", $"[{Type} - {Id}] ")
            .WriteTo.File(path: $"./data/logs/{Type.ToString().ToLower()}/{Id}.log",    // Dedicated file for this logger
                          outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message}{NewLine}{Exception}",
                          fileSizeLimitBytes: 10485760,
                          restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Debug)  
            .WriteTo.Logger(Serilog.Log.Logger); // This forwards to the global logger
        
        if (Serilog.Log.Logger.IsEnabled(Serilog.Events.LogEventLevel.Verbose))
        {
            loggerConfig = loggerConfig.MinimumLevel.Verbose();
        }
        else if (Serilog.Log.Logger.IsEnabled(Serilog.Events.LogEventLevel.Debug))
        {
            loggerConfig = loggerConfig.MinimumLevel.Debug();
        }

        Log = loggerConfig.CreateLogger();
    }

    public void SetStatus(string status)
    {
        this.Status = status;
        this.Log.Information(status);

        if (Type == DeviceTypes.Radar)
        {
            RMSEvents.Instance.RadarUpdatedEvent(Id);
        }
        else if (Type == DeviceTypes.Camera)
        {
            RMSEvents.Instance.CameraUpdatedEvent(Id);
        }
    }
}