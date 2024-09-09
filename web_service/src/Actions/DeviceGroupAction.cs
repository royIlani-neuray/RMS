/***
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using WebService.Context;
using WebService.Entites;
using WebService.Events;

namespace WebService.Actions;

public abstract class DeviceGroupAction : EntityAction<DeviceGroup>
{
    public DeviceGroupAction(string userId) : base(DeviceGroupContext.Instance, userId) {}

    protected abstract void RunDeviceGroupAction(DeviceGroup deviceGroup);

    protected override void RunAction(DeviceGroup deviceGroup)
    {
        RunDeviceGroupAction(deviceGroup);
    }

    protected override void RunPostActionTask(DeviceGroup deviceGroup)
    {
        RMSEvents.Instance.DeviceGroupUpdatedEvent(deviceGroup.Id);
    }
}