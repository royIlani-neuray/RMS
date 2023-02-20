/***
** Copyright (C) 2020-2023 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using WebService.Entites;

namespace WebService.Actions.Radar;

public class EnableRadarAction : RadarAction 
{
    public EnableRadarAction(string deviceId) : base(deviceId) {}

    protected override void RunRadarAction(RadarDevice radarDevice)
    {
        if (radarDevice.Enabled)
            return; // nothing to do.
        
        radarDevice.SetStatus("Enabling radar device...");
        radarDevice.Enabled = true;

        var connectAction = new ConnectRadarAction(radarDevice);
        connectAction.Run();
    }
}