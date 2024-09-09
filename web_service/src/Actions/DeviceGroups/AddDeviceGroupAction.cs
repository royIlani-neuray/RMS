/***
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using Serilog;
using System.Text.Json.Serialization;
using WebService.Context;
using WebService.Entites;

namespace WebService.Actions.DeviceGroups;

public class AddDeviceGroupArgs 
{
    [JsonPropertyName("group_name")]
    public string GroupName { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("location")]
    public string Location { get; set; }

    [JsonPropertyName("timezone")]
    public string TimeZone { get; set; }

    [JsonPropertyName("sub_groups_ids")]
    public List<string> SubGroupsIdList { get; set; }

    [JsonPropertyName("radar_ids")]
    public List<string> RadarIdList { get; set; }

    [JsonPropertyName("camera_ids")]
    public List<string> CameraIdList { get; set; }

    public AddDeviceGroupArgs()
    {
        GroupName = String.Empty;
        Description = String.Empty;
        Location = String.Empty;
        TimeZone = String.Empty;
        SubGroupsIdList = [];
        RadarIdList = [];
        CameraIdList = [];
    }

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(GroupName))
            throw new HttpRequestException("Group name not defined");
    }

}

public class AddDeviceGroupAction : IAction 
{
    AddDeviceGroupArgs args;

    public string DeviceGroupId { get; set; }

    public AddDeviceGroupAction(AddDeviceGroupArgs args)
    {
        DeviceGroupId = String.Empty;
        this.args = args;
    }
    public void Run()
    {
        args.Validate();

        var group = new DeviceGroup
        {
            GroupName = args.GroupName,
            Description = args.Description,
            Location = args.Location,
            TimeZone = args.TimeZone,
            CreatedAt = DateTime.UtcNow
        };

        foreach (var radarId in args.RadarIdList)
        {
            var radar = RadarContext.Instance.GetRadar(radarId); // throws exception if not exist.
            group.RadarIdList.Add(radarId);
        }

        foreach (var cameraId in args.CameraIdList)
        {
            var camera = CameraContext.Instance.GetCamera(cameraId); // throws exception if not exist.
            group.CameraIdList.Add(cameraId);
        }

        foreach (var subgroupId in args.SubGroupsIdList)
        {
            var subgroup = DeviceGroupContext.Instance.GetDeviceGroup(subgroupId); // throws exception if not exist.
            group.SubGroupsIdList.Add(subgroupId);
        }
        
        Log.Information($"Adding new device group - '{group.GroupName}'");
 
        DeviceGroupContext.Instance.AddDeviceGroup(group);

        Log.Information($"Device group '{group.GroupName}' added successfuly.");

        DeviceGroupId = group.Id;
    }
}