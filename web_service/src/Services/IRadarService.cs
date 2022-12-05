/***
** Copyright (C) 2020-2023 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using WebService.Entites;
using WebService.Tracking;

namespace WebService.Services;

public interface IRadarService 
{
    public string ServiceId { get; }

    public RadarServiceSettings? Settings {get; set; }
    public IServiceContext CreateServiceContext(RadarDevice device, Dictionary<string,string> serviceOptions);

    public void DisposeServiceContext(IServiceContext serviceContext);

    public void HandleFrame(FrameData frame, IServiceContext serviceContext);
    
}