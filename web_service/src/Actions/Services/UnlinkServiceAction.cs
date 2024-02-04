/***
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using WebService.Entites;
using WebService.Services;

namespace WebService.Actions.Services;

public class UnlinkServiceAction : IAction
{
    private string serviceId;
    private DeviceEntity device;
    
    public UnlinkServiceAction(DeviceEntity device, string serviceId)
    {
        this.device = device;
        this.serviceId = serviceId;
    }

    public void Run()
    {
        var linkedService = device.LinkedServices.FirstOrDefault(linkedService => linkedService.ServiceId == serviceId);

        if (linkedService == null)
            throw new Exception($"Could not find linked service with id - {serviceId}");
        
        ServiceManager.Instance.DisposeServiceContext(device.Id, linkedService);
        device.LinkedServices.Remove(linkedService);
    }
}