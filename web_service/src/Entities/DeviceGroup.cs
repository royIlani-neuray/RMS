/***
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using System.Text.Json.Serialization;
using WebService.Context;
using WebService.Database;

namespace WebService.Entites;


public class DeviceGroup : IEntity 
{
    [JsonIgnore]
    public IEntity.EntityTypes EntityType => IEntity.EntityTypes.DeviceGroup;

    [JsonIgnore]
    public string StoragePath => StorageDatabase.DeviceGroupStoragePath;

    [JsonPropertyName("group_id")]
    public string Id { get; }

    [JsonPropertyName("group_name")]
    public string GroupName { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("location")]
    public string Location { get; set; }

    [JsonPropertyName("timezone")]
    public string TimeZone { get; set; } // TZ identifier. see: https://en.wikipedia.org/wiki/List_of_tz_database_time_zones

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("sub_groups_ids")]
    public List<string> SubGroupsIdList { get; set; }

    [JsonPropertyName("radar_ids")]
    public List<string> RadarIdList { get; set; }

    [JsonPropertyName("camera_ids")]
    public List<string> CameraIdList { get; set; }

    [JsonIgnore]
    public List<DeviceGroup> SubGroups => SubGroupsIdList.ConvertAll(DeviceGroupContext.Instance.GetDeviceGroup);

    [JsonIgnore]
    public List<Radar> Radars {
        get {
            var radarList = RadarIdList.ConvertAll(RadarContext.Instance.GetRadar);
            SubGroups.ForEach(subgroup => radarList.AddRange(subgroup.Radars));
            return radarList;
        }
    }

    [JsonIgnore]
    public List<Camera> Cameras {
        get {
            var cameraList = CameraIdList.ConvertAll(CameraContext.Instance.GetCamera);
            SubGroups.ForEach(subgroup => cameraList.AddRange(subgroup.Cameras));
            return cameraList;
        }
    }

    [JsonIgnore]
    public ReaderWriterLockSlim EntityLock { get; set; }

    public class DeviceGroupBrief(DeviceGroup group)
    {
        [JsonPropertyName("group_id")]
        public string Id { get; } = group.Id;

        [JsonPropertyName("group_name")]
        public string GroupName { get; } = group.GroupName;

        [JsonPropertyName("description")]
        public string Description { get; } = group.Description;

        [JsonPropertyName("location")]
        public string Location { get; set; } = group.Location;
    }

    public DeviceGroup()
    {
        EntityLock = new ReaderWriterLockSlim();
        Id = Guid.NewGuid().ToString();
        GroupName = String.Empty;
        Location = String.Empty;
        TimeZone = String.Empty;
        Description = String.Empty;
        CreatedAt = DateTime.UtcNow;
        SubGroupsIdList = [];
        RadarIdList = [];
        CameraIdList = [];
    }

}