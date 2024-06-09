/***
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using System.Text.Json.Serialization;
using WebService.Database;

namespace WebService.Entites;

public class RecordingSchedule : IEntity
{
    [JsonIgnore]
    public IEntity.EntityTypes EntityType => IEntity.EntityTypes.RecordingSchedule;

    [JsonIgnore]
    public string StoragePath => StorageDatabase.RecordingSchedulesStoragePath;

    [JsonPropertyName("id")]
    public String Id { get; set; }

    [JsonPropertyName("name")]
    public String Name { get; set; }

    [JsonPropertyName("enabled")]
    public Boolean Enabled { get; set; }

    [JsonPropertyName("radars")]
    public List<string> RadarIds { get; set; }

    [JsonPropertyName("cameras")]
    public List<string> CameraIds { get; set; }

    [JsonPropertyName("start_days")]
    public List<DayOfWeek> StartDays { get; set; }

    [JsonPropertyName("start_time")]
    public TimeOnly StartTime { get; set; }

    [JsonPropertyName("end_days")]
    public List<DayOfWeek> EndDays { get; set; }

    [JsonPropertyName("end_time")]
    public TimeOnly EndTime { get; set; }

    [JsonIgnore]
    public DateTime RegisteredAt { get; set; }

    [JsonPropertyName("last_start")]
    public DateTime LastStart { get; set; }

    [JsonIgnore]
    public DateTime LastEnd { get; set; }

    [JsonIgnore]
    public ReaderWriterLockSlim EntityLock { get; set; }
    public RecordingSchedule()
    {
        EntityLock = new ReaderWriterLockSlim();
        Id = Guid.NewGuid().ToString();
        Name = String.Empty;
        RadarIds = new List<String>();
        CameraIds = new List<String>();
        StartDays = new List<DayOfWeek>();
        EndDays = new List<DayOfWeek>();
        RegisteredAt = DateTime.UtcNow;
    }
}