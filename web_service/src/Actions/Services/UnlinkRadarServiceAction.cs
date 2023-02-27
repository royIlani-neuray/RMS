/***
** Copyright (C) 2020-2023 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using WebService.Entites;
using WebService.Services;

namespace WebService.Actions.Services;

public class UnlinkRadarServiceAction : RadarAction 
{
    private string serviceId;

    public UnlinkRadarServiceAction(string radarId, string serviceId) : base(radarId) 
    {
        this.serviceId = serviceId;
    }

    protected override void RunRadarAction(Radar radar)
    {
        var unlinkServiceAction = new UnlinkServiceAction(radar, serviceId);
        unlinkServiceAction.Run();
    }
}