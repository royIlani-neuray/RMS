/***
** Copyright (C) 2020-2023 neuRay Labs. All rights reserved.
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

public abstract class RadarAction : EntityAction<RadarDevice>
{
    public RadarAction(string deviceId) : base(RadarContext.Instance, deviceId) {}

    protected abstract void RunRadarAction(RadarDevice radarDevice);

    protected override void RunAction(RadarDevice radarDevice)
    {
        RunRadarAction(radarDevice);
    }

    protected override void RunPostActionTask(RadarDevice radarDevice)
    {
        RMSEvents.Instance.RadarDeviceUpdatedEvent(radarDevice.Id);
    }

}