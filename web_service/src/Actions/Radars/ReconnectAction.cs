/***
** Copyright (C) 2020-2023 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using WebService.Entites;
using WebService.RadarLogic;

namespace WebService.Actions.Radars;

public class ReconnectAction : RadarAction 
{
    public ReconnectAction(string deviceId) : base(deviceId) {}

    protected override void RunRadarAction(RadarDevice radarDevice)
    {
        if (!radarDevice.Enabled)
            return;

        if (radarDevice.State != RadarDevice.DeviceState.Disconnected)
            return;

        var connectAction = new ConnectRadarAction(radarDevice);
        connectAction.Run();
    }
}