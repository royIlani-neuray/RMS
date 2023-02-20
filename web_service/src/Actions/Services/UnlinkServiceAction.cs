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

public class UnlinkServiceAction : RadarAction 
{
    private string serviceId;

    public UnlinkServiceAction(string radarId, string serviceId) : base(radarId) 
    {
        this.serviceId = serviceId;
    }

    protected override void RunRadarAction(Radar radar)
    {
        var linkedService = radar.LinkedServices.FirstOrDefault(linkedService => linkedService.ServiceId == serviceId);

        if (linkedService == null)
            throw new Exception($"Could not find linked service with id - {serviceId}");
        
        ServiceManager.Instance.DisposeServiceContext(radar.Id, linkedService);
        radar.LinkedServices.Remove(linkedService);
    }
}