/***
** Copyright (C) 2020-2023 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using WebService.Entites;

namespace WebService.Actions.Radars;

public class DisableRadarAction : RadarAction 
{
    public DisableRadarAction(string radarId) : base(radarId) {}

    protected override void RunRadarAction(Radar radar)
    {
        if (!radar.Enabled)
            return; // nothing to do.

        radar.SetStatus("Disabling radar device...");

        var disconnectAction = new DisconnectRadarAction(radar);
        disconnectAction.Run();

        radar.Enabled = false;
        radar.SetStatus("The device is disabled.");
    }
}