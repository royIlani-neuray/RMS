/***
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using WebService.Entites;
using WebService.Context;
using Microsoft.AspNetCore.Mvc;
using WebService.Actions.DeviceGroups;

namespace WebService.Controllers;

[ApiController]
[Route("device-groups")]
public class DeviceGroupController : ControllerBase
{
    private void ValidateDeviceGroupId(string groupId)
    {
        if (string.IsNullOrWhiteSpace(groupId) || !Guid.TryParse(groupId, out _))
            throw new BadRequestException("invalid device group id provided.");
    }

    [HttpGet]
    public List<DeviceGroup.DeviceGroupBrief> GetDeviceGroups()
    {
        return DeviceGroupContext.Instance.GetDeviceGroupsBrief();
    }

    [HttpGet("{groupId}")]
    public DeviceGroup GetDeviceGroup(string groupId)
    {
        ValidateDeviceGroupId(groupId);        
        if (!DeviceGroupContext.Instance.IsGroupExist(groupId))
            throw new NotFoundException("There is no device group with the provided id");

        return DeviceGroupContext.Instance.GetDeviceGroup(groupId);
    }

    [HttpPost]
    public object AddDeviceGroup([FromBody] AddDeviceGroupArgs args)
    {
        var action = new AddDeviceGroupAction(args);
        action.Run();
        return new { group_id = action.DeviceGroupId };
    }

    [HttpDelete("{groupId}")]
    public void DeleteDeviceGroup(string groupId)
    {        
        ValidateDeviceGroupId(groupId); 
        var action = new DeleteDeviceGroupAction(groupId);
        action.Run();
    }

    [HttpPost("{groupId}/enable-devices")]
    public void EnableDevices(string groupId)
    {
        ValidateDeviceGroupId(groupId); 
        var action = new EnableDevicesAction(groupId);
        action.Run();
    }

    [HttpPost("{groupId}/disable-devices")]
    public void DisableDevices(string groupId)
    {
        ValidateDeviceGroupId(groupId); 
        var action = new DisableDevicesAction(groupId);
        action.Run();
    }

}