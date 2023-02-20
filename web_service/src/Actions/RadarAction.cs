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

public abstract class RadarAction : EntityAction<Radar>
{
    public RadarAction(string radarId) : base(RadarContext.Instance, radarId) {}

    protected abstract void RunRadarAction(Radar radarDevice);

    protected override void RunAction(Radar radarDevice)
    {
        RunRadarAction(radarDevice);
    }

    protected override void RunPostActionTask(Radar radarDevice)
    {
        RMSEvents.Instance.RadarDeviceUpdatedEvent(radarDevice.Id);
    }

}